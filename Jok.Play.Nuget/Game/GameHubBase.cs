using Jok.GameEngine.Models;
using Jok.Play;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jok.GameEngine
{
    public abstract class GameHubBase<TTable> : GameHubBase<GameHubConnection<TTable>, TTable>
    where TTable : class, IGameTable, new()
    {
    }

    public abstract class GameHubBase<TConnection, TTable> : Hub, IGameHub
        where TConnection : GameHubConnection<TTable>, new()
        where TTable : class, IGameTable, new()
    {
        public static ConcurrentDictionary<string, TConnection> Connections { get; set; }
        public static List<TTable> Tables { get; set; }
        public static object TablesSyncObject = new object();

        public static bool IsTournamentChannel(string channel) 
        {
            if (String.IsNullOrWhiteSpace(channel)) return false;

            return channel.ToLower().StartsWith("tournament");
        }


        static GameHubBase()
        {
            try
            {
                JokAPI.GameID = Convert.ToInt32(ConfigurationManager.AppSettings["Jok:GameID"]);
                JokAPI.GameSecret = ConfigurationManager.AppSettings["Jok:GameSecret"];
            }
            catch { Debug.WriteLine("Jok:GameID or Jok:GameSecret not found in web.config"); }


            Connections = new ConcurrentDictionary<string, TConnection>();
            Tables = new List<TTable>();
        }


        public int ConnectionsCount
        {
            get
            {
                return Connections.Count;
            }
        }

        public int TablesCount
        {
            get
            {
                return Tables.Count;
            }
        }



        protected virtual bool IsTablesEnabled
        {
            get { return true; }
        }

        protected virtual bool OneConnectionPerUserID
        {
            get { return true; }
        }


        public override Task OnConnected()
        {
            var token = Context.QueryString["token"];
            var ipaddress = GetIpAddress();
            var connectionID = Context.ConnectionId;
            var channel = Context.QueryString["channel"] ?? String.Empty;

            base.OnConnected().Wait();

            ConnectedEvent(token, ipaddress, channel, connectionID);

            return null;
        }

        public override Task OnReconnected()
        {
            var token = Context.QueryString["token"];
            var ipaddress = GetIpAddress();
            var connectionID = Context.ConnectionId;
            var channel = Context.QueryString["channel"] ?? String.Empty;


            base.OnReconnected().Wait();

            // თუ პამეხების გამო შიდა რეკონექტი იყო, არ სჭირდება მაშინ აქ არაფრის გაკეთება
            // მემორიში საჭირო ინფო არის ისედაც
            TConnection user;
            if (Connections.TryGetValue(connectionID, out user))
                return null;

            ConnectedEvent(token, ipaddress, channel, connectionID);

            return null;
        }

        public override Task OnDisconnected(bool isStopped)
        {
            var connectionID = Context.ConnectionId;

            base.OnDisconnected().Wait();

            TConnection user;
            if (!Connections.TryRemove(connectionID, out user))
                return null;

            if (IsTablesEnabled)
            {
                user.Table.Leave(user.UserID, connectionID);

                if (user.Table.PlayersCount == 0 || user.Table.OnlinePlayersCount == 0)
                    lock (TablesSyncObject)
                    {
                        Tables.Remove(user.Table);
                    }

                user.Table = null;
            }

            return null;
        }


        protected virtual void ConnectedEvent(string token, string ipaddress, string channel, string connectionID)
        {
            try
            {
                if (String.IsNullOrEmpty(token))
                {
                    this.Clients.Caller.Close("Token not provided");
                    return;
                }

                var userInfo = GetUserInfo(token, ipaddress);

                if (userInfo.IsSuccess != true)
                {
                    this.Clients.Caller.Close("User info not found for token: " + token);
                    return;
                }

                var userid = userInfo.UserID;
                var isVIP = userInfo.IsVIP;


                var conn = new TConnection
                {
                    UserID = userid,
                    Channel = channel,
                    IPAddress = ipaddress,
                    IsVIP = isVIP,
                    CultureName = userInfo.CultureName
                };

                Groups.Add(connectionID, userid.ToString()).Wait();

                // თუ მხოლოდ ერთი კონექშენია დაშვებული, დანარჩენებს ვთიშავთ
                if (OneConnectionPerUserID)
                {
                    Clients.Group(conn.UserID.ToString(), connectionID).Close("Another connection is open");
                }

                // ვუგზავნით მომხმარებელს ინფორმაციას თუ რომელი იუზერია
                Clients.Caller.UserAuthenticated(userid, true);
                // ხოლო თუ კიდე არის სხვა მოერთებული იგივე იუზერ აიდით, იმასაც ვუგზავნით ინფოს რომ მოერთდა ვიღაც კიდე
                if (!OneConnectionPerUserID)
                    Clients.Group(conn.UserID.ToString(), connectionID).UserAuthenticated(userid, false);

                if (!Connections.TryAdd(connectionID, conn))
                {
                    this.Clients.Caller.Close("User object add to Users collection failed");
                    return;
                }

                if (IsTablesEnabled)
                {
                    conn.Table = GetTable(conn);

                    if (conn.Table == null)
                    {
                        this.Clients.Caller.Close("User can't join table. User.Table is null");
                        return;
                    }

                    //await Groups.Add(connectionID, user.Table.ID.ToString());

                    conn.Table.Join(userid, connectionID, ipaddress, isVIP, conn);
                }
            }
            catch (Exception ex)
            {
                this.Clients.Caller.Close(ex.ToString());
            }
        }

        protected virtual JokUserInfo GetUserInfo(string token, string ipaddress)
        {
            return JokAPI.GetUser(token, ipaddress);
        }

        private TTable GetTable(TConnection conn)
        {
            var table = default(TTable);

            lock (TablesSyncObject)
            {
                table = FindTable(conn);
                if (table == default(TTable))
                {
                    table = CreateTable(conn);
                    if (table != null)
                        Tables.Add(table);
                }
            }

            return table;
        }

        protected virtual TTable FindTable(TConnection conn)
        {
            var table = default(TTable);

            // in started_waiting tables
            table = Tables.FirstOrDefault(t => t.UserIDs.Contains(conn.UserID) && t.IsStarted && !t.IsFinished);

            if (table != default(TTable)) return table;


            // find in existing tables
            table = Tables.FirstOrDefault(t =>
                t.Channel == conn.Channel &&
                t.PlayersCount < t.MaxPlayersCount &&
                !t.IsStarted &&
                !t.IsFinished &&
                (!IsTournamentChannel(conn.Channel) || (conn.IsVIP == t.IsVIPTable && TournamentChannelValidations(t, conn.IPAddress))));

            return table;
        }

        protected virtual bool TournamentChannelValidations(TTable table, string ipaddress)
        {
            if (table.PlayersCount == table.MaxPlayersCount)
                return false;

            // ყველა ip უნდა იყოს განსხვავებული
            if (table.IPAddresses.Contains(ipaddress))
                return false;

            if (table.MaxPlayersCount != 2 || table.PlayersCount != 1 || table.IPAddresses.Count != 1)
                return true;

            var now = DateTime.Now;

            return JokSharedInfo.IPsLog.Count(l => (now - l.CreateDate < TimeSpan.FromHours(2)) && (
                (l.IP1 == table.IPAddresses[0] && l.IP2 == ipaddress) ||
                (l.IP2 == table.IPAddresses[0] && l.IP1 == ipaddress))) == 0;
        }

        protected virtual TTable CreateTable(TConnection conn)
        {
            return new TTable
            {
                ID = Guid.NewGuid(),
                Channel = conn.Channel,
                IsVIPTable = IsTournamentChannel(conn.Channel) ? (bool?)conn.IsVIP : null
            };
        }

        protected TConnection GetCurrentUser()
        {
            TConnection user;
            if (Connections.TryGetValue(Context.ConnectionId, out user))
                return user;

            return null;
        }


        protected string GetIpAddress()
        {
            var env = Get<IDictionary<string, object>>(Context.Request.Items, "owin.environment");
            if (env == null)
            {
                return null;
            }
            var ipAddress = Get<string>(env, "server.RemoteIpAddress");
            return ipAddress;
        }

    }


    public class GameHubConnection<TTable>
    {
        public int UserID { get; set; }
        public string Channel { get; set; }
        public string IPAddress { get; set; }
        public bool IsVIP { get; set; }
        public string CultureName { get; set; }
        public TTable Table { get; set; }
    }

}

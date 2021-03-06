﻿using Jok.Play.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Jok.Play
{
    [DataContract]
    public abstract class GameTableBase<TGamePlayer> : IGameTable
        where TGamePlayer : class, IGamePlayer, new()
    {
        #region IGameTable
        [DataMember]
        public virtual Guid ID { get; set; }
        [DataMember]
        public virtual string Channel { get; set; }
        [DataMember]
        public virtual int Mode { get; set; }
        [IgnoreDataMember]
        public List<string> IPAddresses { get; set; }
        [IgnoreDataMember]
        public virtual int MaxPlayersCount
        {
            get { return 2; }
        }
        [IgnoreDataMember]
        public int PlayersCount
        {
            get
            {
                lock (SyncObject)
                {
                    return Players.Count;
                }
            }
        }
        [IgnoreDataMember]
        public int OnlinePlayersCount
        {
            get
            {
                lock (SyncObject)
                {
                    return Players.Count(p => p.IsOnline);
                }
            }
        }
        [IgnoreDataMember]
        public abstract bool IsStarted { get; }
        [IgnoreDataMember]
        public abstract bool IsFinished { get; }
        [IgnoreDataMember]
        public List<string> ConnectionIDs { get; set; }
        [IgnoreDataMember]
        public List<int> UserIDs { get; set; }
        [IgnoreDataMember]
        public virtual bool? IsVIPTable { get; set; }
        [DataMember]
        public virtual bool IsDeleteAllowed
        {
            get
            {
                return PlayersCount == 0 || OnlinePlayersCount == 0;
            }
        }
        [DataMember]
        public virtual DateTime CreateDate { get; private set; }

        #endregion

        protected GameTableBase<TGamePlayer> Table
        {
            get { return this; }
        }

        [DataMember]
        public virtual List<TGamePlayer> Players { get; set; }
        [DataMember]
        public virtual TGamePlayer ActivePlayer { get; set; }

        protected object SyncObject = new object();

        public GameTableBase()
        {
            IPAddresses = new List<string>();
            ConnectionIDs = new List<string>();
            UserIDs = new List<int>();
            Players = new List<TGamePlayer>();
            ActivePlayer = null;
            CreateDate = DateTime.Now;
        }


        public void Join(int userid, string connectionID, string ipaddress, bool isVIP, object state = null, string userInfoResponseString = "")
        {
            lock (SyncObject)
            {
                var player = GetPlayer(userid);
                if (player == null)
                    player = new TGamePlayer { UserID = userid, IPAddress = ipaddress, IsVIP = isVIP };

                player.ConnectionIDs = new List<string>();
                player.ConnectionIDs.Add(connectionID);
                player.IsOnline = true;

                if (!IsStarted && !IsFinished)
                    AddPlayer(player);

                RefreshIPAddressesAndUserIDs();

                OnJoin(player, state, userInfoResponseString);

                JokAPI.PlayerLogin(player.UserID, player.IPAddress);

                RefreshIPAddressesAndUserIDs();
            }
        }

        public void Leave(int userid, string connectionID)
        {
            lock (SyncObject)
            {
                var player = GetPlayer(userid);
                if (player == null) return;
                if (!player.ConnectionIDs.Contains(connectionID)) return;

                if (!IsStarted)
                    RemovePlayer(player);

                player.IsOnline = false;
                OnLeave(player);

                JokAPI.PlayerLogout(player.UserID);

                RefreshIPAddressesAndUserIDs();
            }
        }


        protected abstract void OnJoin(TGamePlayer player, object state, string userInfoResponseString);

        protected abstract void OnLeave(TGamePlayer player);


        void AddPlayer(TGamePlayer player)
        {
            if (player == null) return;

            if (ActivePlayer != null && ActivePlayer.UserID == player.UserID)
                ActivePlayer = player;

            if (Players.Contains(player)) return;

            Players.Add(player);
            RefreshIPAddressesAndUserIDs();
        }

        void RemovePlayer(TGamePlayer player)
        {
            if (player == null) return;

            if (!Players.Contains(player)) return;

            Players.Remove(player);
            RefreshIPAddressesAndUserIDs();
        }

        void RefreshIPAddressesAndUserIDs()
        {
            IPAddresses.Clear();
            ConnectionIDs.Clear();
            UserIDs.Clear();

            Players.ForEach(p =>
            {
                if (!IPAddresses.Contains(p.IPAddress))
                    IPAddresses.Add(p.IPAddress);

                if (!UserIDs.Contains(p.UserID))
                    UserIDs.Add(p.UserID);

                p.ConnectionIDs.ForEach(c =>
                {
                    if (!ConnectionIDs.Contains(c))
                        ConnectionIDs.Add(c);
                });
            });
        }

        protected void RemoveOfflinePlayers()
        {
            Players.RemoveAll(p => !p.IsOnline);
            RefreshIPAddressesAndUserIDs();
        }


        protected TGamePlayer GetPlayer(int userid)
        {
            return Players.FirstOrDefault(p => p.UserID == userid);
        }

        protected TGamePlayer GetNextPlayer(TGamePlayer player = default(TGamePlayer))
        {
            if (Players.Count <= 1) return null;

            if (player == default(TGamePlayer))
                player = ActivePlayer;

            if (player == default(TGamePlayer)) return null;

            player = GetPlayer(player.UserID);

            var index = Players.IndexOf(player);
            if (index == -1) return null;

            return Players[index < Players.Count - 1 ? ++index : 0];
        }

        protected void SaveIPAddressesLog()
        {
            lock (SyncObject)
            {
                if (Players.Count != 2) return;

                JokSharedInfo.IPsLog.Add(new IPAddressesLog
                {
                    IP1 = Players[0].IPAddress,
                    IP2 = Players[1].IPAddress,
                    CreateDate = DateTime.Now
                });

                JokSharedInfo.IPsLog.RemoveAll(p => p.CreateDate < DateTime.Now.AddHours(-5));
            }
        }

        protected void ProcessException(Exception ex, EventLogEntryType type = EventLogEntryType.Error)
        {
            var tableJson = String.Empty;
            try
            {
                tableJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            }
            catch { }

            if (type == EventLogEntryType.Error)
                Startup.AddError();

            var errorString = String.Format("Error:{0}{1}{0}{0}TableInfo:{0}{2}{0}{0}CreateTime:{0}{3}{0}{0}", Environment.NewLine, ex.ToString(), tableJson, DateTime.Now);

            EventLog.WriteEntry(Startup.ApplicationName, errorString, type);
        }
    }
}

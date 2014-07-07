using Jok.Play.Models;
using System;
using System.Collections.Generic;
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
        public Guid ID { get; set; }
        [DataMember]
        public string Channel { get; set; }
        [DataMember]
        public int Mode { get; set; }
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
        public bool? IsVIPTable { get; set; }
        [DataMember]
        public virtual bool IsDeleteAllowed
        {
            get
            {
                return PlayersCount == 0 || OnlinePlayersCount == 0;
            }
        }
        [DataMember]
        public DateTime CreateDate { get; private set; }

        #endregion

        protected GameTableBase<TGamePlayer> Table
        {
            get { return this; }
        }

        [DataMember]
        public List<TGamePlayer> Players = new List<TGamePlayer>();
        [DataMember]
        public TGamePlayer ActivePlayer;

        protected object SyncObject = new object();

        public GameTableBase()
        {
            IPAddresses = new List<string>();
            ConnectionIDs = new List<string>();
            UserIDs = new List<int>();
            ActivePlayer = null;
            CreateDate = DateTime.Now;
        }


        public void Join(int userid, string connectionID, string ipaddress, bool isVIP, object state = null)
        {
            lock (SyncObject)
            {
                var player = GetPlayer(userid);
                if (player == null)
                    player = new TGamePlayer { UserID = userid, IPAddress = ipaddress, IsVIP = isVIP };

                player.ConnectionIDs = new List<string>();
                player.ConnectionIDs.Add(connectionID);
                player.IsOnline = true;

                RefreshIPAddressesAndUserIDs();

                OnJoin(player, state);

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

                player.IsOnline = false;
                OnLeave(player);

                RefreshIPAddressesAndUserIDs();
            }
        }


        protected abstract void OnJoin(TGamePlayer player, object state);

        protected abstract void OnLeave(TGamePlayer player);


        protected void AddPlayer(TGamePlayer player)
        {
            if (Players.Contains(player)) return;

            Players.Add(player);
            RefreshIPAddressesAndUserIDs();
        }

        protected void RemovePlayer(TGamePlayer player)
        {
            if (!Players.Contains(player)) return;

            Players.Remove(player);
            RefreshIPAddressesAndUserIDs();
        }

        protected void RefreshIPAddressesAndUserIDs()
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
    }


}

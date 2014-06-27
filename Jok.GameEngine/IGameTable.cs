﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jok.GameEngine
{
    public interface IGameTable : ICallback
    {
        Guid ID { get; set; }
        int MaxPlayersCount { get; }
        int PlayersCount { get; }
        int OnlinePlayersCount { get; }
        string Channel { get; set; }
        bool IsStarted { get; }
        bool IsFinished { get; }
        List<string> IPAddresses { get; }
        List<int> UserIDs { get; }
        bool? IsVIPTable { get; set; }

        void Join(int userID, string connectionID, string ipaddress, bool isVIP, object state = null);
        void Leave(int userID, string connectionID);
    }
}

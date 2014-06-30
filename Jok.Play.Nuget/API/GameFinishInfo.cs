using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jok.Play.Models
{
    /// <summary>
    /// Finished game information
    /// </summary>
    public class GameFinishInfo
    {
        /// <summary>
        /// Game ID - identificator
        /// </summary>
        public int GameID { get; set; }
        /// <summary>
        /// Game secret key
        /// </summary>
        public string GameSecret { get; set; }
        /// <summary>
        /// Game table channel, where players has played
        /// </summary>
        public string Channel { get; set; }
        /// <summary>
        /// Players information
        /// </summary>
        public List<GameFinishPlayerInfo> Players { get; set; }

        /// <summary>
        /// [OPTIONAL] Was full game or not, if wasn't ratings will not be affected to winner
        /// </summary>
        public bool? IsFull { get; set; }
        /// <summary>
        /// [OPTIONAL] If set, players places are ignored
        /// </summary>
        public bool? IsDraw { get; set; }
        /// <summary>
        /// [OPTIONAL] Information about finished game, to make finish picture back
        /// </summary>
        public string Results { get; set; }
    }
}

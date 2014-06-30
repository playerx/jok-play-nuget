using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jok.Play.Models
{
    /// <summary>
    /// Finished game player information
    /// </summary>
    public class GameFinishPlayerInfo
    {
        /// <summary>
        /// Jok User ID - identificator
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// User ip address, from where was playing
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// Was online or not, when game finished
        /// </summary>
        public bool IsOnline { get; set; }
        /// <summary>
        /// Points gained in this game
        /// </summary>
        public int Points { get; set; }
        /// <summary>
        /// [OPTIONAL] Players place after game finish, if not set it will be calculated using points
        /// </summary>
        public int? Place { get; set; }
    }
}

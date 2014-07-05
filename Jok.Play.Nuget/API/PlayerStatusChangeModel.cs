using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jok.Play
{
    public class PlayerStatusChangeModel
    {
        public int? GameID { get; set; }

        public string GameSecret { get; set; }

        public int UserID { get; set; }

        public string IPAddress { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jok.GameEngine
{
    public interface IGamePlayer : ICallback
    {
        int UserID { get; set; }
        string IPAddress { get; set; }
        bool IsVIP { get; set; }
        bool IsOnline { get; set; }
    }
}

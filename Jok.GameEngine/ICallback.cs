using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jok.GameEngine
{
    public interface ICallback
    {
        List<string> ConnectionIDs { get; set; }
    }
}

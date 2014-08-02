using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jok.Play
{
    public class CheckTextResult
    {
        public bool IsSuccess { get; set; }
        public int? BanDays { get; set; }
        public string Error { get; set; }
    }
}

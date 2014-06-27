using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Jok.GameEngine.Models
{
    public class GameFinishResult
    {
        public bool? IsSuccess { get; set; }
        public string Message { get; set; }

        public List<Item> Result { get; set; }

        public string Details { get; set; }

        public class Item
        {
            public int? UserID { get; set; }
            public decimal? AddedRating { get; set; }
            public decimal? NewRating { get; set; }
            public int? AddedPoints { get; set; }
            public int? Place { get; set; }
            public bool? IsProvisionalRating { get; set; }
            public bool? IsProvisioningChanged { get; set; }
        }

        [IgnoreDataMember]
        public Exception Error { get; set; }
    }
}

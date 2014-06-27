using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jok.GameEngine.Models
{
    public class JokUserInfo
    {
        public bool? IsSuccess { get; set; }
        public string Reason { get; set; }
        public int UserID { get; set; }
        public string Nick { get; set; }
        public bool IsVIP { get; set; }
        public int? GenderID { get; set; }
        public int? LanguageID { get; set; }

        public bool IsMale
        {
            get { return GenderID == 1; }
        }
        public bool IsFemale
        {
            get { return GenderID == 2; }
        }
        public string CultureName
        {
            get
            {
                switch(LanguageID)
                {
                    case 1: return "ka-GE";
                    case 2: return "en-US";
                    case 3: return "ru-RU";
                    default: return String.Empty;
                }
            }
        }

    }
}

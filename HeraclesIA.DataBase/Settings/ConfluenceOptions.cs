using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.DataBase.Settings
{
    public sealed class ConfluenceOptions
    {
        public string BaseUrl { get; set; } = "";      
        public string ConfluenceUrl { get; set; } = "";
        public string ApiUrl { get; set; } = "";
        public string SpaceKey { get; set; } = "PH";
        public string Username { get; set; } = "";
        public string ApiToken { get; set; } = "";
    }
}

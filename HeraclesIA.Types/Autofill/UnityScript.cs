using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Types.Autofill
{
    public sealed class UnityScript
    {
        public int unityprojectnum { get; set; }
        public string unityprojectname { get; set; } = string.Empty;
        public string unityprojectdesc { get; set; } = string.Empty;
        public DateTime lastmodified { get; set; }
        public int usernum { get; set; }
        public int currentversion { get; set; }
        public int unitysourcenum { get; set; }
        public int sourceversionnum { get; set; }
        public int seqnum { get; set; }
        public string unitysourcetext { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Types.Autofill
{
    public sealed class AutoFill
    {
        public AutoFill()
        {
            Keywords = new List<Keyword>();
            UnityScripts = new List<UnityScript>();
        }

        public int AutofillId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int UnityScriptId { get; set; }
        public int JiraParentId { get; set; }
        public bool Estatus { get; set; }

        // En tu refactor usabas UnityScripts: eso aquí sí existe
        public List<UnityScript> UnityScripts { get; set; }

        public List<Keyword> Keywords { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Types.Autofill
{
    public sealed class Keyword
    {
        public int KeywordNum { get; set; }
        public string KeywordName { get; set; } = string.Empty;
        public string KeywordTipoDato { get; set; } = string.Empty;
        public int KeywordLongitud { get; set; }
    }
}

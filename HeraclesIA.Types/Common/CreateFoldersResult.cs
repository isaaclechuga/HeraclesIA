using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Types.Common
{
    public sealed class CreateFoldersResult
    {
        public int Creados { get; set; }
        public int YaExistian { get; set; }
        public int Omitidos { get; set; }
        public int ActualizadosEnDb { get; set; }
        public int Errores { get; set; }
    }
}

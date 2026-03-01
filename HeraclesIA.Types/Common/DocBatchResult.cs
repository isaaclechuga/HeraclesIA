using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Types.Common
{
    public sealed class DocBatchResult
    {
        public int Considerados { get; set; }
        public int Nuevos { get; set; }
        public int Existentes { get; set; }

        public int Publicados { get; set; }
        public int PublicacionOmitida { get; set; }

        public int SinCambios { get; set; }

        public int Errores { get; set; }
        public int ErroresSideEffects { get; set; }

        public override string ToString()
            => $"Considerados={Considerados}, Nuevos={Nuevos}, Existentes={Existentes}, " +
               $"Publicados={Publicados}, PublicacionOmitida={PublicacionOmitida}, SinCambios={SinCambios}, " +
               $"Errores={Errores}, ErroresSideEffects={ErroresSideEffects}";
    }
}

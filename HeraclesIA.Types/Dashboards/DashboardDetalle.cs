using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Types.Dashboards
{
    public class DashboardDetalle
    {
        public int DashboardId { get; set; }
        public int DataProviderId { get; set; }
        public string Servidor { get; set; } = string.Empty;
        public string BaseDatos { get; set; } = string.Empty;
        public string Tabla { get; set; } = string.Empty;
    }
}

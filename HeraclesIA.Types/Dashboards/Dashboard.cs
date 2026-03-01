using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Types.Dashboards
{
    public class Dashboard
    {
        public int DashboardNum { get; set; }
        public string DashboardName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public List<DataProvider> DataProviders { get; set; } = new();
    }
}

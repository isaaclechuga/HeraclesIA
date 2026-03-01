using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.DataBase.Settings
{
    public sealed class SqlSettings
    {
        public string NautilusConnectionString { get; set; } = string.Empty;
        public string CatalogosConnectionString { get; set; } = string.Empty;
        public string OperacionesConnectionString { get; set; } = string.Empty;
    }
}

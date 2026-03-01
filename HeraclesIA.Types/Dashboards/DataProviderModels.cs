using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeraclesIA.Types.Prompts;

namespace HeraclesIA.Types.Dashboards
{
    public class DataProvider
    {
        public int DataProviderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HelpText { get; set; } = string.Empty;
        public string DataSource { get; set; } = string.Empty;

        public Query? Query { get; set; }
    }

    public class Query
    {
        public PromptType Tipo { get; set; }
        public string SqlQuery { get; set; } = string.Empty;

        public List<Query> Children { get; set; } = new();
        public List<Servidor> Servidores { get; set; } = new();

        public Query() { }

        public Query(PromptType tipo, string sqlQuery)
        {
            Tipo = tipo;
            SqlQuery = sqlQuery;
        }
    }

    public class Servidor
    {
        public string Nombre { get; set; } = string.Empty;
        public List<BaseDatos> BaseDatos { get; set; } = new();
    }

    public class BaseDatos
    {
        public string Nombre { get; set; } = string.Empty;
        public List<string> Tablas { get; set; } = new();
    }

    public class Temp
    {
        public int DashboardNum { get; set; }
        public string DashboardName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int RptProviderNum { get; set; }
        public string RptDataProviderName { get; set; } = string.Empty;
        public string RptDataProviderHelpText { get; set; } = string.Empty;

        public string OdbcDataSource { get; set; } = string.Empty;
        public int SeqNum { get; set; }
        public string SqlQuery { get; set; } = string.Empty;

        public string CategoryNum { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }
}

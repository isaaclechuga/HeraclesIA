using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Types.Prompts
{
    public enum PromptType
    {
        [Description("Store Procedure")]
        StoreProcedure = 0,
        [Description("SQl Query")]
        SqlQuery = 1,   
        [Description("Message utilizado para el análisis del objetivo de un dashboard basandose en el nombre, descripción y los proveedores de datos utilizados")]
        DashboardDescription = 2,
        [Description("Evalua todo el árbol de queries del Dashboard")]
        DashboardSummary = 3,
        [Description("Evalua la información proporcionada y genera un resumen ejecutivo")]
        ProcessSummary = 4,
        [Description("Analiza el script de C# proporcionado")]
        ScriptAnalize = 5,
        [Description("Analiza el script de C# y retorna una lista de librerias externas utilizadas")]
        ExternalReference = 6,
        [Description("Extrae los nombres de los Servidores y Bases de Datos contenidos en un query de Sql")]
        SqlExtractServer = 7,
        [Description("Consulta Natiuva de SQl Document")]
        SQlDocument = 8,
    }

    public enum Language
    {
        SQL = 0,
        CSharp = 1
    }

    public enum Project
    {
        NONE = 0
    }
}

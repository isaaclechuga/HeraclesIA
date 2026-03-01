using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Types.Prompts
{
    public static class Message
    {
        public static string Build(PromptType type, string data, string data2 = "")
        {
            switch (type)
            {
                case PromptType.StoreProcedure:
                    return @$"Dame el nombre de los Stores Procedures utilizados en la consulta sql proporcionada y separalos por ; Consulta SQL a analizar: {data}; si la consulta SQl corresponde a la creacion del Store Procedure evita retornar el nombre del Store Procedure que se esta creando. no agregues una description, si no encuentras ninguno retorna la respuesta 'false' sin descripción adicional";

                case PromptType.SqlQuery:
                    return $@"Eres un ingeniero de IA especializado en análisis de bases de datos y queries SQL. 
        Tu tarea es analizar el query SQL proporcionado, desglosándolo en componentes clave para facilitar la comprensión, optimización y documentación. 
        Debes estructurar tu respuesta de manera clara, objetiva y basada en evidencia del query, sin agregar información externa o asumir detalles no presentes.
        Instrucciones Generales:
        Entrada Esperada: Un query SQL completo (ej. SELECT, INSERT, UPDATE, DELETE, etc.), incluyendo cualquier contexto relevante como el entorno de base de datos (e.g., MySQL, PostgreSQL).
        Análisis Paso a Paso:
        Identifica el Objetivo General: Determina el propósito principal del query (e.g., recuperar datos, actualizar registros).
        Desglosa el Flujo: Resume los pasos lógicos del query (e.g., selección de tablas, joins, filtros, agrupaciones).
        Extrae Estructuras de Datos: Lista las bases de datos, esquemas, tablas, alias y campos involucrados.
        Analiza Joins y Relaciones: Identifica y explica cualquier join o relación entre tablas.
        Analiza Condiciones y Filtros: Desglosa cláusulas WHERE, filtros en joins, etc.
        Formato de Salida: Estructura tu respuesta en las siguientes secciones numeradas. Usa tablas Markdown para claridad. Si una sección no aplica (e.g., no hay joins), indicarlo explícitamente.
        Consideraciones:
        Sé preciso: Basado únicamente en el query proporcionado.
        Usa lenguaje técnico pero accesible.
        Si hay ambigüedades (e.g., alias no definidos), asume razonablemente o nota la incertidumbre.
        Mantén la respuesta concisa pero completa.
        Secciones de Salida:
        usa el nombre del proveedor de datos proporcionado como subtitulo
        Resumen Ejecutivo y Lógica
        Crea una tabla con dos columnas:
        Concepto: Contiene exactamente 3 filas: ""Objetivo"", ""Resumen del Flujo"", y una tercera fila opcional si es relevante (e.g., ""Limitaciones"" si aplica).
        Descripción: Una breve descripción (1-2 oraciones) para cada concepto, basada en el query.
        Estructura de Datos (Bases de Datos, Tablas y Campos)
        Crea una tabla con 6 columnas:
        Base de Datos/Entorno: Nombre de la BD o entorno (e.g., ""MySQL"").
        Esquema: Esquema o namespace (e.g., ""public"").
        Tabla: Nombre de la tabla.
        Alias/Variable: Alias usado en el query (e.g., ""t1"").
        Campos Principales Utilizados: Lista de campos clave (e.g., ""id, name"").
        Descripción de la Tabla: Breve explicación de su propósito en el query (e.g., ""Tabla de usuarios para filtrar por edad"").
        Incluye una fila por tabla involucrada.
        Análisis de Joins y Relaciones
        Crea una tabla con 5 columnas:
        Ubicación: Parte del query donde ocurre (e.g., ""FROM clause"").
        Tipo de Join: Tipo (e.g., ""INNER JOIN"", ""LEFT JOIN"").
        Tablas Involucradas: Tablas conectadas (e.g., ""users, orders"").
        Condición de Unión (ON): La condición exacta (e.g., ""users.id = orders.user_id"").
        Explicación: Breve descripción de por qué se usa este join (e.g., ""Une usuarios con sus pedidos para relacionar datos"").
        Incluye una fila por join; si no hay joins, indica ""No aplica"".
        Análisis de Condiciones (WHERE) y Filtros
        Crea una tabla con 4 columnas:
        Ubicación: Cláusula o parte del query (e.g., ""WHERE clause"").
        Campo/Condición: Campo o expresión (e.g., ""age > 18"").
        Valor/Lógica: Valor ó lógica aplicada (e.g., ""18, AND"").
        Propósito: Explicación breve (e.g., ""Filtra usuarios mayores de edad"").
        Incluye una fila por condición; cubre WHERE, HAVING, filtros en joins, etc. Si no hay, indica ""No aplica"".
        Consulta SQL a analizar:
        {data}
        Luego, genera las secciones basadas en él.
        Al final, agrega un parrafo con Observaciones Adicionales sin mencionar riesgos en la consulta, Usa un lenguaje en tercera persona, formal y evita frases como 'aquí tienes', 'proporcionado'
        ";

                case PromptType.DashboardDescription:
                    return $@"Con base en el nombre del dashbaord, la descripión y los nombres de los proveedores de datos, ayudame con una descripción de la funcionalidad del dashboard,
                    incluye lista de Proveedores de datos (separados por ;) en forma de tabla en la cual requiero dos columnas: Nombre, Descripcion. Usa un lenguaje ejecutivo, en tercera persona y formal, evita frases como 'aquí tienes'. Estos son los datos que vas a analizar: {data}.";

                case PromptType.DashboardSummary:
                    return @$"Eres un ingeniero de IA especializado en análisis de bases de datos y queries SQL y experto en temas financieros. 
        Tu tarea es generar un documento con subtitulo Análisis Funcional con una explicacion logica del flujo que siguen las consultas para obtener resultados, basate en la estructura json proporcionada, utiliza los nombres de los campos y contenido de las consultas SQL proporcionadas para tu análisis, este analisis va enfocado en explicarle a una persona sin contexto la logica que siguen las consultas utilizadas, debes usar un lenguaje empresarial, en tercera persona, omite hablar de ti mismo, omite palabras como 'aquí tienes', 'el presente documento', tu analisis es continuación de un analisis previo por lo que debes evitar la palabra Introduccion. 
        Debes estructurar tu respuesta de manera clara, objetiva y basada en evidencia de los datos proporcionados. Estructura json a analizar: {data}";
                case PromptType.ProcessSummary:
                    return @$"Eres un analista de información financiera, tu trabajo es hacer un resumen ejecutivo de la informacion proporcionada, complementando la importancia del proceso,  debes usar un lenguaje empresarial, en tercera persona, omite hablar de ti mismo, omite palabras como 'aquí tienes', 'el presente documento', tu analisis es continuación de un analisis previo por lo que debes evitar la palabra Introduccion.
                        Debes estructurar tu respuesta de manera clara, objetiva y basada en evidencia de los datos proporcionados. Estructura json a analizar: {data}";
                case PromptType.ScriptAnalize:
                    return $@"Eres un ingeniero de IA especializado en desarrollo de software en .Net, específicamente en el lenguaje C#, tu tarea es analizar el script proporcionado basándote en la información proporcionada. El código proporcionado consiste en una rutina de código que utiliza una consulta SQl para consultar información en una base de datos para autocompletar campos correspondientes de un formulario de unity form llamado “{data2}”, estos campos se llaman Keywords.
                                Prepara una tabla con tres columnas:
                                1.	Proyecto: Contiene la información del espacio de nombres referenciado (excluye System y Hyland)
                                2.	Clase: Contiene el nombre de la clase perteneciente al proyecto
                                3.	Método/Función: Contiene el nombre de los métodos o funciones de las clases encontradas.
                                4.	Parámetros de entrada: Contiene la información de los parámetros que requieren los métodos o funciones.
                                5.	Parámetros de salida: Contiene la información de los datos de salida de los métodos o funciones
                                Formato de Salida: Estructura tu respuesta en las siguientes secciones numeradas. Usa tablas Markdown para claridad.
                                Criterios particulares a considerar:
                                •	Debes usar un lenguaje empresarial, en tercera persona, utiliza el tiempo presente del modo indicativo, omite hablar de ti mismo, omite palabras como 'aquí tienes', 'mi análisis se basa', 'A continuación se presenta el análisis técnico del script C# proporcionado'. Debes estructurar tu respuesta de manera clara, objetiva y basada en evidencia de los datos proporcionados.
                                •	Debes generar una redacción ejecutiva, en un lenguaje profesional pero que sea comprensible para cualquiera
                                •	Evita mencionar tu rol, tu tarea y las exclusiones
                                Script a analizar: {data}";

                case PromptType.ExternalReference:
                    return @$"Dame una lista de las referencias del proyecto en formato json basandote en esta lista de clase {data2}, y dentro de cada elemento una lista con las clases y dentro de las clases una lista con los metodos utilizados, sin ninguna redacción adicional, si no encuentras referencias entonces retorna nulo, Script a analizar: {data}";
                case PromptType.SqlExtractServer:
                    return $@"Extrae la lista de servidores, bases de datos y tablas existentes en la consulta Sql proporcionada: {data}, DataSource: {data2}. 

Retorna un formato json basándote en esta estructura de clases:
    public class Servidor
    {{
        public string Nombre {{ get; set; }}
        public List<BaseDatos> BaseDatos {{ get; set; }}
    }}
    public class BaseDatos
    {{
        public BaseDatos()
        {{
            Tablas = new List<string>();
        }}
        public string Nombre {{ get; set; }}
        public List<string> Tablas {{ get; set; }}
    }}
, 
Consideraciones: 
-	Evita retornar información adicional
-	Si el datasource es 'SRV-DBSQL-01' y las tablas no tienen un esquema de servidor.base de datos, entonces el servidor es ‘db2db.p.gslb’ y la base de datos es ‘Operaciones’
-	Si el datasource es ‘SRV-MDOCLU' y las tablas no tienen un esquema de servidor.base de datos, entonces el servidor es ‘MNYMKTBDB.P.GSLB’ y la base de datos es ‘base_md’
-	Si el datasource es ‘BNKPRD01’ y las tablas no tienen un esquema de servidor.base de datos, entonces el servidor es ‘DB2_FISERV’ y la base de datos es ‘BNKPRD01’
-	Si el datasource es ‘SRV-SIGLO’ y las tablas contienen 'siglo.siglocc' entonces el servidor es 'sglodb.p.gslb'
-	En caso de que no exista un servidor reconocible pero las tablas inician con hsi entonces el servidor es 'ONBSDB.P.GSLB' y la base de datos es 'Nautilus'
-	No consideres los llamados a Store Procedures como tablas
-	En caso de no detectar información retorna la estructura json vacía sin ninguna información adicional como descripción de la tarea que realizaste, resumen o justificaciones del resultado.
";
                default:
                    return "Unknown message type";
            }
        }

        public static string CodeSnipet(Language language, string data)
        {
            switch (language)
            {
                case Language.SQL:
                    return $@"<p>Estructura SQL:</p>
                                        <ac:structured-macro ac:name='code'>
                                            <ac:parameter ac:name='language'>sql</ac:parameter>
                                            <ac:plain-text-body><![CDATA[{data}]]></ac:plain-text-body>
                                        </ac:structured-macro>";
                case Language.CSharp:
                    return $@"<p>Fragmento de Código Analizado:</p>
                                        <ac:structured-macro ac:name='code'>
                                            <ac:parameter ac:name='language'>csharp</ac:parameter>
                                            <ac:plain-text-body><![CDATA[{data}]]></ac:plain-text-body>
                                        </ac:structured-macro>";
                default:
                    return "Unknown message type";
            }
        }
    }   
}

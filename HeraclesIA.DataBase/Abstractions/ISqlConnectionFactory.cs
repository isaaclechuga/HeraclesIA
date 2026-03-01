using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace HeraclesIA.DataBase.Abstractions
{
    public interface ISqlConnectionFactory
    {
        DbConnection CreateNautilus();
        DbConnection CreateCatalogos();
        DbConnection CreateOperaciones();
    }
}

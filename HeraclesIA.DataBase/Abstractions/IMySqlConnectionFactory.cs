using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HeraclesIA.DataBase.Abstractions 
{ 
    public interface IMySqlConnectionFactory { 
        DbConnection CreatePruebasDb(); 
    } 
}
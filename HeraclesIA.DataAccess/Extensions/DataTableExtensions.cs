using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.DataAccess.Extensions
{
    public static class DataTableExtensions
    {
        public static List<T> ToList<T>(this DataTable table, Func<DataRow, T> map)
        {
            if (table is null) throw new ArgumentNullException(nameof(table));
            if (map is null) throw new ArgumentNullException(nameof(map));

            var list = new List<T>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
                list.Add(map(row));

            return list;
        }
    }
}

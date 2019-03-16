using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;
using System.Data.SqlClient;

namespace MSSQLWrapper.Query {
    public class InsertQueryBuilder : QueryBuilder<InsertQuery, InsertQueryBuilder> {
        public InsertQueryBuilder(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout)
            : base() {
            Query = new InsertQuery(fromTable, connection, timeout);
        }

        public InsertQueryBuilder Insert(params Column[] columns) {
            var tables = columns.Select(r => r.Query.FromTable);

            if (tables.Count() > 1) {
                throw new ArgumentException("Columns must belong to a single table.");
            }

            Query.Table = tables.First();

            Query.InsertColumns = new List<Column>(columns);

            return this;
        }

        public InsertQueryBuilder Insert(string table, params string[] columns) {
            BaseQuery query = new BaseQuery(table);

            Query.Table = table;
            Query.InsertColumns = new List<Column>(columns.Select(r => query.NewColumn(r)));

            return this;
        }

        public InsertQueryBuilder Values(params object[] values) {
            Query.InsertValues = new List<object>(values);

            return this;
        }
    }
}

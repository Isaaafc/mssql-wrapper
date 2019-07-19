using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;
using System.Data.SqlClient;

namespace MSSQLWrapper.Query {
    public class InsertQueryBuilder : QueryBuilder<InsertQuery, InsertQueryBuilder> {
        public InsertQueryBuilder(SqlConnection connection = null, int timeout = DefaultTimeout)
            : base() {
            Query = new InsertQuery(connection, timeout);
        }

        public InsertQueryBuilder Insert(string table, params string[] columns) {
            Query.Table = table;
            Query.InsertColumns = new List<Column>(columns.Select(r => new Column(r)));

            return this;
        }

        public InsertQueryBuilder Values(params object[] values) {
            Query.InsertValues = new List<object>(values);

            return this;
        }

        public InsertQueryBuilder IfNotExists(SelectQuery selectQuery) {
            Query.IfNotExistsQuery = selectQuery;

            return this;
        }

        public InsertQueryBuilder ElseUpdate(UpdateQuery updateQuery) {
            Query.ElseUpdateQuery = updateQuery;

            return this;
        }
    }
}

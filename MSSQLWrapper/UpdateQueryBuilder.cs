using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper.Query {
    public class UpdateQueryBuilder : QueryBuilder<UpdateQuery, UpdateQueryBuilder> {
        public UpdateQueryBuilder(SqlConnection connection = null, int timeout = DefaultTimeout)
            : base() {
            Query = new UpdateQuery(connection, timeout);
        }

        public UpdateQueryBuilder Update(string table, UpdateQuery.UpdateSet updateColumns) {
            Query.UpdateTable = table;
            Query.UpdateColumns = updateColumns;

            return builder;
        }

        public UpdateQueryBuilder Update(string table, Dictionary<string, object> updateColumns) {
            Query.UpdateTable = table;
            Query.UpdateColumns = new UpdateQuery.UpdateSet(
                    updateColumns.Select(kv => Tuple.Create(new Column(kv.Key), kv.Value)).ToArray()
                );

            return builder;
        }

        public UpdateQueryBuilder Update(string table, Dictionary<Column, object> updateColumns) {
            Query.UpdateTable = table;
            Query.UpdateColumns = new UpdateQuery.UpdateSet(
                    updateColumns.Select(kv => Tuple.Create(kv.Key, kv.Value)).ToArray()
                );

            return builder;
        }
    }
}

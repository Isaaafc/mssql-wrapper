using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper.Query {
    public class UpdateQueryBuilder : QueryBuilder<UpdateQuery, UpdateQueryBuilder> {
        public UpdateQueryBuilder(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout)
            : base() {
            Query = new UpdateQuery(fromTable, connection, timeout);
        }

        public UpdateQueryBuilder Update(string table, UpdateQuery.UpdateSet updateColumns) {
            Query.UpdateTable = table;
            Query.UpdateColumns = updateColumns;

            return builder;
        }

        public UpdateQueryBuilder Update(string table, params Tuple<Column, object>[] updateColumns) {
            Query.UpdateTable = table;
            Query.UpdateColumns = new UpdateQuery.UpdateSet(updateColumns);

            return builder;
        }
    }
}

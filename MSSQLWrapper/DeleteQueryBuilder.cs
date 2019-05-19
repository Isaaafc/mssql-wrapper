using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper.Query {
    public class DeleteQueryBuilder : QueryBuilder<DeleteQuery, DeleteQueryBuilder> {
        public DeleteQueryBuilder(SqlConnection connection = null, int timeout = DefaultTimeout)
            : base() {
            Query = new DeleteQuery(connection, timeout);
        }

        public DeleteQueryBuilder Delete(string table) {
            Query.Table = table;

            return builder;
        }
    }
}

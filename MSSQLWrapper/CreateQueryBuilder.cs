using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;

namespace MSSQLWrapper.Query {
    public class CreateQueryBuilder : QueryBuilder<CreateQuery, CreateQueryBuilder> {

        public CreateQueryBuilder(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout)
            : base() {
            Query = new CreateQuery(fromTable, connection, timeout);
        }

        public CreateQueryBuilder Create(string table) {
            Query.Table = table;

            return this;
        }

        public CreateQueryBuilder AddConstraint(TableConstraint constraint) {
            Query.ListConstraints.Add(constraint);

            return this;
        }

        public CreateQueryBuilder AddColumn(string columnName, DataType dataType, int arg = -1) {
            Query.ListColumns.Add(Tuple.Create(columnName, dataType, arg));

            return this;
        }

        public CreateQueryBuilder AddColumn(string columnName, TableConstraint constraint, DataType dataType, int arg = -1) {
            Query.ListColumns.Add(Tuple.Create(columnName, dataType, arg));

            Query.ListConstraints.Add(constraint);

            return this;
        }
    }
}

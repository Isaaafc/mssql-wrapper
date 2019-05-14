using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;

namespace MSSQLWrapper.Query {
    public class CreateQueryBuilder : QueryBuilder<CreateQuery, CreateQueryBuilder> {

        public CreateQueryBuilder(SqlConnection connection = null, int timeout = DefaultTimeout)
            : base() {
            Query = new CreateQuery(connection, timeout);
        }

        public CreateQueryBuilder Create(string table) {
            Query.Table = table;

            return this;
        }

        public CreateQueryBuilder AddConstraint(TableConstraint constraint) {
            Query.ListConstraints.Add(constraint);

            return this;
        }

        /// <summary>
        /// Specify column to be identity
        /// </summary>
        /// <param name="columnName">Name of column to be an identity</param>
        /// <param name="seed">Starting index</param>
        /// <param name="increment">Increment step</param>
        /// <returns></returns>
        public CreateQueryBuilder AddIdentity(string columnName, int seed, int increment) {
            Query.IdentityColumns[columnName] = Tuple.Create(seed, increment);

            if (Query.ListColumns.Where(r => r.Item1 == columnName).Count() == 0) {
                AddColumn(columnName, DataType.Int);
            }

            return this;
        }

        /// <summary>
        /// Add column to create
        /// </summary>
        /// <param name="columnName">Name of column</param>
        /// <param name="dataType">SQL data type</param>
        /// <param name="arg">Parameter of data type if applicable</param>
        /// <returns></returns>
        public CreateQueryBuilder AddColumn(string columnName, DataType dataType, int arg = -1) {
            Query.ListColumns.Add(Tuple.Create(columnName, dataType, arg));

            return this;
        }

        /// <summary>
        /// Add column with constraint
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="constraint"></param>
        /// <param name="dataType"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public CreateQueryBuilder AddColumn(string columnName, TableConstraint constraint, DataType dataType, int arg = -1) {
            Query.ListColumns.Add(Tuple.Create(columnName, dataType, arg));

            Query.ListConstraints.Add(constraint);

            return this;
        }
    }
}

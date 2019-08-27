using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper.Query {
    public class DeleteQuery : BaseQuery {
        /// <summary>
        /// Name of table to delete. Leave empty for delete all
        /// </summary>
        public string Table { get; set; }

        public DeleteQuery(SqlConnection connection = null, int timeout = DefaultTimeout)
            : base(connection, timeout) {
        }

        public new DeleteQuery Clone() {
            DeleteQuery query = new DeleteQuery(connection: Connection, timeout: Timeout);

            /// From BaseQuery
            if (FromTable != null)
                query.FromTable = FromTable;
            else
                query.FromQuery = FromQuery;

            query.WhereCondition = WhereCondition;
            query.ListJoin = ListJoin;
            query.Alias = Alias;

            return query;
        }

        public override string ToPlainQuery() {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("DELETE {0} FROM {1}{2}", Table, FromTableOrQuery(), FromQuery == null ? "" : $" AS {FromQuery.Item2}");

            sb.Append(JoinString);

            if (WhereCondition != null) {
                sb.AppendFormat(" WHERE {0}", WhereCondition.ToString());
            }

            return sb.ToString().Trim();
        }

        public int ExecuteQuery() {
            using (SqlCommand cmd = GetSqlCommand()) {
                var listConditions = GetConditions();
                AssignParamNames(listConditions);

                cmd.CommandText = ToPlainQuery();
                AddCommandParams(cmd, listConditions);

                return cmd.ExecuteNonQuery();
            }
        }

        public int ExecuteQuery(SqlTransaction trans) {
            using (SqlCommand cmd = GetSqlCommand(trans)) {
                var listConditions = GetConditions();
                AssignParamNames(listConditions);

                cmd.CommandText = ToPlainQuery();
                AddCommandParams(cmd, listConditions);

                return cmd.ExecuteNonQuery();
            }
        }
    }
}

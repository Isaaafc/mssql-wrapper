using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;
using System.Data.SqlClient;

namespace MSSQLWrapper.Query {
    public class UpdateQuery : BaseQuery {
        /// <summary>
        /// Represents a set of column value pairs to be updated
        /// </summary>
        public struct UpdateSet {
            internal List<Tuple<Column, object>> ListSet { get; private set; }

            public UpdateSet(params Tuple<Column, object>[] args) {
                ListSet = new List<Tuple<Column, object>>(args);

                /// Validate
                var c = ListSet.Select(r => r.Item1.Query == null ? null : r.Item1.Query.FromTableOrAlias())
                               .Where(r => r != null)
                               .Distinct()
                               .Count();

                if (c > 1) {
                    throw new ArgumentException("Left columns must be from the same table");
                }
            }

            public override string ToString() {
                var str = new string[ListSet.Count];

                for (int i = 0; i < ListSet.Count; i++) {
                    Column left = ListSet[i].Item1;
                    object right = ListSet[i].Item2;

                    str[i] = String.Format(" {0} = {1}", left.FullName, (right is Column) ? (right as Column).FullName : $"@updP{i}");
                }

                return String.Join($",{Environment.NewLine}", str);
            }
        }

        /// <summary>
        /// Columns and values to be updated
        /// </summary>
        public UpdateSet UpdateColumns { get; set; }
        /// <summary>
        /// Table to be updated
        /// </summary>
        public string UpdateTable { get; set; }

        public UpdateQuery(SqlConnection connection = null, int timeout = DefaultTimeout)
            : base(connection, timeout) {
            
        }

        /// <summary>
        /// Returns a new instance with exactly the same properties
        /// </summary>
        /// <returns></returns>
        public new UpdateQuery Clone() {
            UpdateQuery query = new UpdateQuery(connection: Connection, timeout: Timeout);

            /// From BaseQuery
            if (FromTable != null)
                query.FromTable = FromTable;
            else
                query.FromQuery = FromQuery;

            query.WhereCondition = WhereCondition;
            query.ListJoin = ListJoin;
            query.Alias = Alias;

            /// From UpdateQuery
            query.UpdateColumns = UpdateColumns;
            query.UpdateTable = UpdateTable;

            return query;
        }

        public override string ToPlainQuery() {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("UPDATE {0}", UpdateTable)
              .AppendLine();

            sb.AppendLine("SET");

            sb.AppendLine(UpdateColumns.ToString());

            if (FromTableOrQuery() != null) {
                sb.AppendLine("FROM")
                  .AppendFormat("{0}{1}", FromTableOrQuery(), FromQuery == null ? "" : $" AS {FromQuery.Item2}");
            }

            sb.AppendLine()
              .Append(JoinString);

            if (WhereCondition != null) {
                sb.AppendLine("WHERE");

                sb.AppendLine(" " + WhereCondition.ToString());
            }

            return sb.ToString();
        }

        protected override void AddCommandParams(SqlCommand cmd, List<Condition> listConditions) {
            base.AddCommandParams(cmd, listConditions);

            for (int i = 0; i < UpdateColumns.ListSet.Count; i++) {
                var tuple = UpdateColumns.ListSet[i];

                cmd.Parameters.Add(new SqlParameter($"@updP{i}", tuple.Item2 ?? DBNull.Value));
            }
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

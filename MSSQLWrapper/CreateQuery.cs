using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;

namespace MSSQLWrapper.Query {
    public class CreateQuery : BaseQuery {
        public string Table { get; set; }
        public List<TableConstraint> ListConstraints { get; set; }
        /// <summary>
        /// Tuple of [columnName, dataType, dataType args]
        /// </summary>
        public List<Tuple<string, DataType, int>> ListColumns { get; set; }
        /// <summary>
        /// Identity columns
        /// Key: column name, Value: seed, increment
        /// </summary>
        public Dictionary<string, Tuple<int, int>> IdentityColumns { get; set; }

        public CreateQuery(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout)
           : base(fromTable, connection, timeout) {

            ListConstraints = new List<TableConstraint>();
            ListColumns = new List<Tuple<string, DataType, int>>();

            IdentityColumns = new Dictionary<string, Tuple<int, int>>();
        }

        public new CreateQuery Clone() {
            CreateQuery query = new CreateQuery(connection: Connection, timeout: Timeout);

            /// From BaseQuery
            if (FromTable != null)
                query.FromTable = FromTable;
            else
                query.FromQuery = FromQuery;

            query.WhereCondition = WhereCondition;
            query.ListJoin = ListJoin;
            query.Alias = Alias;

            /// From CreateQuery
            query.Table = Table;
            query.ListConstraints = ListConstraints;
            query.ListColumns = ListColumns;
            query.IdentityColumns = IdentityColumns;

            return query;
        }

        public string GetConstraintString() {
            StringBuilder sb = new StringBuilder();

            foreach (TableConstraint constraint in ListConstraints) {
                sb.AppendLine(constraint.ToString());
            }

            return sb.ToString();
        }

        protected override string ToRawQuery(List<Condition> listConditions) {
            StringBuilder sb = new StringBuilder();

            if (FromTableOrQuery() == null) {
                sb.AppendFormat("CREATE TABLE {0} (", Table)
                  .AppendLine();

                foreach (var column in ListColumns) {
                    sb.AppendFormat(" {0} {1}", column.Item1, String.Format(column.Item2.GetStringValue(), column.Item3));

                    Tuple<int, int> idParam;

                    if (IdentityColumns.TryGetValue(column.Item1, out idParam)) {
                        sb.AppendFormat(" IDENTITY({0}, {1})", idParam.Item1, idParam.Item2);
                    }

                    sb.AppendLine(",");
                }

                if (ListConstraints.Count > 0) {
                    sb.Append(GetConstraintString());
                }

                sb.AppendLine(");");
            } else {
                sb.AppendLine("SELECT")
                  .AppendLine(" *")
                  .AppendLine("INTO")
                  .AppendLine($" {Table}");

                sb.AppendLine("FROM");

                sb.AppendLine(FromTableOrQuery());

                sb.Append(JoinString);

                if (WhereCondition != null) {
                    sb.AppendLine("WHERE");

                    sb.AppendLine($"  {WhereCondition.ToString()}");
                }
            }

            return sb.ToString();
        }

        public int ExecuteQuery() {
            using (SqlCommand cmd = GetSqlCommand()) {
                var listConditions = GetConditions();
                AssignParamNames(listConditions);

                cmd.CommandText = ToRawQuery(listConditions);
                AddCommandParams(cmd, listConditions);

                return cmd.ExecuteNonQuery();
            }
        }

        public int ExecuteQuery(SqlTransaction trans) {
            using (SqlCommand cmd = GetSqlCommand(trans)) {
                var listConditions = GetConditions();
                AssignParamNames(listConditions);

                cmd.CommandText = ToRawQuery(listConditions);
                AddCommandParams(cmd, listConditions);

                return cmd.ExecuteNonQuery();
            }
        }
    }
}

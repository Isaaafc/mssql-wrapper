using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;
using System.Data.SqlClient;

namespace MSSQLWrapper.Query {
    public class InsertQuery : BaseQuery {
        public List<Column> InsertColumns { get; set; }
        public List<object> InsertValues { get; set; }

        public string Table { get; set; }

        public override string FromTable {
            set {
                if (value != null)
                    FromQuery = Tuple.Create(new SelectQuery(value), (string)null);
            }
        }

        public InsertQuery(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout)
            : base(fromTable, connection, timeout) {
            InsertColumns = new List<Column>();
            InsertValues = new List<object>();
        }

        public override string ToRawQuery() {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("INSERT INTO {0}", Table);

            if (InsertColumns.Count > 0) {
                sb.AppendLine(" (")
                  .AppendLine(String.Join($",{Environment.NewLine}", InsertColumns.Select(r => $"[{r.Name}]")))
                  .AppendLine(")");
            } else {
                sb.AppendLine();
            }

            if (FromQuery == null) {
                sb.AppendLine("VALUES (");

                sb.AppendLine(String.Join($",{Environment.NewLine}", Enumerable.Range(0, InsertValues.Count).Select(r => $"@insertParam{r}")))
                  .AppendLine(")");
            } else {
                sb.AppendLine(FromQuery.Item1.ToRawQuery());
            }

            return sb.ToString();
        }

        protected override void AddCommandParams(SqlCommand cmd) {
            base.AddCommandParams(cmd);

            for (int i = 0; i < InsertValues.Count; i++) {
                object val = InsertValues[i];

                if (!(val is Column)) {
                    cmd.Parameters.Add(new SqlParameter($"@insertParam{i}", val));
                }
            }
        }

        public int ExecuteQuery() {
            using (SqlCommand cmd = GetSqlCommand()) {
                cmd.CommandText = ToRawQuery();

                AddCommandParams(cmd);

                return cmd.ExecuteNonQuery();
            }
        }
    }
}

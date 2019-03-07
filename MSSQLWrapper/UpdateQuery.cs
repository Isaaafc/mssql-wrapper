using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;
using System.Data.SqlClient;

namespace MSSQLWrapper.Query {
    public class UpdateQuery : BaseQuery {
        public struct UpdateSet {
            internal List<Tuple<Column, object>> ListSet { get; private set; }

            public UpdateSet(params object[] args) {
                ListSet = new List<Tuple<Column, object>>();

                for (int i = 0; i < args.Length; i += 2) {
                    ListSet.Add(Tuple.Create(args[i] as Column, args[i + 1]));
                }

                /// Validate
                var c = ListSet.Select(r => r.Item1.FullName)
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

                    str[i] = String.Format("{0} = {1}", left.FullName, (right is Column) ? (right as Column).FullName : $"@updateParam{i}");
                }

                return String.Join($",{Environment.NewLine}", str);
            }
        }

        public UpdateSet UpdateColumns { get; set; }
        public string UpdateTable { get; set; }

        public UpdateQuery(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout)
            : base(fromTable, connection, timeout) {
            
        }

        public override string ToRawQuery() {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("UPDATE {0}", UpdateTable)
              .AppendLine();

            sb.AppendLine("SET");

            sb.Append(UpdateColumns.ToString());

            sb.AppendLine("FROM")
              .AppendFormat("{0}{1}", FromTableOrQuery(), FromQuery == null ? "" : $" AS {FromQuery.Item2}")
              .AppendLine();

            sb.Append(JoinString);

            if (WhereCondition != null) {
                sb.AppendLine("WHERE");

                sb.AppendLine(" " + WhereCondition.ToString());
            }

            return sb.ToString();
        }

        protected override void AddCommandParams(SqlCommand cmd) {
            base.AddCommandParams(cmd);

            for (int i = 0; i < UpdateColumns.ListSet.Count; i++) {
                object val = UpdateColumns.ListSet[i];

                if (!(val is Column)) {
                    cmd.Parameters.Add(new SqlParameter($"@updateParam{i}", val));
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

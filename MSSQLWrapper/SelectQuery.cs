using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;
using System.Data;

namespace MSSQLWrapper.Query {
    public class SelectQuery : BaseQuery<SelectQuery> {
        public List<Column<SelectQuery>> SelectColumns { get; set; }
        public List<Column<SelectQuery>> GroupByColumns { get; set; }
        public List<Condition<SelectQuery>> HavingColumns { get; set; }
        public List<Tuple<Column<SelectQuery>, Order>> OrderByColumns { get; set; }
        public int Top { get; set; }
        public bool Distinct { get; set; }
        public string JoinQuery {
            get {
                return (SelectColumns.Count == 0 && ListJoin.Count == 0) ? FromTable : ToRawQuery();
            }
        }

        public SelectQuery(SqlConnection connection = null, int timeout = DefaultTimeout)
            : base(connection, timeout) {

            SelectColumns = new List<Column<SelectQuery>>();

            GroupByColumns = new List<Column<SelectQuery>>();

            HavingColumns = new List<Condition<SelectQuery>>();

            OrderByColumns = new List<Tuple<Column<SelectQuery>, Order>>();

            Top = -1;

            Distinct = false;
        }

        public SelectQuery Select(params string[] columns) {
            SelectColumns.AddRange(columns.Select(r => GetNewColumn(r)));

            return this;
        }

        public SelectQuery Select(params Column<SelectQuery>[] columns) {
            SelectColumns.AddRange(columns);

            return this;
        }

        public SelectQuery GroupBy(params string[] columns) {
            GroupByColumns.AddRange(columns.Select(r => GetNewColumn(r)));

            return this;
        }

        public SelectQuery GroupBy(params Column<SelectQuery>[] columns) {
            GroupByColumns.AddRange(columns);

            return this;
        }

        public SelectQuery OrderBy(params object[] columns) {
            ArgumentException e = new ArgumentException("Invalid arguments: params must be in pairs of (Columns/string, Order)");

            for (int i = 0; i < columns.Length; i += 2) {
                if (i + 1 >= columns.Length || !(columns[i + 1] is Order)) {
                    throw e;
                }

                if (columns[i] is string) {
                    OrderByColumns.Add(Tuple.Create(GetNewColumn((string)columns[i]), (Order)columns[i + 1]));
                } else if (columns[i] is Column<SelectQuery>) {
                    OrderByColumns.Add(Tuple.Create((Column<SelectQuery>)columns[i], (Order)columns[i + 1]));
                } else {
                    throw e;
                }
            }

            return this;
        }

        public override string ToRawQuery() {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT");

            if (Distinct) {
                sb.Append(" DISTINCT");
            }

            if (Top > 0) {
                sb.AppendFormat(" TOP {0}", Top)
                  .AppendLine();
            }

            if (SelectColumns.Count > 0) {
                for (int i = 0; i < SelectColumns.Count; i++) {
                    Column<SelectQuery> col = SelectColumns[i];

                    sb.Append(col.AliasedName);
                    
                    if (i < SelectColumns.Count - 1)
                        sb.Append(",");

                    sb.AppendLine();
                }
            } else {
                sb.AppendLine(" *");
            }

            sb.AppendLine("FROM")
              .AppendFormat(" {0}", FromTableOrQuery())
              .AppendLine();

            sb.Append(JoinString);

            if (WhereCondition != null) {
                sb.AppendLine("WHERE");

                sb.AppendLine(" " + WhereCondition.ToString());
            }

            if (GroupByColumns.Count > 0) {
                sb.AppendLine("GROUP BY");

                sb.AppendLine(String.Join(",", GroupByColumns.Select(r => " " + r.ConditionName)));
            }

            if (OrderByColumns.Count > 0) {
                sb.AppendLine("ORDER BY");

                sb.AppendLine(String.Join(",", OrderByColumns.Select(r => String.Format(" [{0}] {1}", String.IsNullOrEmpty(r.Item1.Alias)?r.Item1.Name:r.Item1.Alias, r.Item2.GetStringValue()))));
            }

            return sb.ToString();
        }

        public string ToQuotedQuery(bool withAlias) {
            string rawQuery = ToRawQuery(), 
                   vanillaQuery = FromTable == null ? "" : new SelectQuery().From(FromTable).ToRawQuery();
            
            /// If the query only contains a table name, return table name directly
            return String.Format("({0}{1}{0}){2}",
                Environment.NewLine,
                rawQuery == vanillaQuery ? FromTable : rawQuery,
                withAlias ? "" : $" AS {Alias}");
        }

        public DataTable ExecuteQuery() {
            using (SqlCommand cmd = GetSqlCommand()) {
                cmd.CommandText = ToRawQuery();

                AddCommandParams(cmd);

                DataTable dt = new DataTable();

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) {
                    adapter.Fill(dt);
                }

                return dt;
            }
        }

        public SqlDataReader ExecuteReader() {
            using (SqlCommand cmd = GetSqlCommand()) {
                cmd.CommandText = ToRawQuery();

                AddCommandParams(cmd);

                return cmd.ExecuteReader();
            }
        }
    }
}

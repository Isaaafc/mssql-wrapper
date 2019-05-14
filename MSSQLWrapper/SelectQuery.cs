using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;
using System.Data;
using System.Dynamic;

namespace MSSQLWrapper.Query {
    public class SelectQuery : BaseQuery {
        public List<Column> SelectColumns { get; set; }
        public List<Column> GroupByColumns { get; set; }
        public Condition HavingCondition { get; set; }
        public List<Tuple<Column, Order>> OrderByColumns { get; set; }
        public int TopN { get; set; }
        public bool SelectDistinct { get; set; }

        /// <summary>
        /// Returns true if this object is equal to SELECT * FROM [Table]
        /// </summary>
        public bool IsTableOnly {
            get {
                return (
                    SelectColumns.Count == 0
                    && GroupByColumns.Count == 0
                    && OrderByColumns.Count == 0
                    && TopN == -1
                    && SelectDistinct == false
                    && ListJoin.Count == 0
                    && HavingCondition == null
                    && WhereCondition == null
                    && (FromQuery == null || FromQuery.Item1.IsTableOnly)
                );
            }
        }

        public SelectQuery(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout)
            : base(connection, timeout) {

            SelectColumns = new List<Column>();

            GroupByColumns = new List<Column>();

            OrderByColumns = new List<Tuple<Column, Order>>();

            TopN = -1;

            SelectDistinct = false;

            FromTable = fromTable;
        }

        /// <summary>
        /// Returns a new instance with exactly the same properties
        /// </summary>
        /// <returns></returns>
        public new SelectQuery Clone() {
            SelectQuery query = new SelectQuery(connection: Connection, timeout: Timeout);

            /// From BaseQuery
            if (FromTable != null)
                query.FromTable = FromTable;
            else
                query.FromQuery = FromQuery;

            query.WhereCondition = WhereCondition;
            query.ListJoin = ListJoin;
            query.Alias = Alias;

            /// From SelectQuery
            query.SelectColumns = SelectColumns;
            query.GroupByColumns = GroupByColumns;
            query.HavingCondition = HavingCondition;
            query.OrderByColumns = OrderByColumns;
            query.TopN = TopN;
            query.SelectDistinct = SelectDistinct;

            return query;
        }

        protected override string ToRawQuery(List<Condition> listConditions) {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT");

            if (SelectDistinct) {
                sb.Append(" DISTINCT");
            }

            if (TopN > 0) {
                sb.AppendFormat(" TOP {0}", TopN)
                  .AppendLine();
            }

            if (SelectColumns.Count > 0) {
                for (int i = 0; i < SelectColumns.Count; i++) {
                    Column col = SelectColumns[i];

                    sb.Append(" " + col.AliasedName);
                    
                    if (i < SelectColumns.Count - 1)
                        sb.Append(",");

                    sb.AppendLine();
                }
            } else {
                sb.AppendLine(" *");
            }

            sb.AppendLine("FROM")
              .AppendFormat("{0}{1}", FromTableOrQuery(), FromQuery == null ? "" : $" AS {FromQuery.Item2}")
              .AppendLine();

            sb.Append(JoinString);

            if (WhereCondition != null) {
                sb.AppendLine("WHERE");

                sb.AppendLine(" " + WhereCondition.ToString());
            }

            if (GroupByColumns.Count > 0) {
                sb.AppendLine("GROUP BY");

                sb.AppendLine(String.Join(",", GroupByColumns.Select(r => " " + r.FullName)));
            }

            if (HavingCondition != null) {
                sb.AppendLine("HAVING");

                sb.AppendLine(" " + HavingCondition.ToString());
            }

            if (OrderByColumns.Count > 0) {
                sb.AppendLine("ORDER BY");

                sb.AppendLine(String.Join(",", OrderByColumns.Select(r => String.Format(" {0} {1}", String.IsNullOrEmpty(r.Item1.Alias) ? r.Item1.FullName : r.Item1.Alias, r.Item2.GetStringValue()))));
            }

            return sb.ToString();
        }

        /// <summary>
        /// If the query only contains a table name, return table name directly
        /// </summary>
        /// <returns></returns>
        public string ToTableOrQuery() {
            return String.Format("{1}{0}", Environment.NewLine, IsTableOnly ? (FromTable == null ? FromQuery.Item1.FromTable : FromTable) : $"({ToRawQuery()})");
        }

        protected override List<Condition> GetConditions() {
            var listConditions = base.GetConditions();

            if (HavingCondition != null) {
                listConditions.AddRange(HavingCondition.GetAllConditions());
            }

            return listConditions;
        }

        public DataTable ExecuteQuery() {
            using (SqlCommand cmd = GetSqlCommand()) {
                var listConditions = GetConditions();
                AssignParamNames(listConditions);

                cmd.CommandText = ToRawQuery(listConditions);
                AddCommandParams(cmd, listConditions);

                DataTable dt = new DataTable();

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) {
                    adapter.Fill(dt);
                }

                return dt;
            }
        }

        public IEnumerable<dynamic> ExecuteDynamic() {
            using (var reader = ExecuteReader()) {
                var schemaTable = reader.GetSchemaTable();

                var colNames = new List<string>();

                foreach (DataRow row in schemaTable.Rows) {
                    colNames.Add(row["ColumnName"].ToString());
                }

                while (reader.Read()) {
                    var data = new ExpandoObject() as IDictionary<string, Object>;

                    foreach (string colname in colNames) {
                        var val = reader[colname];
                        data.Add(colname, Convert.IsDBNull(val) ? null : val);
                    }

                    yield return (ExpandoObject)data;
                }
            }
        }

        public SqlDataReader ExecuteReader() {
            using (SqlCommand cmd = GetSqlCommand()) {
                var listConditions = GetConditions();
                AssignParamNames(listConditions);

                cmd.CommandText = ToRawQuery(listConditions);
                AddCommandParams(cmd, listConditions);

                return cmd.ExecuteReader();
            }
        }
    }
}

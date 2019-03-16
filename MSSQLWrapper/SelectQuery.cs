﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;
using System.Data;

namespace MSSQLWrapper.Query {
    public class SelectQuery : BaseQuery {
        public List<Column> SelectColumns { get; set; }
        public List<Column> GroupByColumns { get; set; }
        public List<Condition> HavingColumns { get; set; }
        public List<Tuple<Column, Order>> OrderByColumns { get; set; }
        public int TopN { get; set; }
        public bool SelectDistinct { get; set; }

        public bool IsTableOnly {
            get {
                return (
                    SelectColumns.Count == 0
                    && GroupByColumns.Count == 0
                    && HavingColumns.Count == 0
                    && OrderByColumns.Count == 0
                    && TopN == -1
                    && SelectDistinct == false
                    && ListJoin.Count == 0
                    && WhereCondition == null
                    && (FromQuery == null || FromQuery.Item1.IsTableOnly)
                );
            }
        }

        public SelectQuery(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout)
            : base(fromTable, connection, timeout) {

            SelectColumns = new List<Column>();

            GroupByColumns = new List<Column>();

            HavingColumns = new List<Condition>();

            OrderByColumns = new List<Tuple<Column, Order>>();

            TopN = -1;

            SelectDistinct = false;
        }

        public override string ToRawQuery() {
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

            if (OrderByColumns.Count > 0) {
                sb.AppendLine("ORDER BY");

                sb.AppendLine(String.Join(",", OrderByColumns.Select(r => String.Format(" [{0}] {1}", String.IsNullOrEmpty(r.Item1.Alias)?r.Item1.Name:r.Item1.Alias, r.Item2.GetStringValue()))));
            }

            return sb.ToString();
        }

        public string ToTableOrQuery() {
            /// If the query only contains a table name, return table name directly
            return String.Format("{0}{1}{0}", Environment.NewLine, IsTableOnly ? (FromTable == null ? FromQuery.Item1.FromTable : FromTable) : $"({ToRawQuery()})");
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

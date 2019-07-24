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
                if (value != null) {
                    FromQuery = Tuple.Create(new SelectQuery(value), (string)null);
                }
            }
        }

        public SelectQuery IfNotExistsQuery { get; set; }
        public UpdateQuery ElseUpdateQuery { get; set; }

        public InsertQuery(SqlConnection connection = null, int timeout = DefaultTimeout)
            : base(connection, timeout) {
            InsertColumns = new List<Column>();
            InsertValues = new List<object>();
        }

        public new InsertQuery Clone() {
            InsertQuery query = new InsertQuery(connection: Connection, timeout: Timeout);

            /// From BaseQuery
            if (FromTable != null)
                query.FromTable = FromTable;
            else
                query.FromQuery = FromQuery;

            query.WhereCondition = WhereCondition;
            query.ListJoin = ListJoin;
            query.Alias = Alias;

            /// From InsertQuery
            query.InsertColumns = InsertColumns;
            query.InsertValues = InsertValues;
            query.Table = Table;
            query.FromTable = FromTable;
            query.IfNotExistsQuery = IfNotExistsQuery;
            query.ElseUpdateQuery = ElseUpdateQuery;

            return query;
        }

        public override string ToPlainQuery() {
            StringBuilder sb = new StringBuilder();

            if (IfNotExistsQuery != null) {
                sb.AppendFormat("IF NOT EXISTS ({0})", IfNotExistsQuery.ToPlainQuery());
            }

            sb.AppendFormat(" INSERT INTO {0}", Table);

            if (InsertColumns.Count > 0) {
                sb.AppendFormat(" ({0})", String.Join($", ", InsertColumns.Select(r => $"{r.Name}")));
            }

            if (FromQuery == null) {
                sb.AppendFormat(" VALUES ({0})", String.Join($", ", Enumerable.Range(0, InsertValues.Count).Select(r => $"@insP{r}")));
            } else {
                sb.Append($" {FromQuery.Item1.ToPlainQuery()}");
            }

            if (ElseUpdateQuery != null) {
                sb.AppendFormat(" ELSE {0}", ElseUpdateQuery.ToPlainQuery());
            }

            return sb.ToString().Trim();
        }

        protected override void AddCommandParams(SqlCommand cmd, List<Condition> listConditions) {
            base.AddCommandParams(cmd, listConditions);

            for (int i = 0; i < InsertValues.Count; i++) {
                object val = InsertValues[i];

                if (!(val is Column)) {
                    cmd.Parameters.Add(new SqlParameter($"@insP{i}", val));
                }
            }

            if (ElseUpdateQuery != null) {
                ElseUpdateQuery.AddUpdateCommandParams(cmd);
            }
        }

        internal override List<Condition> GetConditions() {
            var listConditions = base.GetConditions();

            /// If not exists conditions
            if (IfNotExistsQuery != null) {
                listConditions.AddRange(IfNotExistsQuery.GetConditions());
            }

            /// Else update conditions
            if (ElseUpdateQuery != null) {
                listConditions.AddRange(ElseUpdateQuery.GetConditions());
            }

            return listConditions;
        }

        public int ExecuteQuery(SqlTransaction trans = null) {
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

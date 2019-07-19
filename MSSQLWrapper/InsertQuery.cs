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
                sb.AppendLine("IF NOT EXISTS (")
                  .AppendLine(IfNotExistsQuery.ToPlainQuery())
                  .AppendLine(")");
            }

            sb.AppendFormat("INSERT INTO {0}", Table);

            if (InsertColumns.Count > 0) {
                sb.AppendLine(" (")
                  .AppendLine(String.Join($",{Environment.NewLine}", InsertColumns.Select(r => $" {r.Name}")))
                  .AppendLine(")");
            } else {
                sb.AppendLine();
            }

            if (FromQuery == null) {
                sb.AppendLine("VALUES (")
                  .AppendLine(String.Join($",{Environment.NewLine}", Enumerable.Range(0, InsertValues.Count).Select(r => $" @insP{r}")))
                  .AppendLine(")");
            } else {
                sb.AppendLine(FromQuery.Item1.ToPlainQuery());
            }

            if (ElseUpdateQuery != null) {
                sb.AppendLine("ELSE")
                  .AppendLine(ElseUpdateQuery.ToPlainQuery());
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

            ElseUpdateQuery.AddUpdateCommandParams(cmd);
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

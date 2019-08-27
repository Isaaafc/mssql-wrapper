using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;

namespace MSSQLWrapper.Query {
    /// <summary>
    /// Represents one SQL query with a single connection
    /// </summary>
    public class BaseQuery {
        /// <summary>
        /// Default timeout in seconds
        /// </summary>
        public const int DefaultTimeout = 30;

        protected string rawQuery;

        protected Tuple<SelectQuery, string> fromQuery;

        protected string fromTable;

        public string RawQuery {
            get {
                return rawQuery;
            }
        }

        public virtual string FromTable {
            get {
                return fromTable;
            }
            set {
                fromQuery = null;

                fromTable = value;
            }
        }

        public virtual Tuple<SelectQuery, string> FromQuery {
            get {
                return fromQuery;
            }
            set {
                fromTable = null;

                fromQuery = value;
            }
        }

        public string Alias { get; internal set; }
        public int Timeout { get; set; }
        public SqlConnection Connection { get; set; }
        public Condition WhereCondition { get; set; }
        public List<JoinClause> ListJoin { get; set; }

        public string JoinString {
            get {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < ListJoin.Count; i++) {
                    var joinClause = ListJoin[i];

                    sb.AppendFormat("{0}JOIN {1}{2} ON {3}",
                        joinClause.Type == JoinType.Inner ? "" : $"{joinClause.Type.GetStringValue()} ",
                        joinClause.Query.ToTableOrQuery(),
                        String.IsNullOrEmpty(joinClause.Query.Alias) ? "" : $" AS {joinClause.Query.Alias}",
                        joinClause.Condition.ToString());
                }

                return sb.ToString();
            }
        }

        public BaseQuery(SqlConnection connection = null, int timeout = DefaultTimeout) {
            Connection = connection;

            Timeout = timeout;

            ListJoin = new List<JoinClause>();
        }

        protected BaseQuery Clone() {
            BaseQuery query = new BaseQuery(Connection, Timeout);

            query.FromTable = FromTable;
            query.FromQuery = FromQuery;
            query.WhereCondition = WhereCondition;
            query.ListJoin = ListJoin;

            return query;
        }

        public Column NewColumn(string name, string alias = null) {
            return new Column(query: this, name: name, alias: alias);
        }

        /// <summary>
        /// Gets a flatted list of all conditions associated with this query
        /// </summary>
        /// <returns></returns>
        internal virtual List<Condition> GetConditions() {
            var listConditions = new List<Condition>();

            if (WhereCondition != null) {
                listConditions.AddRange(WhereCondition.GetAllConditions());
            }

            /// All Conditions from Join
            listConditions.AddRange(ListJoin.Select(r => r.Condition.GetAllConditions()).SelectMany(r => r));

            /// All Conditions from joined Queries
            listConditions.AddRange(ListJoin.Select(r => r.Query.GetConditions()).SelectMany(r => r));

            /// All Conditions from FromQuery
            if (FromQuery != null) {
                listConditions.AddRange(FromQuery.Item1.GetConditions());
            }

            return listConditions;
        }

        protected void AssignParamNames(List<Condition> listConditions) {
            int i = 0;

            for (int j = 0; j < listConditions.Count; j++) {
                if (!(listConditions[j].Value is Column)) {
                    listConditions[j].Name = $"@param{i++}";
                }
            }
        }

        protected virtual void AddCommandParams(SqlCommand cmd, List<Condition> listConditions) {
            cmd.Parameters.Clear();

            foreach (Condition condition in listConditions) {
                if (!String.IsNullOrEmpty(condition.Name) && !(condition.Value is Column)) {
                    cmd.Parameters.Add(new SqlParameter(condition.Name, condition.Value ?? DBNull.Value));
                }
            }
        }

        public string ToRawQuery() {
            var listConditions = GetConditions();

            AssignParamNames(listConditions);

            return ToPlainQuery();
        }

        /// <summary>
        /// ToRawQuery but without assigning param names
        /// </summary>
        /// <returns></returns>
        public virtual string ToPlainQuery() {
            return FromTable;
        }

        public string FromTableOrQuery() {
            if (FromTable == null && FromQuery == null)
                return null;

            return FromTable == null ? FromQuery.Item1.ToTableOrQuery() : FromTable;
        }

        public string FromTableOrAlias() {
            if (FromTable == null && FromQuery == null)
                return null;

            return FromTable == null ? FromQuery.Item2 : FromTable;
        }

        protected SqlCommand GetSqlCommand(SqlTransaction trans = null) {
            SqlCommand cmd = trans == null ? new SqlCommand("", Connection) : new SqlCommand("", Connection, trans);

            cmd.CommandTimeout = Timeout;

            return cmd;
        }
    }
}

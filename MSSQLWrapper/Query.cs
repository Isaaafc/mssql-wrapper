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
        public List<Tuple<SelectQuery, Condition>> ListJoin { get; set; }

        public string JoinString {
            get {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < ListJoin.Count; i++) {
                    var tuple = ListJoin[i];

                    sb.AppendLine("JOIN")
                      .AppendFormat("{0}{1}", tuple.Item1.ToTableOrQuery(), String.IsNullOrEmpty(tuple.Item1.Alias) ? "" : $" AS {tuple.Item1.Alias}")
                      .AppendLine();

                    sb.AppendFormat(" ON {0}", tuple.Item2.ToString())
                      .AppendLine();
                }

                return sb.ToString();
            }
        }

        public BaseQuery(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout) {
            Connection = connection;

            Timeout = timeout;

            ListJoin = new List<Tuple<SelectQuery, Condition>>();

            FromTable = fromTable;
        }

        protected BaseQuery Clone() {
            BaseQuery query = new BaseQuery(FromTable, Connection, Timeout);

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
        protected virtual List<Condition> GetConditions() {
            var listConditions = new List<Condition>();

            if (WhereCondition != null) {
                listConditions.AddRange(WhereCondition.GetAllConditions());
            }

            /// All Conditions from Join
            listConditions.AddRange(ListJoin.Select(r => r.Item2.GetAllConditions()).SelectMany(r => r));

            /// All Conditions from joined Queries
            listConditions.AddRange(ListJoin.Select(r => r.Item1.GetConditions()).SelectMany(r => r));

            /// All Conditions from FromQuery
            if (FromQuery != null) {
                listConditions.AddRange(FromQuery.Item1.GetConditions());
            }

            return listConditions;
        }

        protected void AssignParamNames(List<Condition> listConditions) {
            for (int i = 0; i < listConditions.Count; i++) {
                listConditions[i].Name = $"@param{i}";
            }
        }

        protected virtual void AddCommandParams(SqlCommand cmd, List<Condition> listConditions) {
            cmd.Parameters.Clear();

            foreach (Condition condition in listConditions) {
                if (!String.IsNullOrEmpty(condition.Name) && condition.Value != null && !(condition.Value is Column)) {
                    cmd.Parameters.Add(new SqlParameter(condition.Name, condition.Value));
                }
            }
        }

        public string ToRawQuery() {
            var listConditions = GetConditions();

            AssignParamNames(listConditions);

            return ToRawQuery(listConditions);
        }

        protected virtual string ToRawQuery(List<Condition> listConditions) {
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

        protected SqlCommand GetSqlCommand() {
            SqlCommand cmd = new SqlCommand("", Connection);

            cmd.CommandTimeout = Timeout;

            return cmd;
        }

        protected SqlCommand GetSqlCommand(SqlTransaction trans) {
            SqlCommand cmd = new SqlCommand("", Connection, trans);

            cmd.CommandTimeout = Timeout;

            return cmd;
        }
    }
}

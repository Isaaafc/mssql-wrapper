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

        public string Alias { get; private set; }
        public int Timeout { get; set; }
        public SqlConnection Connection { get; set; }
        public Condition WhereCondition { get; set; }
        public List<Tuple<SelectQuery, string, Condition>> ListJoin { get; set; }

        public string JoinString {
            get {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < ListJoin.Count; i++) {
                    var tuple = ListJoin[i];

                    sb.AppendLine("JOIN")
                      .AppendFormat("{0}{1}", tuple.Item1.ToQuotedQuery(), String.IsNullOrEmpty(tuple.Item2) ? "" : $" AS {tuple.Item2}")
                      .AppendLine();

                    tuple.Item1.Alias = tuple.Item2;

                    sb.AppendFormat(" ON {0}", tuple.Item3.ToString())
                      .AppendLine();

                    tuple.Item1.Alias = null;
                }

                return sb.ToString();
            }
        }

        public BaseQuery(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout) {
            Connection = connection;

            Timeout = timeout;

            ListJoin = new List<Tuple<SelectQuery, string, Condition>>();

            FromTable = fromTable;
        }

        public Column GetNewColumn(string name, string alias = null) {
            return new Column(query: this, name: name, alias: alias);
        }

        protected virtual void AddCommandParams(SqlCommand cmd) {
            cmd.Parameters.Clear();

            var listConditions = new List<Condition>();

            listConditions.AddRange(WhereCondition.GetAllConditions());
            listConditions.AddRange(ListJoin.Select(r => r.Item3.GetAllConditions()).SelectMany(r => r));

            foreach (Condition condition in listConditions) {
                if (!String.IsNullOrEmpty(condition.Name) && condition.Value != null && !(condition.Value is Column)) {
                    cmd.Parameters.Add(new SqlParameter(condition.Name, condition.Value));
                }
            }
        }

        public virtual string ToRawQuery() {
            return FromTable;
        }

        public string FromTableOrQuery() {
            return FromTable == null ? FromQuery.Item1.ToQuotedQuery() : FromTable;
        }

        public string FromTableOrAlias() {
            return FromTable == null ? FromQuery.Item2 : FromTable;
        }

        protected SqlCommand GetSqlCommand() {
            SqlCommand cmd = new SqlCommand("", Connection);

            cmd.CommandTimeout = Timeout;

            return cmd;
        }
    }
}

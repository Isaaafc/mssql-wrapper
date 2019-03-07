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
    public class BaseQuery<T> where T : BaseQuery<T> {
        /// <summary>
        /// Default timeout in seconds
        /// </summary>
        public const int DefaultTimeout = 30;

        protected string rawQuery;

        private SelectQuery fromQuery;

        private string fromTable;

        public string Alias { get; set; }

        public string RawQuery {
            get {
                return rawQuery;
            }
        }

        public string FromTable {
            get {
                return fromTable;
            }
            set {
                fromQuery = null;

                fromTable = value;
            }
        }

        public SelectQuery FromQuery {
            get {
                return fromQuery;
            }
            set {
                fromTable = null;

                fromQuery = value;
            }
        }

        public int Timeout { get; set; }
        public SqlConnection Connection { get; set; }
        protected Condition<T> WhereCondition { get; set; }
        protected List<Tuple<BaseQuery<T>, Condition<T>>> ListJoin { get; set; }

        public string JoinString {
            get {
                StringBuilder sb = new StringBuilder();

                foreach (var tuple in ListJoin) {
                    sb.AppendLine("JOIN")
                      .AppendFormat("{0}", (tuple.Item1 as SelectQuery).ToQuotedQuery(true))
                      .AppendLine()
                      .AppendFormat(" ON {0}", tuple.Item2.ToString())
                      .AppendLine();
                }

                return sb.ToString();
            }
        }

        public BaseQuery(SqlConnection connection = null, int timeout = DefaultTimeout) {
            Connection = connection;

            Timeout = timeout;

            ListJoin = new List<Tuple<BaseQuery<T>, Condition<T>>>();
        }

        public Column<T> GetNewColumn(string name, string alias = null) {
            return new Column<T>(query: (T)this, name: name, alias: alias);
        }

        public virtual T From(SelectQuery fromQuery) {
            FromQuery = fromQuery;

            return (T)this;
        }

        public virtual T From(string fromTable) {
            FromTable = fromTable;

            return (T)this;
        }

        /// <summary>
        /// Joins queries
        /// </summary>
        /// <param name="targetQuery"></param>
        /// <param name="targetColumns">Columns with Key: target query column name, Value: condition</param>
        /// <returns></returns>
        public virtual T Join(BaseQuery<T> otherQuery, Condition<T> condition) {
            if (!(otherQuery is SelectQuery))
                throw new ArgumentException("otherQuery must be SelectQuery");

            if (condition.Type == ConditionType.Join) {
                
                ListJoin.Add(Tuple.Create(otherQuery, condition));
            } else {
                throw new ArgumentException("ConditionType is invalid. (Condition.Type must be equal to ConditionType.Join)");
            }

            return (T)this;
        }

        public virtual T Join(BaseQuery<T> otherQuery, Operator op, params string[] columns) {
            if (!(otherQuery is SelectQuery))
                throw new ArgumentException("otherQuery must be SelectQuery");

            Condition<T> condition = new Condition<T>(ConditionType.Join);

            if (columns.Length > 0) {
                condition.Column = otherQuery.GetNewColumn(columns[0]);
                condition.Operator = op;
                condition.Value = GetNewColumn(columns[0]);

                for (int i = 1; i < columns.Length; i++) {
                    condition.Append(Conditional.And, new Condition<T>(ConditionType.Join, otherQuery.GetNewColumn(columns[i]), GetNewColumn(columns[i])));
                }

                Join(otherQuery, condition);
            }

            return (T)this;
        }

        public virtual T Join(string otherTable, Condition<T> condition) {
            return Join(new BaseQuery<T>().From(otherTable), condition);
        }

        public virtual T Join(string otherTable, Operator op, params string[] columns) {
            return Join(new BaseQuery<T>().From(otherTable), op, columns);
        }

        public void Where(Condition<T> condition) {
            if (condition.Type == ConditionType.Where) {
                WhereCondition = condition;
            } else {
                throw new ArgumentException("ConditionType is invalid. (Condition.Type must be equal to ConditionType.Where)");
            }
        }

        public void Where(Column<T> column, object value, Operator op) {
            Condition<T> condition = new Condition<T>(ConditionType.Where);

            condition.Column = column;
            condition.Value = value;
            condition.Operator = op;

            WhereCondition = condition;
        }

        protected virtual void AddCommandParams(SqlCommand cmd) {
            cmd.Parameters.Clear();

            var listConditions = new List<Condition<T>>();

            listConditions.AddRange(WhereCondition.GetAllConditions());
            listConditions.AddRange(ListJoin.Select(r => r.Item2.GetAllConditions()).SelectMany(r => r));

            foreach (Condition<T> condition in listConditions) {
                if (!String.IsNullOrEmpty(condition.Name) && condition.Value != null && !(condition.Value is Column<T>)) {
                    cmd.Parameters.Add(new SqlParameter(condition.Name, condition.Value));
                }
            }
        }

        public virtual string ToRawQuery() {
            return FromTable;
        }

        public virtual string FromTableOrQuery() {
            return FromTable == null ? FromQuery.ToQuotedQuery(false) : FromTable;
        }

        protected SqlCommand GetSqlCommand() {
            SqlCommand cmd = new SqlCommand("", Connection);

            cmd.CommandTimeout = Timeout;

            return cmd;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;

namespace MSSQLWrapper.Query {
    public abstract class QueryBuilder<TQuery, TQueryBuilder>
        where TQuery : BaseQuery
        where TQueryBuilder : QueryBuilder<TQuery, TQueryBuilder> {
        public const int DefaultTimeout = 30;

        /// <summary>
        /// Self instance to be returned by fluent functions
        /// </summary>
        protected readonly TQueryBuilder builder = null;
        public TQuery Query { get; protected set; }

        public QueryBuilder() {
            builder = (TQueryBuilder)this;
        }

        public TQueryBuilder From(SelectQuery fromQuery, string alias) {
            Query.FromQuery = Tuple.Create(fromQuery, alias);

            return builder;
        }

        public TQueryBuilder From(string fromTable, string alias = null) {
            if (!String.IsNullOrEmpty(alias)) {
                Query.FromQuery = Tuple.Create(new SelectQuery(fromTable), alias);
            } else {
                Query.FromTable = fromTable;
            }

            return builder;
        }

        /// <summary>
        /// Joins queries
        /// </summary>
        /// <param name="targetQuery"></param>
        /// <param name="targetColumns">Columns with Key: target query column name, Value: condition</param>
        /// <returns></returns>
        public TQueryBuilder Join(SelectQuery otherQuery, string alias, Condition condition) {
            if (condition.Type == ConditionType.Join) {

                Query.ListJoin.Add(Tuple.Create(otherQuery, alias, condition));
            } else {
                throw new ArgumentException("ConditionType is invalid. (Condition.Type must be equal to ConditionType.Join)");
            }

            return builder;
        }

        public TQueryBuilder Join(SelectQuery otherQuery, string alias, Operator op, params string[] columns) {
            Condition condition = new Condition(ConditionType.Join);

            if (columns.Length > 0) {
                condition.Column = otherQuery.GetNewColumn(columns[0]);
                condition.Operator = op;
                condition.Value = Query.GetNewColumn(columns[0]);

                for (int i = 1; i < columns.Length; i++) {
                    condition.Append(Conditional.And, new Condition(ConditionType.Join, otherQuery.GetNewColumn(columns[i]), Query.GetNewColumn(columns[i])));
                }

                Join(otherQuery, alias, condition);
            }

            return builder;
        }

        public TQueryBuilder Join(string otherTable, string alias, Condition condition) {
            return Join(new SelectQuery(otherTable), alias, condition);
        }

        public TQueryBuilder Join(string otherTable, string alias, Operator op, params string[] columns) {
            return Join(new SelectQuery(otherTable), alias, op, columns);
        }

        public TQueryBuilder Where(Condition condition) {
            if (condition.Type == ConditionType.Where) {
                Query.WhereCondition = condition;
            } else {
                throw new ArgumentException("ConditionType is invalid. (Condition.Type must be equal to ConditionType.Where)");
            }

            return builder;
        }

        public TQueryBuilder Where(Column column, Operator op, object value) {
            Condition condition = new Condition(ConditionType.Where);

            condition.Column = column;
            condition.Value = value;
            condition.Operator = op;

            Query.WhereCondition = condition;

            return builder;
        }
    }
}

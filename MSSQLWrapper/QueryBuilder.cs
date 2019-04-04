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
            SelectQuery clone = otherQuery.Clone();
            clone.Alias = alias;

            var columns = condition.GetAllColumns()
                                   .Where(r => r.Query == otherQuery)
                                   .ToList();

            for (int i = 0; i < columns.Count; i++) {
                columns[i].Query = clone;
            }

            Query.ListJoin.Add(Tuple.Create(clone, condition));

            return builder;
        }

        public TQueryBuilder Join(SelectQuery otherQuery, string alias, Operator op, params string[] columns) {
            Condition condition = new Condition();

            if (columns.Length > 0) {
                condition.Column = otherQuery.NewColumn(columns[0]);
                condition.Operator = op;
                condition.Value = Query.NewColumn(columns[0]);

                for (int i = 1; i < columns.Length; i++) {
                    condition.Append(Conditional.And, new Condition(otherQuery.NewColumn(columns[i]), Operator.Equals, Query.NewColumn(columns[i])));
                }

                return Join(otherQuery, alias, condition);
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
            Query.WhereCondition = condition;
            
            return builder;
        }

        public TQueryBuilder Where(Column column, Operator op, object value) {
            Query.WhereCondition = new Condition(column, op, value);

            return builder;
        }

        public Column NewColumn(string name, string alias = null) {
            return Query.NewColumn(name);
        }
    }
}

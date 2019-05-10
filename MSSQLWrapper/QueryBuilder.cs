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
        /// Joins queries. Default inner join
        /// </summary>
        /// <returns></returns>
        public TQueryBuilder Join(JoinType joinType, SelectQuery otherQuery, string alias, Condition condition) {
            SelectQuery clone = otherQuery.Clone();
            clone.Alias = alias;

            var columns = condition.GetAllColumns()
                                   .Where(r => r.Query == otherQuery)
                                   .ToList();

            for (int i = 0; i < columns.Count; i++) {
                columns[i].Query = clone;
            }

            Query.ListJoin.Add(new JoinClause(joinType, clone, condition));

            return builder;
        }

        public TQueryBuilder Join(JoinType joinType, SelectQuery otherQuery, string alias, SqlOperator op, params string[] columns) {
            Condition condition = new Condition();

            if (columns.Length > 0) {
                condition.Column = otherQuery.NewColumn(columns[0]);
                condition.Operator = op;
                condition.Value = Query.NewColumn(columns[0]);

                for (int i = 1; i < columns.Length; i++) {
                    condition.Append(Conditional.And, new Condition(otherQuery.NewColumn(columns[i]), SqlOperator.Equals, Query.NewColumn(columns[i])));
                }

                return Join(joinType, otherQuery, alias, condition);
            }

            return builder;
        }

        public TQueryBuilder Join(JoinType joinType, string otherTable, string alias, Condition condition) {
            return Join(joinType, new SelectQuery(otherTable), alias, condition);
        }

        public TQueryBuilder Join(JoinType joinType, string otherTable, string alias, SqlOperator op, params string[] columns) {
            return Join(joinType, new SelectQuery(otherTable), alias, op, columns);
        }

        public TQueryBuilder Join(SelectQuery otherQuery, string alias, Condition condition) {
            return Join(JoinType.Inner, otherQuery, alias, condition);
        }

        public TQueryBuilder Join(SelectQuery otherQuery, string alias, SqlOperator op, params string[] columns) {
            return Join(JoinType.Inner, otherQuery, alias, op, columns);
        }

        public TQueryBuilder Join(string otherTable, string alias, Condition condition) {
            return Join(JoinType.Inner, otherTable, alias, condition);
        }

        public TQueryBuilder Join(string otherTable, string alias, SqlOperator op, params string[] columns) {
            return Join(JoinType.Inner, otherTable, alias, op, columns);
        }

        public TQueryBuilder Where(Condition condition) {
            Query.WhereCondition = condition;
            
            return builder;
        }

        public TQueryBuilder Where(Column column, SqlOperator op, object value) {
            Query.WhereCondition = new Condition(column, op, value);

            return builder;
        }

        public Column NewColumn(string name, string alias = null) {
            return Query.NewColumn(name, alias);
        }
    }
}

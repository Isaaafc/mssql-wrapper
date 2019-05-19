using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;

namespace MSSQLWrapper.Query {
    public class SelectQueryBuilder : QueryBuilder<SelectQuery, SelectQueryBuilder> {
        public SelectQueryBuilder(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout) 
            : base() {
            Query = new SelectQuery(fromTable, connection, timeout);
        }

        public SelectQueryBuilder Select(params string[] columns) {
            Query.SelectColumns.AddRange(columns.Select(r => Query.NewColumn(r)));

            return builder;
        }

        public SelectQueryBuilder Select(params Column[] columns) {
            Query.SelectColumns.AddRange(columns);

            return builder;
        }

        public SelectQueryBuilder Top(int n) {
            Query.TopN = n;

            return builder;
        }

        public SelectQueryBuilder Distinct() {
            Query.SelectDistinct = true;

            return builder;
        }

        public SelectQueryBuilder Union(SelectQuery otherQuery) {
            Query.ListSetOp.Add(new SetOpClause(SetOpType.Union, otherQuery));

            return builder;
        }

        public SelectQueryBuilder Union(string otherTable) {
            return Union(new SelectQuery(otherTable));
        }

        public SelectQueryBuilder UnionAll(SelectQuery otherQuery) {
            Query.ListSetOp.Add(new SetOpClause(SetOpType.UnionAll, otherQuery));

            return builder;
        }

        public SelectQueryBuilder UnionAll(string otherTable) {
            return UnionAll(new SelectQuery(otherTable));
        }

        public SelectQueryBuilder Intersect(SelectQuery otherQuery) {
            Query.ListSetOp.Add(new SetOpClause(SetOpType.Intersect, otherQuery));

            return builder;
        }

        public SelectQueryBuilder Intersect(string otherTable) {
            return Intersect(new SelectQuery(otherTable));
        }

        public SelectQueryBuilder Except(SelectQuery otherQuery) {
            Query.ListSetOp.Add(new SetOpClause(SetOpType.Except, otherQuery));

            return builder;
        }

        public SelectQueryBuilder Except(string otherTable) {
            return Except(new SelectQuery(otherTable));
        }

        public SelectQueryBuilder GroupBy(params string[] columns) {
            Query.GroupByColumns.AddRange(columns.Select(r => Query.NewColumn(r)));

            return builder;
        }

        public SelectQueryBuilder GroupBy(params Column[] columns) {
            Query.GroupByColumns.AddRange(columns);

            return builder;
        }

        public SelectQueryBuilder Having(Condition condition) {
            Query.HavingCondition = condition;

            return builder;
        }

        public SelectQueryBuilder Having(Column column, SqlOperator op, object value) {
            Query.HavingCondition = new Condition(column, op, value);

            return builder;
        }

        public SelectQueryBuilder OrderBy(params Tuple<Column, Order>[] columns) {
            Query.OrderByColumns = new List<Tuple<Column, Order>>(columns);

            return builder;
        }

        public SelectQueryBuilder OrderBy(params Column[] columns) {
            Query.OrderByColumns = columns.Select(r => Tuple.Create(r, Order.Asc))
                                          .ToList();

            return builder;
        }
    }
}

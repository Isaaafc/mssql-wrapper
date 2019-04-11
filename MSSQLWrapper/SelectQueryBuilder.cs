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

        public SelectQueryBuilder OrderBy(params Tuple<Column, Order>[] columns) {
            Query.OrderByColumns = new List<Tuple<Column, Order>>(columns);

            return builder;
        }

    }
}

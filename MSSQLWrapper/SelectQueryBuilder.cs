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
            Query.SelectColumns.AddRange(columns.Select(r => Query.GetNewColumn(r)));

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
            Query.GroupByColumns.AddRange(columns.Select(r => Query.GetNewColumn(r)));

            return builder;
        }

        public SelectQueryBuilder GroupBy(params Column[] columns) {
            Query.GroupByColumns.AddRange(columns);

            return builder;
        }

        public SelectQueryBuilder OrderBy(params object[] columns) {
            ArgumentException e = new ArgumentException("Invalid arguments: params must be in pairs of (Columns/string, Order)");

            for (int i = 0; i < columns.Length; i += 2) {
                if (i + 1 >= columns.Length || !(columns[i + 1] is Order)) {
                    throw e;
                }

                if (columns[i] is string) {
                    Query.OrderByColumns.Add(Tuple.Create(Query.GetNewColumn((string)columns[i]), (Order)columns[i + 1]));
                } else if (columns[i] is Column) {
                    Query.OrderByColumns.Add(Tuple.Create((Column)columns[i], (Order)columns[i + 1]));
                } else {
                    throw e;
                }
            }

            return builder;
        }

    }
}

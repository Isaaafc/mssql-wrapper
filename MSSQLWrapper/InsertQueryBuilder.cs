using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;
using System.Data.SqlClient;

namespace MSSQLWrapper.Query {
    public class InsertQueryBuilder : QueryBuilder<InsertQuery, InsertQueryBuilder> {
        public InsertQueryBuilder(SqlConnection connection = null, int timeout = DefaultTimeout)
            : base() {
            Query = new InsertQuery(connection, timeout);
        }

        public InsertQueryBuilder Insert(string table, params string[] columns) {
            Query.Table = table;
            Query.InsertColumns = new List<Column>(columns.Select(r => new Column(r)));

            return this;
        }

        public InsertQueryBuilder Values(params object[] values) {
            Query.InsertValues = new List<object>(values);

            return this;
        }

        public InsertQueryBuilder InsertValues(string table, Dictionary<string, object> dict) {
            Query.Table = table;
            Query.InsertColumns = dict.Keys.Select(r => new Column(r)).ToList();
            Query.InsertValues = dict.Values.ToList();

            return this;
        }

        public InsertQueryBuilder IfNotExists(SelectQuery selectQuery) {
            Query.IfNotExistsQuery = selectQuery;

            return this;
        }

        public InsertQueryBuilder ElseUpdate(UpdateQuery updateQuery) {
            Query.ElseUpdateQuery = updateQuery;

            return this;
        }

        public InsertQueryBuilder IfNotExists(params string[] columns) {
            SelectQueryBuilder select = new SelectQueryBuilder(connection: Query.Connection);
            select.From(Query.Table);

            var insertCols = Query.InsertColumns
                                  .Select(r => r.Name)
                                  .ToList();

            try {
                Condition cond = new Condition(new Column(columns[0]), SqlOperator.Equals, Query.InsertValues[insertCols.IndexOf(columns[0])]);

                foreach (string c in columns.Skip(1)) {
                    cond.And(new Column(c), SqlOperator.Equals, Query.InsertValues[insertCols.IndexOf(c)]);
                }

                select.Where(cond);

                Query.IfNotExistsQuery = select.Query;
            } catch (IndexOutOfRangeException) {
                throw new IndexOutOfRangeException("Insert columns and values must be defined before calling this IfNotExists() overload");
            }

            return this;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;

namespace MSSQLWrapper.Query {
    public class CreateQuery : BaseQuery {
        public string Table { get; set; }
        public List<TableConstraint> ListConstraints { get; set; }
        /// <summary>
        /// Tuple of [columnName, dataType, dataType args]
        /// </summary>
        public List<Tuple<string, DataType, int>> ListColumns { get; set; }

        public CreateQuery(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout)
           : base(fromTable, connection, timeout) {
            ListConstraints = new List<TableConstraint>();
            ListColumns = new List<Tuple<string, DataType, int>>();
        }

        public string GetConstraintString() {
            StringBuilder sb = new StringBuilder();

            foreach (TableConstraint constraint in ListConstraints) {
                sb.AppendLine(constraint.ToString());
            }

            return sb.ToString();
        }

        public override string ToRawQuery() {
            StringBuilder sb = new StringBuilder();

            if (FromQuery == null) {
                sb.AppendFormat("CREATE {0} (", Table)
                  .AppendLine();

                foreach (var column in ListColumns) {
                    sb.AppendFormat("{0} {1},", column.Item1, String.Format(column.Item2.GetStringValue(), column.Item3))
                      .AppendLine();
                }

                sb.Append(GetConstraintString());

                sb.AppendLine(");");
            }

            return sb.ToString();
        }
    }
}

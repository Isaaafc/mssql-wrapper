using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;
using System.Data.SqlClient;

namespace MSSQLWrapper.Query {
    public class UpdateQuery : BaseQuery {
        public struct UpdateSet {
            List<Tuple<Column, object>> listSet { get; set; }

            public UpdateSet(params object[] args) {
                listSet = new List<Tuple<Column, object>>();

                for (int i = 0; i < args.Length; i += 2) {
                    listSet.Add(Tuple.Create(args[i] as Column, args[i + 1]));
                }

                /// Validate
                var c = listSet.Select(r => r.Item1.FullName)
                               .Distinct()
                               .Count();

                if (c > 1) {
                    throw new ArgumentException("Left columns must be from the same table");
                }
            }
        }

        public UpdateSet UpdateColumns { get; set; }

        public UpdateQuery(string fromTable = null, SqlConnection connection = null, int timeout = DefaultTimeout)
            : base(fromTable, connection, timeout) {

        }
    }
}

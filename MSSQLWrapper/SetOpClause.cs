using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;

namespace MSSQLWrapper.Query {
    /// <summary>
    /// Represents a set operation (Union, Intersect, Except) with another query
    /// </summary>
    public class SetOpClause {
        public SetOpType Op { get; set; }
        public SelectQuery Query { get; set; }

        public SetOpClause(SetOpType op, SelectQuery query) {
            Op = op;
            Query = query;
        }
    }
}

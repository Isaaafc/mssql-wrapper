using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums; 

namespace MSSQLWrapper.Query {
    public class JoinClause {
        public JoinType Type { get; set; }
        public SelectQuery Query { get; set; }
        public Condition Condition { get; set; }

        public JoinClause(JoinType type, SelectQuery query, Condition condition) {
            Type = type;
            Query = query;
            Condition = condition;
        }
    }
}

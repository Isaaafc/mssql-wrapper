using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper.Query {
    public class Column<T> where T : BaseQuery<T> {
        public BaseQuery<T> Query { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; } 
        public string ConditionName {
            get {
                /// TODO: make it work across schemas
                return (Query is SelectQuery) ? $"{(Query as SelectQuery).Alias}.[{Name}]" : $"[{Name}]";
            }
        }
        public string AliasedName {
            get {
                return ConditionName + (String.IsNullOrEmpty(Alias) ? "" : $" AS {Alias}");
            }
        }

        public Column(BaseQuery<T> query, string name, string alias = null) {
            Query = query;
            Name = name;
            Alias = alias;
        }
    }
}

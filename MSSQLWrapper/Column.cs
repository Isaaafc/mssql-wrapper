using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper.Query {
    public class Column {
        public BaseQuery Query { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; } 
        public string FullName {
            get {
                string table = String.IsNullOrEmpty(Query.Alias) ? Query.FromTableOrAlias() : Query.Alias;

                return $"{table}.[{Name}]";
            }
        }
        public string AliasedName {
            get {
                return FullName + (String.IsNullOrEmpty(Alias) ? "" : $" AS {Alias}");
            }
        }

        public Column(BaseQuery query, string name, string alias = null) {
            Query = query;
            Name = name;
            Alias = alias;
        }
    }
}

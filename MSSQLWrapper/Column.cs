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
                if (Query == null || Query.FromTableOrAlias() == null) {
                    return Name;
                } else if (String.IsNullOrEmpty(Query.Alias)) {
                    return $"{Query.FromTableOrAlias()}.[{Name}]";
                } else {
                    return $"{Query.Alias}.[{Name}]";
                }
            }
        }

        public string AliasedName {
            get {
                return FullName + (String.IsNullOrEmpty(Alias) ? "" : $" AS {Alias}");
            }
        }

        public Column(string name, string alias = null) {
            Name = name;
            Alias = alias;
        }

        public Column(BaseQuery query, string name, string alias = null)
            : this(name, alias) {
            Query = query;
        }
    }
}

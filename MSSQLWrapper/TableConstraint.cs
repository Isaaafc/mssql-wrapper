using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;

namespace MSSQLWrapper.Query {
    public class TableConstraint {
        public string Name { get; set; }
        public string[] Columns { get; set; }
        public Constraint Constraint { get; set; }
        public string Arg { get; set; }

        public TableConstraint(string name, string[] columns, Constraint constraint, string arg = null) {
            Name = name;
            Columns = columns;
            Constraint = constraint;
            Arg = arg;
        }

        /// <summary>
        /// Shorthand for creating foreign key with Column class
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="refColumn"></param>
        /// <returns></returns>
        public static TableConstraint ForeignKey(string name, string columnName, Column refColumn) {
            return new TableConstraint(name, new string[] { columnName }, Constraint.ForeignKey, refColumn.FullName);
        }

        /// <summary>
        /// Shorthand for creating Check condition
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static TableConstraint Check(string name, Condition condition) {
            return new TableConstraint(name, null, Constraint.ForeignKey, condition.ToString());
        }

        public static TableConstraint Index(string name, string[] columns, bool unique = false) {
            return new TableConstraint(name, columns, unique ? Constraint.UniqueIndex : Constraint.Index);
        }

        public override string ToString() {
            if (Constraint == Constraint.ForeignKey) {
                return String.Format("CONSTRAINT {0} {1} REFERENCES {2}", Name, Constraint.GetStringValue(), String.Join(", ", Columns));
            } else {
                return String.Format("CONSTRAINT {0} {1} ({2})", Name, Constraint.GetStringValue(), Arg ?? String.Join(", ", Columns));
            }
        }
    }
}

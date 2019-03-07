using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;

namespace MSSQLWrapper.Query {
    /// <summary>
    /// Represents a full condition clause such as ((a = b and b = c) or c = d ...) and so on
    /// </summary>
    public class Condition<T> where T : BaseQuery<T> {
        private static int count = 0;
        public ConditionType Type { get; }
        public Operator Operator { get; set; }
        public Column<T> Column { get; set; }
        /// <summary>
        /// Name of param to be inserted in SQLCommand
        /// </summary>
        public string Name { get; private set; }
        public object Value { get; set; }

        public List<Tuple<Conditional, Condition<T>>> ListConditions { get; private set; }

        public Condition() {
            ListConditions = new List<Tuple<Conditional, Condition<T>>>();
            Operator = Operator.Equals;
            Name = $"@param{count++}";
        }

        public Condition(ConditionType type, object value = null)
            : this() {
            Type = type;
            Value = value;
        }

        public Condition(ConditionType type, Column<T> column, object value = null)
            : this(type, value) {
            Column = column;
        }

        public Condition<T> Append(Conditional cond, Condition<T> otherCondition) {
            ListConditions.Add(Tuple.Create(cond, otherCondition));

            return this;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            sb.Append("(");

            sb.AppendFormat("{0} {1}", Column.ConditionName, Operator.GetStringValue());

            if (Operator != Operator.IsNotNull && Operator != Operator.IsNull) {
                sb.Append(" " + (Value is Column<T> ? ((Column<T>)Value).ConditionName : Name));
            }

            foreach (var tuple in ListConditions) {
                sb.AppendFormat("{0} {1} {2}", Environment.NewLine, tuple.Item1.GetStringValue(), tuple.Item2.ToString());
            }

            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// Gets a flattened list of all conditions involved in this instance
        /// </summary>
        /// <returns></returns>
        public List<Condition<T>> GetAllConditions() {
            var list = new List<Condition<T>>();

            list.Add(this);

            list.AddRange(
                ListConditions
                .Select(r => r.Item2.GetAllConditions())
                .SelectMany(r => r)
            );

            return list;
        }
    }
}

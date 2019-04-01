using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSSQLWrapper.Enums;

namespace MSSQLWrapper.Query {
    /// <summary>
    /// Represents a full condition clause such as ((a = b and b = c) or c = d ...)
    /// </summary>
    public class Condition {
        private static int count = 0;
        public Operator Operator { get; set; }
        public Column Column { get; set; }
        /// <summary>
        /// Name of param to be inserted in SQLCommand
        /// </summary>
        public string Name { get; private set; }
        public object Value { get; set; }

        public List<Tuple<Conditional, Condition>> ListConditions { get; private set; }

        public Condition() {
            ListConditions = new List<Tuple<Conditional, Condition>>();
            Operator = Operator.Equals;
            Name = $"@param{count++}";
        }

        public Condition(Column column, Operator op, object value = null)
            : this() {
            Column = column;
            Operator = op;
            Value = value;
        }

        public Condition Append(Conditional cond, Condition otherCondition) {
            ListConditions.Add(Tuple.Create(cond, otherCondition));

            return this;
        }

        public Condition And(Column column, Operator op, object value = null) {
            Condition otherCondition = new Condition(column, op, value);

            return Append(Conditional.And, otherCondition);
        }

        public Condition And(Condition otherCondition) {
            return Append(Conditional.And, otherCondition);
        }

        public Condition Or(Column column, Operator op, object value = null) {
            Condition otherCondition = new Condition(column, op, value);

            return Append(Conditional.Or, otherCondition);
        }

        public Condition Or(Condition otherCondition) {
            return Append(Conditional.Or, otherCondition);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            sb.Append("(");

            sb.AppendFormat("{0} {1}", Column.FullName, Operator.GetStringValue());

            if (Operator != Operator.IsNotNull && Operator != Operator.IsNull) {
                sb.Append(" " + (Value is Column ? ((Column)Value).FullName : Name));
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
        public List<Condition> GetAllConditions() {
            var list = new List<Condition>();

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

﻿using System;
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
        public SqlOperator Operator { get; set; }
        public Column Column { get; set; }

        /// <summary>
        /// Name of param to be inserted in SQLCommand
        /// </summary>
        public string Name { get; internal set; }
        public object Value { get; set; }

        public List<Tuple<Conditional, Condition>> ListConditions { get; private set; }

        public Condition() {
            ListConditions = new List<Tuple<Conditional, Condition>>();
            Operator = SqlOperator.Equals;
        }

        public Condition(Column column, SqlOperator op, object value = null)
            : this() {
            Column = column;
            Operator = op;
            Value = value;
        }

        /// <summary>
        /// Constant condition
        /// </summary>
        /// <param name="val"></param>
        public Condition(bool val)
            : this() {
            Column = new Column("1");
            Operator = SqlOperator.Equals;
            Value = new Column(val ? "1" : "0");
        }

        public Condition Append(Conditional cond, Condition otherCondition) {
            ListConditions.Add(Tuple.Create(cond, otherCondition));

            return this;
        }

        public Condition And(Column column, SqlOperator op, object value = null) {
            Condition otherCondition = new Condition(column, op, value);

            return Append(Conditional.And, otherCondition);
        }

        public Condition And(Condition otherCondition) {
            return Append(Conditional.And, otherCondition);
        }

        public Condition Or(Column column, SqlOperator op, object value = null) {
            Condition otherCondition = new Condition(column, op, value);

            return Append(Conditional.Or, otherCondition);
        }

        public Condition Or(Condition otherCondition) {
            return Append(Conditional.Or, otherCondition);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            sb.Append("(");

            sb.AppendFormat("{0} {1}", Column.FullName, (ValueIsNull() ? SqlOperator.IsNull : Operator).GetStringValue());

            if (Operator != SqlOperator.IsNotNull && Operator != SqlOperator.IsNull && !ValueIsNull()) {
                sb.Append(" " + (Value is Column ? ((Column)Value).FullName : Name));
            }

            foreach (var tuple in ListConditions) {
                sb.AppendFormat(" {0} {1}", tuple.Item1.GetStringValue(), tuple.Item2.ToString());
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

        /// <summary>
        /// Gets all columns involved in this instance excluding null values
        /// </summary>
        /// <returns></returns>
        public List<Column> GetAllColumns() {
            var list = new List<Column>();

            list.Add(Column);
            list.Add(Value as Column);

            list.AddRange(
                ListConditions
                .Select(r => r.Item2.GetAllColumns())
                .SelectMany(r => r)
            );

            return list.Where(r => r != null)
                       .ToList();
        }

        public bool ValueIsNull() {
            return (Value == null || Value == DBNull.Value) && Operator != SqlOperator.IsNotNull;
        }
    }
}

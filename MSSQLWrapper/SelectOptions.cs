using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper {
    public class SelectOptions {
        private List<string> orderByList;
        public bool Distinct { get; set; }
        public List<string> OrderByAsc {
            get {
                return orderByList;
            }
            set {
                orderByList = value == null ? null : value.Distinct().ToList();

                if (OrderBy == null)
                    OrderBy = new Dictionary<string, Enums.Order>();

                if (orderByList != null)
                    foreach (string s in orderByList)
                        OrderBy[s] = Enums.Order.Asc;
            }
        }

        public Dictionary<string, Enums.Order> OrderBy { get; set; }
        public int Top { get; set; }
        public Dictionary<string, Enums.Operator> Operators { get; set; }

        /// <summary>
        /// Function to simplify definition for multiple values and operators in calls to functions in DbFunctions
        /// </summary>
        /// <param name="args">list of args in the order [value1], [operator1], [value2], [operator2], ...</param>
        /// <returns></returns>
        public static List<Tuple<object, Enums.Operator>> GetOperatorTuples(params object[] args) {
            var list = new List<Tuple<object, Enums.Operator>>();

            for (int i = 0; i < args.Count(); i += 2) {
                if (args.Count() > i + 1 && args[i + 1].GetType().Equals(typeof(Enums.Operator))) {
                    list.Add(Tuple.Create(args[i], (Enums.Operator)args[i + 1]));
                } else if (args.Count() <= i + 1) {
                    throw new ArgumentException("Number of arguments must be divisible by 2");
                } else if (!args[i + 1].GetType().Equals(typeof(Enums.Operator))) {
                    throw new ArgumentException("Object value must be followed by Enums.Operator enum", args[i].ToString());
                }
            }

            return list;
        }

        public SelectOptions() {
            Distinct = false;
            OrderBy = null;
            OrderByAsc = null;
            Top = -1;
        }

        public SelectOptions(bool distinct, int top, List<string> orderByList) : this() {
            Distinct = distinct;
            Top = top;
            OrderByAsc = orderByList;
        }

        public SelectOptions(bool distinct, int top, Dictionary<string, Enums.Order> orderBy) : this() {
            Distinct = distinct;
            Top = top;
            OrderBy = orderBy;
        }

        public SelectOptions(Enums.QuickOption qo) : this() {            
            switch (qo) {
                case Enums.QuickOption.Distinct:
                    Distinct = true;
                    break;
                case Enums.QuickOption.Top10:
                    Top = 10;
                    break;
                case Enums.QuickOption.Top50:
                    Top = 50;
                    break;
                case Enums.QuickOption.Top100:
                    Top = 100;
                    break;
                case Enums.QuickOption.Top1000:
                    Top = 1000;
                    break;
                default:
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper.Enums {
    public class EnumArgsAttribute : Attribute {
        public EnumArgsAttribute(bool hasArgs) {
            HasArgs = hasArgs;
        }

        public bool HasArgs { get; set; }
    }
}

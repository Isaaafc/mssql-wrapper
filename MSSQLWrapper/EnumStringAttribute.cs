using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper.Enums {
    public class EnumStringAttribute : Attribute {
        public EnumStringAttribute(string stringValue) {
            StringValue = stringValue;
        }

        public string StringValue { get; set; }
    }
}

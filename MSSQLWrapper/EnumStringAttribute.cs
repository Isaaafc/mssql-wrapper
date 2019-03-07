using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper.Enums {
    public class EnumStringAttribute : Attribute {
        public EnumStringAttribute(string stringValue) {
            this.stringValue = stringValue;
        }

        private string stringValue;

        public string StringValue {
            get {
                return stringValue;
            }
            set {
                stringValue = value;
            }
        }
    }
}

using MSSQLWrapper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper {
    public static class Extensions {
        public static string GetStringValue(this Enum value) {
            Type type = value.GetType();

            FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes  
            EnumStringAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(EnumStringAttribute), false) as EnumStringAttribute[];

            // Return the first if there was a match.  
            return attribs.Length > 0 ? attribs[0].StringValue : null;
        }
    }
}

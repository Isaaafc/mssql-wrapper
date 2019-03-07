using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLWrapper.Enums {
    public enum Operator {
        [EnumString("=")]
        Equals,
        [EnumString(">")]
        Gt,
        [EnumString(">=")]
        Gte,
        [EnumString("<")]
        Lt,
        [EnumString("<=")]
        Lte,
        [EnumString("!=")]
        NotEquals,
        [EnumString("IS NULL")]
        IsNull,
        [EnumString("IS NOT NULL")]
        IsNotNull,
        [EnumString("LIKE")]
        Like
    }

    public enum Order {
        [EnumString("ASC")]
        Asc,
        [EnumString("DESC")]
        Desc
    }

    public enum QuickOption {
        None,
        Distinct,
        Top10,
        Top50,
        Top100,
        Top1000
    }

    public enum ConditionType {
        Where,
        Join,
        LeftJoin,
        RightJoin,
        OuterJoin
    }

    public enum Conditional {
        [EnumString("AND")]
        And,
        [EnumString("OR")]
        Or
    }
}

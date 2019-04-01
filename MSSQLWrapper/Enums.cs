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

    public enum Conditional {
        [EnumString("AND")]
        And,
        [EnumString("OR")]
        Or
    }

    public enum DataType {
        [EnumString("char({0})")]
        Char,
        [EnumString("varchar({0})")]
        VarChar,
        [EnumString("varchar(max)")]
        VarCharMax,
        [EnumString("text")]
        Text,
        [EnumString("nchar({0})")]
        NChar,
        [EnumString("nvarchar({0})")]
        NVarChar,
        [EnumString("nvarchar(max)")]
        NVarCharMax,
        [EnumString("ntext")]
        NText,
        [EnumString("binary({0})")]
        Binary,
        [EnumString("varbinary")]
        VarBinary,
        [EnumString("varbinary(max)")]
        VarBinaryMax,
        [EnumString("image")]
        Image,
        [EnumString("bit")]
        Bit,
        [EnumString("tinyint")]
        TinyInt,
        [EnumString("smallint")]
        SmallInt,
        [EnumString("int")]
        Int,
        [EnumString("bigint")]
        BigInt,
        [EnumString("decimal({0}, {1})")]
        Decimal,
        [EnumString("numeric({0}, {1})")]
        Numeric,
        [EnumString("smallmoney")]
        SmallMoney,
        [EnumString("money")]
        Money,
        [EnumString("float({0})")]
        Float,
        [EnumString("real")]
        Real,
        [EnumString("datetime")]
        DateTime,
        [EnumString("datetime2")]
        DateTime2,
        [EnumString("smalldatetime")]
        SmallDateTime,
        [EnumString("date")]
        Date,
        [EnumString("time")]
        Time,
        [EnumString("datetimeoffset")]
        DateTimeOffset,
        [EnumString("timestamp")]
        Timestamp,
        [EnumString("sql_variant")]
        SQLVariant,
        [EnumString("uniqueidentifier")]
        UniqueIdentifier,
        [EnumString("xml")]
        Xml,
        [EnumString("cursor")]
        Cursor,
        [EnumString("table")]
        Table
    }

    public enum Constraint {
        [EnumString("NOT NULL")]
        NotNull,
        [EnumString("UNIQUE")]
        Unique,
        [EnumString("PRIMARY KEY")]
        PrimaryKey,
        [EnumString("FOREIGN KEY")]
        [EnumArgs(true)]
        ForeignKey,
        [EnumString("CHECK")]
        [EnumArgs(true)]
        Check,
        [EnumString("DEFAULT")]
        [EnumArgs(true)]
        Default,
        [EnumString("INDEX")]
        [EnumArgs(true)]
        Index,
        [EnumString("UNIQUE INDEX")]
        [EnumArgs(true)]
        UniqueIndex
    }

    public enum Function {

    }
}

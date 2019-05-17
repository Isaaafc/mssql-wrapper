# MSSQLWrapper

Build and execute queries directly in C# code with simple syntax.

## Usage

There are currently 4 types of queries implemented: Select, Insert, Update, Create. They all work in similar ways. 

Builder classes are used to build queries.

```csharp
SelectQueryBuilder select = new SelectQueryBuilder(sqlConnection);
select.From("[dbo].[Table1]");

Console.WriteLine(select.Query.ToRawQuery());
```

Output

```sql
SELECT
 *
FROM
[dbo].[Table1]
```

The Column class represents a column from a table / query. The Condition class defines the relation between Columns / values. 

```csharp
select.Join(/// Table name or SelectQuery class
            "[dbo].[Table2]",
            /// Alias for Table2
            "t2",
            /// Join condition
            new Condition(new Column("col1"), Operator.Equals, select.NewColumn("col1"))
        )
        .Where(select.NewColumn("col2"), Operator.IsNotNull);
```

Output

```sql
SELECT
 *
FROM
[dbo].[Table1]
JOIN
[dbo].[Table2]
 AS t2
 ON ([col1] = [dbo].[Table1].[col1])
WHERE
 ([dbo].[Table1].[col2] IS NOT NULL)
```

Query execution can be done directly as well. 

```csharp
/// DataTable output
DataTable dt = select.Query.ExecuteQuery();
/// Dynamic IEnumerable output
IEnumerable<dynamic> results = select.Query.ExecuteDynamic();
/// Optional SQLTransaction param for queries except Select
int rowsAffected = insert.Query.ExecuteQuery(transaction);
```

## Other examples

### Create
```csharp
/// Simple creation
CreateQueryBuilder create = new CreateQueryBuilder(connection: sqlConnection);

create.Create("#TestTable1");
create.AddColumn("col1", DataType.Int);
create.AddColumn("col2", DataType.NVarChar, 50);
create.AddIdentity("Id", 1, 1);
```

Output

```sql
CREATE TABLE #TestTable1 (
 col1 int,
 col2 nvarchar(50),
 ID int IDENTITY(1, 1),
);
```

```csharp
/// Create a table from another table
CreateQueryBuilder create2 = new CreateQueryBuilder(connection: sqlConnection);
create2.Create("#TestTable2")
       .From("#TestTable1");
```

Output

```sql
SELECT
 *
INTO
 #TestTable2
FROM
#TestTable1
```

```csharp
/// Create from select query
SelectQueryBuilder select = new SelectQueryBuilder(connection: conn);

select.From("#TestTable1", "t1");

select.Select(select.NewColumn("col1"),
            select.NewColumn("col2"),
            select.NewColumn("Id"),
            new Column("t2.[col1]", "col1_2"),
            new Column("t2.[col2]", "col2_2"),
            new Column("t2.[Id]", "Id_2"))
      .Join("#TestTable2"
            "t2",
            SqlOperator.Equals,
            "Id");

CreateQueryBuilder create = new CreateQueryBuilder(conn);

create.Create("#TableFromQuery")
        .From(select.Query, "t1");
```

### Insert
```csharp
InsertQueryBuilder insert = new InsertQueryBuilder(connection: sqlConnection);

insert.Insert(/// Table name
              "#TestTable1",
              /// Column names
              "col1",
              "col2")
      .Values(/// Values
              1,
              "AA");
```

Output

```sql
INSERT INTO #TestTable1 (
 [col1],
 [col2]
)
VALUES (
 @insP0,
 @insP1
)
```

### Update
```csharp
UpdateQueryBuilder update = new UpdateQueryBuilder(connection: conn);

/// Either Dictionary of { Column, object } or { string, object }
var updateCols = new Dictionary<string, object>() {
    { "col1", null },
    { "col2", "GGG" }
};

update.Update("#TestTable1", updateCols)
      .Where(new Condition(new Column("ID"), SqlOperator.Lt, 3)
              .And(new Column("col1"), SqlOperator.Equals, 1));
```

Output

```sql
UPDATE #TestTable1
SET
 col1 = @updP0,
 col2 = @updP1
WHERE
 (ID < @param0
 AND (col1 = @param1))
```

```csharp
/// Update with join operations
var updateCols = new Dictionary<Column, object>() {
    { new Column("t2.[col1]"), null },
    { new Column("t2.[col2]"), "GGG" }
};

update.Update("t2", updateCols)
      .From("#TestTable1", "t1")
      .Join("#TestTable2",
          "t2",
          SqlOperator.Equals,
          "Id")
      .Where(new Condition(update.NewColumn("Id"), SqlOperator.Lt, 3)
              .And(update.NewColumn("col1"), SqlOperator.Equals, 1));
```

Output

```sql
UPDATE t2
SET
 t2.[col1] = @updP0,
 t2.[col2] = @updP1
FROM
#TestTable1
 AS t1
JOIN
#TestTable2
 AS t2
 ON (t2.[ID] = t1.[ID])
WHERE
 (t1.[ID] < @param0
 AND (t1.[col1] = @param1))
```
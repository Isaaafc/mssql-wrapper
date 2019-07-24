# MSSQLWrapper

Build and execute queries directly in C# code with simple syntax. Get it via Nuget: https://www.nuget.org/packages/MSSQLWrapper

## Usage

There are currently 5 types of queries implemented: Select, Insert, Update, Create, and Delete. They all work in similar ways. 

Builder classes are used to build queries.

*The outputs shown in this section are formatted for easier reading, while the actual outputs are not after version 1.0.0.1*

```csharp
SelectQueryBuilder select = new SelectQueryBuilder(conn);
select.From("[dbo].[Table1]");

Console.WriteLine(select.Query.ToRawQuery());
```

Output

```sql
SELECT *
FROM [dbo].[Table1]
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
SELECT *
FROM [dbo].[Table1]
JOIN [dbo].[Table2] AS t2 ON ([col1] = [dbo].[Table1].[col1])
WHERE ([dbo].[Table1].[col2] IS NOT NULL)
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
CreateQueryBuilder create = new CreateQueryBuilder(connection: conn);

create.Create("[dbo].[Table1]");
create.AddColumn("col1", DataType.Int);
create.AddColumn("col2", DataType.NVarChar, 50);
create.AddIdentity("Id", 1, 1);
```

Output

```sql
CREATE TABLE [dbo].[Table1] (
      col1 int,
      col2 nvarchar(50),
      Id int IDENTITY(1, 1),
);
```

```csharp
/// Create a table from another table
CreateQueryBuilder create2 = new CreateQueryBuilder(connection: conn);
create2.Create("[dbo].[Table2]")
       .From("[dbo].[Table1]");
```

Output

```sql
SELECT * INTO [dbo].[Table2]
FROM [dbo].[Table1]
```

```csharp
/// Create from select query
SelectQueryBuilder select = new SelectQueryBuilder(connection: conn);

select.From("[dbo].[Table1]", "t1");

select.Select(select.NewColumn("col1"),
            select.NewColumn("col2"),
            select.NewColumn("Id"),
            new Column("t2.[col1]", "col1_2"),
            new Column("t2.[col2]", "col2_2"),
            new Column("t2.[Id]", "Id_2"))
      .Join("[dbo].[Table2]"
            "t2",
            SqlOperator.Equals,
            "Id");

CreateQueryBuilder create = new CreateQueryBuilder(conn);

create.Create("[dbo].[TableFromQuery]")
      .From(select.Query, "t1");
```

```sql
SELECT * INTO [dbo].[TableFromQuery]
FROM
  (SELECT t1.[col1],
          t1.[col2],
          t1.[Id],
          t2.[col1] AS col1_2,
          t2.[col2] AS col2_2,
          t2.[Id] AS Id_2
   FROM [dbo].[Table1] AS t1
   JOIN [dbo].[Table2] AS t2 ON (t2.[Id] = t1.[Id])) AS fr
```

### Insert
```csharp
InsertQueryBuilder insert = new InsertQueryBuilder(connection: conn);

insert.Insert(/// Table name
              "[dbo].[Table1]",
              /// Column names
              "col1",
              "col2")
      .Values(/// Values
              1,
              "AA");
```

Output

```sql
INSERT INTO [dbo].[Table1] ([col1], [col2])
VALUES (@insP0, @insP1)
```

```csharp
/// Insert from a dictionary
/// Key: column name, Value: value to be inserted 
var dict = new Dictionary<string, object>();
dict["col1"] = 1;
dict["col2"] = "AA";

InsertQueryBuilder insert = new InsertQueryBuilder(connection: conn);

insert.InsertValues("[dbo].[Table1]", dict);
```

```sql
INSERT INTO [dbo].[Table1] (col1, col2)
VALUES (@insP0, @insP1)
```

```csharp
/// Insert if not exists else update
InsertQueryBuilder insert = new InsertQueryBuilder(connection: conn);

insert.Insert(/// Table name
              "[dbo].[Table1]",
              /// Column names
              "col1",
              "col2")
      .Values(/// Values
              1,
              "AA")
      .IfNotExists("col1", "col2")
      .ElseUpdate(updateQuery);
```

```sql
IF NOT EXISTS
  (SELECT *
   FROM [dbo].[Table1]
   WHERE (col1 = @param0
          AND col2 = @param1) )
INSERT INTO [dbo].[Table1] (col1, col2)
VALUES (@insP0, @insP1)
ELSE 
UPDATE ...
```

### Update
```csharp
UpdateQueryBuilder update = new UpdateQueryBuilder(connection: conn);

/// Either Dictionary of { Column, object } or { string, object }
var updateCols = new Dictionary<string, object>() {
    { "col1", null },
    { "col2", "GGG" }
};

update.Update("[dbo].[Table1]", updateCols)
      .Where(new Condition(new Column("ID"), SqlOperator.Lt, 3)
              .And(new Column("col1"), SqlOperator.Equals, 1));
```

Output

```sql
UPDATE [dbo].[Table1]
SET col1 = @updP0,
    col2 = @updP1
WHERE (ID < @param0
       AND (col1 = @param1))
```

```csharp
/// Update with join operations
var updateCols = new Dictionary<Column, object>() {
    { new Column("t2.[col1]"), null },
    { new Column("t2.[col2]"), "GGG" }
};

update.Update("t2", updateCols)
      .From("[dbo].[Table1]", "t1")
      .Join("[dbo].[Table2]",
          "t2",
          SqlOperator.Equals,
          "Id")
      .Where(new Condition(update.NewColumn("Id"), SqlOperator.Lt, 3)
              .And(update.NewColumn("col1"), SqlOperator.Equals, 1));
```

Output

```sql
UPDATE t2
SET t2.[col1] = @updP0,
    t2.[col2] = @updP1
FROM [dbo].[Table1] AS t1
JOIN [dbo].[Table2] AS t2 ON (t2.[ID] = t1.[ID])
WHERE (t1.[ID] < @param0
       AND (t1.[col1] = @param1))
```

### Delete
```csharp
/// Simple deletion
DeleteQueryBuilder delete = new DeleteQueryBuilder(connection: conn);

delete.From(testTables[0])
      .Where(delete.NewColumn("Id"), SqlOperator.Gte, 2);
```

Output

```sql
DELETE
FROM [dbo].[Table1]
WHERE ([dbo].[Table1].[Id] >= @param0)
```

```csharp
/// Delete from queries
DeleteQueryBuilder delete = new DeleteQueryBuilder(connection: conn);

delete.Delete("t2")
      .From("[dbo].[Table1]", "t1")
      .Join("[dbo].[Table2]",
          "t2",
          SqlOperator.Equals,
          "Id"
      )
      .Where(delete.NewColumn("Id"), SqlOperator.Gte, 2);
```

Output

```sql
DELETE t2
FROM [dbo].[Table1] AS t1
JOIN [dbo].[Table2] AS t2 ON (t2.[Id] = t1.[Id])
WHERE (t1.[Id] >= @param0)
```
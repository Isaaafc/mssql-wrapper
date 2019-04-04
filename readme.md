# MSSQLWrapper

This wrapper allows you to build and execute queries directly in C# code, so that it is easier to view and format. The main goal is to make the code easy to translate from SQL so developers won't have to learn a new syntax just to write queries. 

## Usage

There are currently 4 types of queries implemented: Select, Insert, Update, Create. They all work in similar ways. 

Builders are used to build queries.

```
SelectQueryBuilder select = new SelectQueryBuilder(sqlConnection);
select.From("[dbo].[Table1]");

Console.WriteLine(select.Query.ToRawQuery());
```

Output

```
SELECT
 *
FROM
[dbo].[Table1]
```

The Column class represents a column from a table / query. The Condition class defines the relation between Columns. 

```
select.Join(
            /// Table name or SelectQuery class
            "[dbo].[Table2]",
            /// Alias for Table2
            "t2",
            new Condition(new Column("Column2"),
            /// Join condition
            Operator.Equals, select.NewColumn("Column1"))
        )
        .Where(new Condition(select.NewColumn("Column1"), Operator.IsNotNull));
```

Output

```
SELECT
 *
FROM
[dbo].[Table1]
JOIN

[dbo].[Table2]
 AS t2
 ON ([Column2] = [dbo].[Table1].[Column1])
WHERE
 ([dbo].[Table1].[Column1] IS NOT NULL)
```

Query execution can be done directly as well. 

```
select.Query.ExecuteQuery();
/// Optional SQLTransaction param for queries except Select
insert.Query.ExecuteQuery(transaction);
```

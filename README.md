# SqlDapper

This is a simple library to generate SQL based on POCO (or better PORO 🎉).

It maps collections, records and anonymous types to SQL so this can be used with Dapper.

NOTE: Upsert function is really Mssql centric. You have been warned.

# Plain Old Record Object
In order to read/write data, you will need to define a record first:

```C#
[Table("StatusEx")]
public record DbStatusEx([property: Key] string Name,
                         int Status,
                         string? Comment);
```

Available attributes are:
* `Table(name)`: force table name at class level. Default value is class name
* `Key`: specify a property as a key (are part of key composite if multiple declarations)

# Namespace
First you have to use `SqlDapper` namespace before proceeding:
```C#
using SqlDapper;
``` 

# Create connection
Create a connection using `DapperConnection` implementing `IDapperConnection`.

# Usage
```C#
var conn = new DapperConnection("<your connection string>");
var status = conn.Select<DbStatusEx>(new { Name = "toto" });
```

# Raw connection methods
Following operations are available (methods on `IDapperConnection`). Those operations are same as Dapper and basically allow unit testing the library:

Operation | Description
----------|------------
`Execute` | Run provided sql query using the parameter and returns the result from the query (an int)
`QueryScalar<T>` | Run provided sql query using the parameter and returns the single result of `T`
`Query<T>` | Run provided sql query using the parameter and returns a list of result of `T`
`TransactionScope` | Create a transaction scope - transaction must be disposed

# SQL builder methods
Operation | Description
----------|------------
`Select<Table>` | Run select query using the conditions and returns a list of result of `Table`
`SelectAll<Table>` | Select all results from table `Table`
`Insert<Table>` | Insert values into table `Table`
`Update<Table>` | Update table Table` with values
`Delete<Table>` | Delete table `Table` using the conditions
`Upsert<Table>` | Upsert values into table `Table'`. NOTE: as of now, Mssql centric.

`Insert`, `Update`, `Delete` and `Upsert` support either a single value or a list. A value is either a record or an anonymous record.

# Examples

## Upsert
```C#
[Table("Status")]
public record DbStatus([property: Key] string Name, int Status);

var prm = new { Name = "tagada", Status = 42 };
using var tx = conn.TransactionScope();
conn.Upsert<DbStatus>(prm);
tx.Complete();
```

This generates following SQL:
```SQL
MERGE INTO [Status] as TARGET 
USING (VALUES(@Name,@Status)) AS SOURCE ([Name],[Status])
ON SOURCE.[Name]=TARGET.[Name]
WHEN MATCHED THEN UPDATE SET [Status]=SOURCE.[Status] 
WHEN NOT MATCHED THEN INSERT ([Name],[Status]) VALUES (SOURCE.[Name],SOURCE.[Status]);
```

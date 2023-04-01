# SqlDapper

[![Build status](https://github.com/pchalamet/SqlDapper/workflows/build/badge.svg)](https://github.com/pchalamet/SqlDapper/actions?query=workflow%3Abuild) 

This is a simple library to generate SQL based on POCO (or better PORO 🎉). This is the C# version of my F# library [FSharpDapper](https://github.com/pchalamet/FSharpDapper).

It maps collections, records and anonymous types to SQL so this can be used with Dapper.

This library does not cache and is reflection base - hence it's not the most efficient library around. It just works™️.

Implementation is provided for:
* Generic SQL (`Select`, `Insert`, `Update`, `Delete`)
* Specialized dialect MSSQL (`Upsert`)

# 📦 NuGet packages

Package | Status | Description
--------|--------|------------
SqlDapper | [![Nuget](https://img.shields.io/nuget/v/SqlDapper)](https://nuget.org/packages/SqlDapper) | Core package
SqlDapper.SqlServer | [![Nuget](https://img.shields.io/nuget/v/SqlDapper.SqlServer)](https://nuget.org/packages/SqlDapper.SqlServer) | SqlServer provider

# 📚 Api

## Namespace
First you have to use `SqlDapper` namespace before proceeding:
```C#
using SqlDapper;
``` 

## Plain Old Record Object
In order to read/write data, you will need to define a record first (or a class with **properties**):

```C#
[Table("StatusEx")]
public record DbStatusEx([property: Key] string Name,
                         int Status,
                         string? Comment);
```

Available attributes are:
* `Table(name)`: force table name at class level. Default value is record/class name
* `Key`: specify a property as a key (or part of key composite if multiple declarations)

## Create connection
Choose a provider first (see `SqlDapper.SqlServer`) and create a new connection:
```C#
var conn = new SqlServerConnection("<your connection string>");
var status = conn.Select<DbStatusEx>(new { Name = "toto" });
```

## Raw connection methods
Following operations are available (methods on `IDapperConnection`). Those operations are same as Dapper and basically allow unit testing the library:

Operation | Description
----------|------------
`Execute` | Run provided sql query using the parameter and returns the result from the query (an int)
`QueryScalar<T>` | Run provided sql query using the parameter and returns the single result of `T`
`Query<T>` | Run provided sql query using the parameter and returns a list of result of `T`
`TransactionScope` | Create a transaction scope - transaction must be disposed

## Generic SQL builder methods
Following operations are available as extension methods on `IDapperConnection`:

Operation | Description
----------|------------
`Select<Table>` | Run select query using the conditions and returns a list of result of `Table`
`SelectAll<Table>` | Select all results from table `Table`
`Insert<Table>` | Insert values into table `Table`
`Update<Table>` | Update table Table` with values
`Delete<Table>` | Delete table `Table` using the conditions

`Insert`, `Update`, `Delete` support either a single value or a list. A value is either a record or an anonymous record.

## SqlServer builder methods
Following operations are available as extension methods on `IDapperConnection`:

Operation | Description
----------|------------
`Upsert<Table>` | Upsert values into table `Table`. NOTE: as of now, Mssql centric.

`Upsert` support either a single value or a list. A value is either a record or an anonymous record.

# Examples

## Upsert
```C#
using SqlDapper;

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
USING (VALUES(@Name,@Status)) AS SOURCE ([Name],[Status]) ON SOURCE.[Name]=TARGET.[Name]
WHEN MATCHED THEN UPDATE SET [Status]=SOURCE.[Status] 
WHEN NOT MATCHED THEN INSERT ([Name],[Status]) VALUES (SOURCE.[Name],SOURCE.[Status]);
```

using System.Reflection;

namespace SqlDapper;

public static class DapperConnectionExtensions {
    private static Type GetUnderlyingType(this Type t) {
        var itf = t.GetInterfaces()
                   .FirstOrDefault(it => it.IsGenericType
                                   && it.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return itf is not null
               ? itf.GenericTypeArguments[0]
               : t;
    }

    private static int GetCount(this object o) {
        return o switch {
            null => 0,
            IEnumerable<object> e => e.Count(),
            _ => 1
        };
    }

    private static IEnumerable<string> GetTableColumns(this Type t) {
        return t.GetUnderlyingType()
            .GetProperties()
            .Select(x => x.Name);
    }

    private static IEnumerable<string> GetTableKeys(this Type t) {
        return t.GetProperties()
                .Where(isKey)
                .Select(p => p.Name);

        bool isKey(PropertyInfo prop) {
            return prop.Name == "Id" || prop.GetCustomAttribute(typeof(KeyAttribute), false) is not null;
        }
    }

    private static string GetTable(this Type t) {
        return t.GetCustomAttribute(typeof(TableAttribute), false) switch {
            TableAttribute attr => attr.Name,
            _ => t.Name
        };
    }

    private static string EscapeName(this string s) {
        return $"[{s}]";
    }

    private static string ParameterName(string s) {
        return $"@{s}";
    }

    private static string Join(this IEnumerable<string> ss, string sep) {
        return String.Join(sep, ss);
    }

    public static IEnumerable<Table> Select<Table>(this IDapperConnection connection, object conditions) {
        // SELECT Id, Toto FROM Table [WHERE Tutu=@Tutu]
        var projection = typeof(Table).GetTableColumns().Select(EscapeName).Join(",");
        var from = typeof(Table).GetTable().EscapeName();
        var columns = conditions.GetType().GetTableColumns();
        var where = columns.Select(c => $"{c.EscapeName()}=@{c}") switch {
            IEnumerable<string> predicates when predicates.Any() => predicates.Join(" AND "),
            _ => "1=1",
        };

        var sql = $"SELECT {projection} FROM {from} WHERE {where}";
        return connection.Query<Table>(sql, conditions);
    }

    public static IEnumerable<Table> SelectAll<Table>(this IDapperConnection connection) {
        return connection.Select<Table>(new { });
    }

    public static void Insert<Table>(this IDapperConnection connection, object entity) {
        // INSERT INTO Table (Id, Toto) VALUES (@Id, @Toto)
        var into = typeof(Table).GetTable().EscapeName();
        var columns = entity.GetType().GetTableColumns();
        var projection = columns.Select(EscapeName).Join(",");
        var values = columns.Select(ParameterName).Join(",");

        var sql = $"INSERT INTO {into} ({projection}) VALUES ({values})";
        var count = entity.GetCount();
        if (count != connection.Execute(sql, entity)) {
            throw new Exception("Failed to Insert");
        }
    }

    public static void Update<Table>(this IDapperConnection connection, object entity) {
        // UPDATE Table set @Toto=@Toto, @Tutu=@Tutu
        var from = typeof(Table).GetTable().EscapeName();
        var ids = typeof(Table).GetTableKeys();
        var columns = entity.GetType().GetTableColumns();
        var assign = columns.Where(x => !ids.Contains(x)).Select(c => $"{c.EscapeName()}=@{c}").Join(",");
        var where = ids.Select(c => $"{c.EscapeName()}=@{c}").Join(" AND ");

        var sql = $"UPDATE {from} SET {assign} WHERE {where}";
        var count = entity.GetCount();
        if (count != connection.Execute(sql, entity)) {
            throw new Exception("Failed to Update");
        }
    }

    public static int Delete<Table>(this IDapperConnection connection, object conditions) {
        // DELETE Table WHERE @Tutu=@Tutu
        var from = typeof(Table).GetTable().EscapeName();
        var columns = conditions.GetType().GetTableColumns();
        var where = columns.Select(c => $"{c.EscapeName()}=@{c}") switch {
            IEnumerable<string> predicates when predicates.Any() => predicates.Join(" AND "),
            _ => "1=1",
        };

        var sql = $"DELETE {from} WHERE {where}";
        return connection.Execute(sql, conditions);
    }

    public static void Upsert<Table>(this IDapperConnection connection, object entity) {
        // MERGE INTO <Table> as TARGET
        // USING ( VALUES(@Name, @Status) ) AS SOURCE ([Name], [Status]) ON SOURCE.[Name] = TARGET.[Name]
        // WHEN MATCHED THEN UPDATE 
        //     SET [Status]=SOURCE.[Status]
        // WHEN NOT MATCHED THEN 
        //     INSERT ([Name],[Status]) VALUES (SOURCE.[Name], SOURCE.[Status]);

        var entityType = entity.GetType().GetUnderlyingType();
        var table = typeof(Table).GetTable().EscapeName();
        var columns = entityType.GetTableColumns();
        var projection = columns.Select(EscapeName).Join(",");
        var values = columns.Select(ParameterName).Join(",");

        var ids = typeof(Table).GetTableKeys();
        var assignMatched = columns.Where(c => !ids.Contains(c)).Select(c => $"{c.EscapeName()}=SOURCE.{c.EscapeName()}").Join(",");
        var assignUnmatched = columns.Select(c => $"SOURCE.{c.EscapeName()}").Join(",");
        var where = ids.Select(c => $"SOURCE.{c.EscapeName()}=TARGET.{c.EscapeName()}").Join(" AND ");
        var count = entity.GetCount();

        var sql = $"MERGE INTO {table} as TARGET USING (VALUES({values})) AS SOURCE ({projection}) ON {where} WHEN MATCHED THEN UPDATE SET {assignMatched} WHEN NOT MATCHED THEN INSERT ({projection}) VALUES ({assignUnmatched});";
        if (count != connection.Execute(sql, entity)) {
            throw new Exception("Failed to Upsert");
        }
    }
}

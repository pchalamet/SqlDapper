namespace SqlDapper;

public static class DapperConnectionExtensions {
    private const string insertKey = "insert";
    private const string selectKey = "select";
    private const string updateKey = "update";
    private const string deleteKey = "delete";

    public static IEnumerable<Table> Select<Table>(this IDapperConnection connection, object conditions) {
        // SELECT Id, Toto FROM Table [WHERE Tutu=@Tutu]
        if (!SqlCache.GetQuery<Table>(selectKey, conditions, out var sql)) {
            var projection = typeof(Table).GetTableColumns().Select(TypeExtensions.EscapeName).Join(",");
            var from = typeof(Table).GetTable().EscapeName();
            var columns = conditions.GetType().GetTableColumns();
            var where = columns.Select(c => $"{c.EscapeName()}=@{c}") switch {
                IEnumerable<string> predicates when predicates.Any() => predicates.Join(" AND "),
                _ => "1=1",
            };

            sql = $"SELECT {projection} FROM {from} WHERE {where}";
            SqlCache.CacheQuery<Table>(selectKey, conditions, sql);
        }
        return connection.Query<Table>(sql, conditions);
    }

    public static IEnumerable<Table> SelectAll<Table>(this IDapperConnection connection) {
        return connection.Select<Table>(new { });
    }

    public static void Insert<Table>(this IDapperConnection connection, object entity) {
        // INSERT INTO Table (Id, Toto) VALUES (@Id, @Toto)
        if (!SqlCache.GetQuery<Table>(insertKey, entity, out var sql)) {
            var into = typeof(Table).GetTable().EscapeName();
            var columns = entity.GetType().GetTableColumns();
            var projection = columns.Select(TypeExtensions.EscapeName).Join(",");
            var values = columns.Select(TypeExtensions.ParameterName).Join(",");

            sql = $"INSERT INTO {into} ({projection}) VALUES ({values})";
            SqlCache.CacheQuery<Table>(insertKey, entity, sql);
        }

        var count = entity.GetCount();
        if (count != connection.Execute(sql, entity)) {
            throw new Exception("Failed to Insert");
        }
    }

    public static void Update<Table>(this IDapperConnection connection, object entity) {
        // UPDATE Table set @Toto=@Toto, @Tutu=@Tutu
        if (!SqlCache.GetQuery<Table>(updateKey, entity, out var sql)) {
            var from = typeof(Table).GetTable().EscapeName();
            var ids = typeof(Table).GetTableKeys();
            var columns = entity.GetType().GetTableColumns();
            var assign = columns.Where(x => !ids.Contains(x)).Select(c => $"{c.EscapeName()}=@{c}").Join(",");
            var where = ids.Select(c => $"{c.EscapeName()}=@{c}").Join(" AND ");

            sql = $"UPDATE {from} SET {assign} WHERE {where}";
            SqlCache.CacheQuery<Table>(updateKey, entity, sql);
        }

        var count = entity.GetCount();
        if (count != connection.Execute(sql, entity)) {
            throw new Exception("Failed to Update");
        }
    }

    public static int Delete<Table>(this IDapperConnection connection, object conditions) {
        // DELETE FROM Table WHERE @Tutu=@Tutu
        if (!SqlCache.GetQuery<Table>(deleteKey, conditions, out var sql)) {
            var from = typeof(Table).GetTable().EscapeName();
            var columns = conditions.GetType().GetTableColumns();
            var where = columns.Select(c => $"{c.EscapeName()}=@{c}") switch {
                IEnumerable<string> predicates when predicates.Any() => predicates.Join(" AND "),
                _ => "1=1",
            };

            sql = $"DELETE FROM {from} WHERE {where}";
            SqlCache.CacheQuery<Table>(deleteKey, conditions, sql);
        }

        return connection.Execute(sql, conditions);
    }
}

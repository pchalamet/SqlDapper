namespace SqlDapper;

public static class MssqlConnectionExtensions {
    private const string upsertKey = "upsert";

    public static void Upsert<Table>(this IDapperConnection connection, object entity) {
        // MERGE INTO <Table> as TARGET
        // USING ( VALUES(@Name, @Status) ) AS SOURCE ([Name], [Status]) ON SOURCE.[Name] = TARGET.[Name]
        // WHEN MATCHED THEN UPDATE 
        //     SET [Status]=SOURCE.[Status]
        // WHEN NOT MATCHED THEN 
        //     INSERT ([Name],[Status]) VALUES (SOURCE.[Name], SOURCE.[Status]);
        if (!SqlCache.GetQuery<Table>(upsertKey, entity, out var sql)) {
            var entityType = entity.GetType().GetUnderlyingType();
            var table = typeof(Table).GetTable().EscapeName();
            var columns = entityType.GetTableColumns();
            var projection = columns.Select(TypeExtensions.EscapeName).Join(",");
            var values = columns.Select(TypeExtensions.ParameterName).Join(",");

            var ids = typeof(Table).GetTableKeys();
            var assignMatched = columns.Where(c => !ids.Contains(c)).Select(c => $"{c.EscapeName()}=SOURCE.{c.EscapeName()}").Join(",");
            var assignUnmatched = columns.Select(c => $"SOURCE.{c.EscapeName()}").Join(",");
            var where = ids.Select(c => $"SOURCE.{c.EscapeName()}=TARGET.{c.EscapeName()}").Join(" AND ");

            sql = $"MERGE INTO {table} as TARGET USING (VALUES({values})) AS SOURCE ({projection}) ON {where} WHEN MATCHED THEN UPDATE SET {assignMatched} WHEN NOT MATCHED THEN INSERT ({projection}) VALUES ({assignUnmatched});";
            SqlCache.CacheQuery<Table>(upsertKey, entity, sql);
        }

        var count = entity.GetCount();
        if (count != connection.Execute(sql, entity)) {
            throw new Exception("Failed to Upsert");
        }
    }
}

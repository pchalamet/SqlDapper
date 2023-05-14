using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace SqlDapper;

public static class SqlCache {
    private static readonly ConcurrentDictionary<(string, Type, Type), string> _sqlCache = new();

    public static void Clear() {
        _sqlCache.Clear();
    }

    public static void CacheQuery<Table>(string context, object entity, string sql) {
        _sqlCache.TryAdd((context, typeof(Table), entity.GetType()), sql);
    }

    public static bool GetQuery<Table>(string context, object entity, [NotNullWhen(true)] out string? sql) {
        return _sqlCache.TryGetValue((context, typeof(Table), entity.GetType()), out sql);
    }
}

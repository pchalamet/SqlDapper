using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace SqlDapper;

public static class SqlCache {
    private static readonly ConcurrentDictionary<(Type, Type), string> _sqlCache = new();

    public static void Clear() {
        _sqlCache.Clear();
    }

    public static void CacheQuery<Table>(object entity, string sql) {
        _sqlCache.TryAdd((typeof(Table), entity.GetType()), sql);
    }

    public static bool GetQuery<Table>(object entity, [NotNullWhen(true)] out string? sql) {
        return _sqlCache.TryGetValue((typeof(Table), entity.GetType()), out sql);
    }
}

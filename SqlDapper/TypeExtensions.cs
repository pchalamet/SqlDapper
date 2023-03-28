using System.Reflection;

namespace SqlDapper;

internal static class TypeExtensions {
    public static Type GetUnderlyingType(this Type t) {
        var itf = Array.Find(t.GetInterfaces(),
                             it => it.IsGenericType
                                   && it.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return itf is not null
               ? itf.GenericTypeArguments[0]
               : t;
    }

    public static int GetCount(this object o) {
        return o switch {
            null => 0,
            IEnumerable<object> e => e.Count(),
            _ => 1
        };
    }

    public static IEnumerable<string> GetTableColumns(this Type t) {
        return t.GetUnderlyingType()
            .GetProperties()
            .Select(x => x.Name);
    }

    public static IEnumerable<string> GetTableKeys(this Type t) {
        return t.GetProperties()
                .Where(isKey)
                .Select(p => p.Name);

        static bool isKey(PropertyInfo prop) {
            return prop.Name == "Id" || prop.GetCustomAttribute(typeof(KeyAttribute), false) is not null;
        }
    }

    public static string GetTable(this Type t) {
        return t.GetCustomAttribute(typeof(TableAttribute), false) switch {
            TableAttribute attr => attr.Name,
            _ => t.Name
        };
    }

    public static string EscapeName(this string s) {
        return $"[{s}]";
    }

    public static string ParameterName(string s) {
        return $"@{s}";
    }

    public static string Join(this IEnumerable<string> ss, string sep) {
        return string.Join(sep, ss);
    }
}

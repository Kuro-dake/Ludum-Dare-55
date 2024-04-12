using System.Collections;
using System.Collections.Generic;
using System.Linq;
public static class CollectionsOutputExtension
{
    public static string GetKeyValuesString<T1, T2>(this Dictionary<T1, T2> d)
        => string.Join("\n", d.ToList().ConvertAll(kv => " - " + kv.Key.ToString() + ": " + kv.Value.ToString()));
    public static string GetValuesString<T>(this List<T> l)
        => string.Join("\n", l.ConvertAll(value => " - " + (value == null ? "NULL" : value.ToString())));
}
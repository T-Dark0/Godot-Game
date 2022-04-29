using System;
using System.Collections.Generic;
using System.Linq;

// NOTE: some methods in this class are restricted to T: struct because C# behaves really awkwardly otherwise
// Specifically, it will make Random return a defaulted T instead of a null one
// and it will make SelectWhere return U? even though the declared return type is U
public static class LinqExtensions
{
    // Reservoir sampling, specialized for the case in which we only want to extract a single element.
    public static T? Random<T>(this IEnumerable<T> iter, Random rng) where T : struct
    {
        var consumed = 0;
        T? result = null;
        foreach (var elem in iter)
        {
            consumed++;
            if (rng.Next(consumed) == 0)
            {
                result = elem;
            }
        }
        return result;
    }

    // Combined Select and Where: if the function returns an element, the resulting enumerable
    // will contain that element, and if the function returns null, the element is dropped instead
    public static IEnumerable<U> SelectWhere<T, U>(this IEnumerable<T> iter, Func<T, U?> f) where U : struct
    {
        foreach (var elem in iter)
        {
            var mapped = f(elem);
            if (mapped is U u) yield return u;
        }
    }

    public static IEnumerable<(T, T)> Windows2<T>(this IEnumerable<T> source)
    {
        var iter = source.GetEnumerator();
        T left;
        if (iter.MoveNext()) left = iter.Current;
        else yield break;
        while (iter.MoveNext())
        {
            var right = iter.Current;
            yield return (left, right);
            left = right;
        }
    }

    public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }

    public static IEnumerable<T> TakeWhileInclusive<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var stop = false;
        foreach (var elem in source)
        {
            if (stop) yield break;
            stop = !predicate(elem);
            yield return elem;
        }
    }
}
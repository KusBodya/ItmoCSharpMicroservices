namespace Zip;

public static class EnumerableExtensions
{
    public static IEnumerable<T[]> ZipMultiple<T>(
        this IEnumerable<T> needsToZip,
        params IEnumerable<T>[] whatNeedsToZip)
    {
        var enumerators = new List<IEnumerator<T>>();

        try
        {
            enumerators.Add(needsToZip.GetEnumerator());

            foreach (IEnumerable<T> collection in whatNeedsToZip)
            {
                enumerators.Add(collection.GetEnumerator());
            }

            while (enumerators.All(enumerator => enumerator.MoveNext()))
            {
                yield return enumerators.Select(enumerator => enumerator.Current).ToArray();
            }
        }
        finally
        {
            foreach (IEnumerator<T> enumerator in enumerators)
            {
                enumerator.Dispose();
            }
        }
    }
}
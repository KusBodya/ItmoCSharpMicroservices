using System.Runtime.CompilerServices;

namespace Zip;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T[]> ZipMultipleAsync<T>(
        this IAsyncEnumerable<T> needsToZip,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        params IAsyncEnumerable<T>[] whatNeedsToZip)
    {
        var enumerators = new List<IAsyncEnumerator<T>>(1 + whatNeedsToZip.Length);

        try
        {
            enumerators.Add(needsToZip.GetAsyncEnumerator(cancellationToken));
            foreach (IAsyncEnumerable<T> collection in whatNeedsToZip)
            {
                enumerators.Add(collection.GetAsyncEnumerator(cancellationToken));
            }

            Task<bool>[] moveTasks = enumerators
                .Select(e => e.MoveNextAsync().AsTask())
                .ToArray();

            bool[] results = await Task.WhenAll(moveTasks);

            while (results.All(r => r))
            {
                var row = new T[enumerators.Count];
                for (int i = 0; i < enumerators.Count; i++)
                    row[i] = enumerators[i].Current;

                yield return row;

                moveTasks = enumerators
                    .Select(e => e.MoveNextAsync().AsTask())
                    .ToArray();

                results = await Task.WhenAll(moveTasks);
            }
        }
        finally
        {
            foreach (IAsyncEnumerator<T>? e in enumerators)
            {
                await e.DisposeAsync();
            }
        }
    }
}
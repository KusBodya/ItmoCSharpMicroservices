using FluentAssertions;
using Xunit;
using Zip;

namespace Lab1.Tests;

public class EnumerableExtensionsTests
{
    public static IEnumerable<object[]> SameLengthCases()
    {
        yield return Case([1, 2, 3]);
        yield return Case([1, 2, 3], [10, 20, 30]);
        yield return Case([1, 2, 3], [10, 20, 30], [100, 200, 300]);
        yield return Case(
            [1, 2, 3],
            [10, 20, 30],
            [100, 200, 300],
            [1000, 2000, 3000],
            [10000, 20000, 30000]);
    }

    public static IEnumerable<object[]> UnequalLengthCases()
    {
        yield return Case3([1, 2, 3], [10, 20]);
        yield return Case3([1, 2, 3], [10, 20, 30, 40], [100, 200, 300, 400, 500]);
        yield return Case3([1, 2, 3, 4], [10], [100, 200, 300]);
        yield return Case3([1, 2, 3], []);

        yield return Case3([1, 2, 3, 4], [10, 20, 30, 40], [], [100, 200]);
    }

    private static object[] Case3(int[] source, params int[][] others)
    {
        int minLen = new[] { source.Length }.Concat(others.Select(o => o.Length)).Min();
        var expected = new List<int[]>(minLen);
        for (int i = 0; i < minLen; i++)
        {
            int[] row = new int[1 + others.Length];
            row[0] = source[i];
            for (int j = 0; j < others.Length; j++)
                row[j + 1] = others[j][i];
            expected.Add(row);
        }

        return [source, others, expected];
    }

    private static object[] Case(int[] source, params int[][] others)
    {
        var expected = new List<int[]>();
        for (int i = 0; i < source.Length; i++)
        {
            int[] row = new int[1 + others.Length];
            row[0] = source[i];
            for (int j = 0; j < others.Length; j++)
                row[j + 1] = others[j][i];
            expected.Add(row);
        }

        return [source, others, expected];
    }

    private static readonly int[] SourceArray = [1, 2, 3];

    [Fact]
    public async Task ZipMultipleAsyncNoOthersProducesSingletonRows()
    {
        IAsyncEnumerable<int> stream = SourceArray.ToAsyncEnumerable();
        var rows = new List<int[]>();

        await foreach (int[] row in stream.ZipMultipleAsync(default))
        {
            rows.Add(row);
        }

        rows.Should().HaveCount(3);
        rows.Should().AllSatisfy(r => r.Should().HaveCount(1));
        rows.Select(r => r[0]).Should().Equal(1, 2, 3);
    }

    [Fact]
    public void ZipMultipleNoOthersProducesSingletonRows()
    {
        int[] source = [1, 2, 3];

        var result = source.ZipMultiple().ToList();

        result.Should().HaveCount(source.Length);
        result.Should().AllSatisfy(row => row.Should().HaveCount(1));
        result.Select(r => r[0]).Should().Equal(source);
    }

    [Theory]
    [MemberData(nameof(SameLengthCases), MemberType = typeof(EnumerableExtensionsTests))]
    public async Task ZipMultipleAsyncSameLengthProducesExpectedRows(
        int[] source,
        int[][] others,
        IList<int[]> expected)
    {
        IAsyncEnumerable<int> stream = source.ToAsyncEnumerable();
        IAsyncEnumerable<int>[] otherStreams = others.Select(AsyncEnumerable.ToAsyncEnumerable).ToArray();

        var rows = new List<int[]>();
        await foreach (int[] row in stream.ZipMultipleAsync(default, otherStreams))
            rows.Add(row);

        rows.Should().HaveCount(expected.Count);
        for (int i = 0; i < expected.Count; i++)
            rows[i].Should().Equal(expected[i]);
    }

    [Theory]
    [MemberData(nameof(UnequalLengthCases), MemberType = typeof(EnumerableExtensionsTests))]
    public async Task ZipMultipleAsyncUnequalLengthsMatchesShortest(
        int[] source,
        int[][] others,
        IList<int[]> expected)
    {
        IAsyncEnumerable<int> first = source.ToAsyncEnumerable();
        IAsyncEnumerable<int>[] rest = others.Select(AsyncEnumerable.ToAsyncEnumerable).ToArray();

        var rows = new List<int[]>();
        await foreach (int[] row in first.ZipMultipleAsync(default, rest))
            rows.Add(row);

        rows.Should().HaveCount(expected.Count);
        for (int i = 0; i < expected.Count; i++)
            rows[i].Should().Equal(expected[i]);
    }
}
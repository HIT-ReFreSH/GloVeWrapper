using System.Collections.Generic;
using System.IO;
using System.Linq;
using HitRefresh.GloVeWrapper.Models;

namespace HitRefresh.GloVeWrapper.IO;

/// <summary>
/// </summary>
public class GloveTextReader : IGloveReader
{
    /// <summary>
    /// </summary>
    public GloveTextReader(bool skipFirstLine = false)
    {
        SkipFirstLine = skipFirstLine;
    }

    private bool SkipFirstLine { get; }

    /// <inheritdoc />
    public async IAsyncEnumerable<(string, IDoubleVector)> GetAllAsync(string path)
    {
        using var f = new StreamReader(File.OpenRead(path));
        if (SkipFirstLine)
            await f.ReadLineAsync();
        for (;;)
        {
            var line = await f.ReadLineAsync();
            if (line == null) break;
            var spl = line.Split(' ');
            yield return (spl[0], new DenseDoubleVector(spl[1..].Select(double.Parse).ToArray()));
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HitRefresh.GloVeWrapper.Models;

namespace HitRefresh.GloVeWrapper.IO;

/// <summary>
/// </summary>
public class GloveBinaryWriter : IGloveWriter
{
    /// <inheritdoc />
    public void WriteStream(IEnumerable<(string, IDoubleVector)> stream, string dictFile, string vecFile)
    {
        using var dict = new BinaryWriter(new BufferedStream(
            File.OpenWrite(dictFile)));
        using var vec = new BinaryWriter(new BufferedStream(
            File.OpenWrite(vecFile)));

        var lineNum = 0L;

        var blockSize = -1L;
        var offset = 0L;
        var byteBuffer = new MemoryStream();
        foreach (var (key, val) in stream)
        {
            lineNum++;
            dict.Write(key);
            byteBuffer.Position = 0;
            dict.Write7BitEncodedInt64(offset);
            WriteVectorData(val, new BinaryWriter(byteBuffer));


            var buf = byteBuffer.ToArray();
            if (blockSize == -1) blockSize = buf.LongLength;

            if (blockSize != buf.Length) throw new Exception($"Invalid Vector Size at Line {lineNum}.");

            vec.Write(buf);

            offset += buf.LongLength;
        }
    }

    /// <inheritdoc />
    public async Task WriteStreamAsync(IAsyncEnumerable<(string, IDoubleVector)> stream, string dictFile,
        string vecFile)
    {
        await using var dict = new BinaryWriter(new BufferedStream(
            File.OpenWrite(dictFile)));
        await using var vec = new BinaryWriter(new BufferedStream(
            File.OpenWrite(vecFile)));

        var lineNum = 0L;

        var blockSize = -1L;
        var offset = 0L;
        var byteBuffer = new MemoryStream();
        await foreach (var (key, val) in stream)
        {
            lineNum++;
            dict.Write(key);
            byteBuffer.Position = 0;
            dict.Write7BitEncodedInt64(offset);
            WriteVectorData(val, new BinaryWriter(byteBuffer));


            var buf = byteBuffer.ToArray();
            if (blockSize == -1) blockSize = buf.LongLength;

            if (blockSize != buf.Length) throw new Exception($"Invalid Vector Size at Line {lineNum}.");

            vec.Write(buf);

            offset += buf.LongLength;
        }
    }


    private static void WriteVectorData(IDoubleVector v, BinaryWriter output)
    {
        foreach (var t in v) output.Write(t);
    }
}
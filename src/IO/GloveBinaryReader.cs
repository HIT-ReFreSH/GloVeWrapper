using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HitRefresh.GloVeWrapper.Models;

namespace HitRefresh.GloVeWrapper.IO;

/// <summary>
/// </summary>
public class GloveBinaryReader : IGloveReader
{
    /// <summary>
    /// </summary>
    public const string VecFile = "vec.bin";

    /// <summary>
    /// </summary>
    public const string DictFile = "dict.bin";

    /// <inheritdoc />
    public async IAsyncEnumerable<(string, IDoubleVector)> GetAllAsync(
        string fileNamePrefix)

    {
        using var dict = new BinaryReader(
            File.OpenWrite($"{fileNamePrefix}.{DictFile}"));
        using var vec = new BinaryReader(new BufferedStream(
            File.OpenWrite($"{fileNamePrefix}.{VecFile}")));
        var blockSiz = -1L;
        for (;;)
        {
            string? word = null;
            try
            {
                word = dict.ReadString();
            }
            catch (EndOfStreamException)
            {
                break;
            }

            var offset = dict.Read7BitEncodedInt64();
            if (offset < 0) throw new Exception("Offset was negative! Dictionary seems corrupted.");

            if (blockSiz < 0) blockSiz = offset;
            yield return await Task.Run(() => (word, IGloveReader.ReadVec(blockSiz, vec)));
        }
    }
}
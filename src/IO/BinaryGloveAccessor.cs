using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading.Tasks;
using HitRefresh.GloVeWrapper.Models;

namespace HitRefresh.GloVeWrapper.IO;

/// <summary>
///     input.PeekChar() != -1
/// </summary>
public class BinaryGloveAccessor : IGloveAccessor, IDisposable
{
    /// <summary>
    /// </summary>
    /// <param name="dictFile"></param>
    /// <param name="vecFile"></param>
    public BinaryGloveAccessor(string dictFile, string vecFile)
    {
        using var input = new BinaryReader(File.OpenRead(dictFile), Encoding.UTF8);

        var size = -1L;
        for (;;)
            try
            {
                var s = input.ReadString();
                var offset = input.Read7BitEncodedInt64();
                if (size <= 0) size = offset;
                Dictionary.TryAdd(s, offset);
            }
            catch (EndOfStreamException)
            {
                break;
            }

        BlockSize = size;
        Vectors = MemoryMappedFile.CreateFromFile(vecFile, FileMode.Open);
    }

    private Dictionary<string, long> Dictionary { get; } = new();
    private MemoryMappedFile Vectors { get; }
    private long BlockSize { get; }

    /// <inheritdoc />
    public bool Contains(string word)
    {
        return Dictionary.ContainsKey(word);
    }

    /// <inheritdoc />
    public IDoubleVector? this[string word]
    {
        get
        {
            if (!Contains(word)) return null;

            var offset = Dictionary[word];
            using var br = new BinaryReader(
                Vectors.CreateViewStream(offset, BlockSize, MemoryMappedFileAccess.Read)
            );
            return IGloveReader.ReadVec(BlockSize, br);
        }
    }

    /// <inheritdoc />
    public async Task<IDoubleVector?> GetAsync(string word)
    {
        if (!Contains(word)) return null;

        return await Task.Run(() =>
        {
            var offset = Dictionary[word];
            using var br = new BinaryReader(
                Vectors.CreateViewStream(offset, BlockSize, MemoryMappedFileAccess.Read)
            );
            return IGloveReader.ReadVec(BlockSize, br);
        });
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Vectors.Dispose();
        GC.SuppressFinalize(this);
    }
}
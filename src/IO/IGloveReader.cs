using System;
using System.Collections.Generic;
using System.IO;
using HitRefresh.GloVeWrapper.Models;

namespace HitRefresh.GloVeWrapper.IO;

/// <summary>
/// </summary>
public interface IGloveReader
{
    /**
     * Streams over the glove file/directory in the given path.
     * 
     * @param input the path to the glove files or directory (defined by the
     * implementation).
     * @return a lazy evaluated stream of the glove file.
     * @throws IOException file not found, or io errors.
     */
    public IAsyncEnumerable<(string, IDoubleVector)> GetAllAsync(string path);

    /// <summary>
    ///     Read a vector from binary stream.
    /// </summary>
    /// <param name="blockSize"></param>
    /// <param name="br"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IDoubleVector ReadVec(long blockSize, BinaryReader br)
    {
        var dim = (int) (blockSize >> 3);
        Span<double> vec = stackalloc double[dim];

        for (var i = 0; i < dim; i++) vec[i] = br.ReadDouble();

        return new DenseDoubleVector(vec);
    }

    /// <summary>
    ///     Read a vector from binary stream.
    /// </summary>
    /// <param name="blockSize"></param>
    /// <param name="br"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static DenseDoubleVector ReadDenseVec(long blockSize, BinaryReader br)
    {
        var dim = (int) (blockSize >> 3);
        Span<double> vec = stackalloc double[dim];

        for (var i = 0; i < dim; i++) vec[i] = br.ReadDouble();

        return new DenseDoubleVector(vec);
    }
}
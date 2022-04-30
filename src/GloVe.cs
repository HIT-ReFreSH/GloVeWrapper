using System.IO;
using System.Threading.Tasks;
using HitRefresh.GloVeWrapper.IO;
using HitRefresh.GloVeWrapper.Models;

namespace HitRefresh.GloVeWrapper;

/// <summary>
///     Useful tools for using this wrapper
/// </summary>
public static class GloVe
{
    /// <summary>
    /// </summary>
    /// <param name="dictFile"></param>
    /// <param name="vecFile"></param>
    /// <returns></returns>
    public static IGloveAccessor AccessBinary(string dictFile, string vecFile)
    {
        return new BinaryGloveAccessor(dictFile, vecFile);
    }

    /// <summary>
    /// </summary>
    /// <param name="dictFile"></param>
    /// <param name="vecFile"></param>
    /// <returns></returns>
    public static IO.HighPerformance.IGloveAccessor AccessBinaryHighPerformance(string dictFile, string vecFile)
    {
        return new IO.HighPerformance.BinaryGloveAccessor(dictFile, vecFile);
    }

    /// <summary>
    ///     Access binary glove files In default filename format(xxx.dict.bin and xxx.vec.bin)
    /// </summary>
    /// <param name="fileNamePrefix">
    ///     Prefix of default filename format,
    ///     e.g.
    ///     '/home/foo/bar' for '/home/foo/bar.dict.bin' and '/home/foo/bar.vec.bin'
    /// </param>
    /// <returns></returns>
    public static IGloveAccessor AccessBinary(string fileNamePrefix)
    {
        return AccessBinary($"{fileNamePrefix}.dict.bin",
            $"{fileNamePrefix}.vec.bin");
    }

    /// <summary>
    ///     Access binary glove files In default filename format(xxx.dict.bin and xxx.vec.bin)
    /// </summary>
    /// <param name="fileNamePrefix">
    ///     Prefix of default filename format,
    ///     e.g.
    ///     '/home/foo/bar' for '/home/foo/bar.dict.bin' and '/home/foo/bar.vec.bin'
    /// </param>
    /// <returns></returns>
    public static IO.HighPerformance.IGloveAccessor AccessBinaryHighPerformance(string fileNamePrefix)
    {
        return AccessBinaryHighPerformance($"{fileNamePrefix}.dict.bin",
            $"{fileNamePrefix}.vec.bin");
    }

    /// <summary>
    /// </summary>
    /// <param name="baseAccessor"></param>
    /// <param name="maxCacheSize"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IGloveAccessor AsCached(this IGloveAccessor baseAccessor, int maxCacheSize = 0, string name = "glove")
    {
        return new CachedGloveAccessor(baseAccessor, maxCacheSize, name);
    }

    /// <summary>
    /// </summary>
    /// <param name="baseAccessor"></param>
    /// <param name="maxCacheSize"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IO.HighPerformance.IGloveAccessor AsCached(this IO.HighPerformance.IGloveAccessor baseAccessor,
        int maxCacheSize = 0, string name = "glove")
    {
        return new IO.HighPerformance.CachedGloveAccessor(baseAccessor, maxCacheSize, name);
    }

    /// <summary>
    /// </summary>
    /// <param name="textDictFile"></param>
    /// <param name="skipFirstLine"></param>
    /// <returns></returns>
    public static async Task CreateBinaryAsync(string textDictFile, bool skipFirstLine = false)
    {
        var input = new GloveTextReader(skipFirstLine);
        var output = new GloveBinaryWriter();
        await output.WriteStreamAsync(input.GetAllAsync(textDictFile),
            Path.ChangeExtension(textDictFile, ".dict.bin"),
            Path.ChangeExtension(textDictFile, ".vec.bin"));
    }

    /// <summary>
    /// </summary>
    /// <param name="vec1"></param>
    /// <param name="vec2"></param>
    /// <returns></returns>
    public static double CosDistanceBetween(IDoubleVector? vec1, IDoubleVector? vec2)
    {
        if (vec1 is null || vec2 is null) return 1.0;
        return 1 - vec1.Dot(vec2) / (vec1.Modulus * vec2.Modulus);
    }

    /// <summary>
    /// </summary>
    /// <param name="vec1"></param>
    /// <param name="vec2"></param>
    /// <returns></returns>
    public static double CosDistanceBetween(DenseDoubleVector vec1, DenseDoubleVector vec2)
    {
        if (vec1.IsEmpty || vec2.IsEmpty) return 1.0;
        return 1 - vec1.Dot(vec2) / (vec1.Modulus * vec2.Modulus);
    }

    /// <summary>
    /// </summary>
    /// <param name="vec1"></param>
    /// <param name="vec2"></param>
    /// <returns></returns>
    public static double CosDistanceBetween(RefDenseDoubleVector vec1, RefDenseDoubleVector vec2)
    {
        if (vec1.IsEmpty || vec2.IsEmpty) return 1.0;
        return 1 - vec1.Dot(vec2) / (vec1.Modulus * vec2.Modulus);
    }
}
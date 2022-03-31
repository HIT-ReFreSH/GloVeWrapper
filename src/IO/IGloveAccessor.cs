using System;
using System.Threading.Tasks;
using HitRefresh.GloVeWrapper.Models;

namespace HitRefresh.GloVeWrapper.IO;

/// <summary>
///     Provides a Accessor to GloVe Word Embedding vectors.
/// </summary>
public interface IGloveAccessor : IDisposable
{
    /**
         * @return the word or null if it doesn't exists.
         */
    public IDoubleVector? this[string word] { get; }

    /**
 * @return true if the glove reader contains this word.
 */
    public bool Contains(string word);

    /// <summary>
    ///     return the word or null if it doesn't exists.
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    public Task<IDoubleVector?> GetAsync(string word);
}
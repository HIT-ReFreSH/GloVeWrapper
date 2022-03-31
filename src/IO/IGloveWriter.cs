using System.Collections.Generic;
using System.Threading.Tasks;
using HitRefresh.GloVeWrapper.Models;

namespace HitRefresh.GloVeWrapper.IO;

/// <summary>
/// </summary>
public interface IGloveWriter
{
    /**
     * A writer for a stream of StringVectorPairs. This is mainly used to rewrite
     * text to binary files or vice versa.
     * 
     * @param stream the stream of elements to write.
     * @param output depending on the implementation, either a file or a folder.
     * @throws IOException file/directory doesn't exist, isn't writable, or other
     * io errors.
     */
    public void WriteStream(IEnumerable<(string, IDoubleVector)> stream, string dictFile, string vecFile);

    /**
     * A writer for a stream of StringVectorPairs. This is mainly used to rewrite
     * text to binary files or vice versa.
     * 
     * @param stream the stream of elements to write.
     * @param output depending on the implementation, either a file or a folder.
     * @throws IOException file/directory doesn't exist, isn't writable, or other
     * io errors.
     */
    public Task WriteStreamAsync(IAsyncEnumerable<(string, IDoubleVector)> stream, string dictFile, string vecFile);
}
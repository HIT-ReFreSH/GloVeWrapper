﻿using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Threading.Tasks;
using HitRefresh.GloVeWrapper.Models;

namespace HitRefresh.GloVeWrapper.IO;

/// <summary>
/// </summary>
public class CachedGloveAccessor : IGloveAccessor, IDisposable
{
    /// <summary>
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="maxCacheSize">Max Cache Size, unit is MB</param>
    /// <param name="name"></param>
    public CachedGloveAccessor(IGloveAccessor reader, int maxCacheSize = 0, string name = "glove")
    {
        Reader = reader;
        Cache = new MemoryCache(name, new NameValueCollection
        {
            {"cacheMemoryLimitMegabytes", $"{maxCacheSize}"}
        }, true);
    }

    private IGloveAccessor Reader { get; }
    private MemoryCache Cache { get; }

    /// <inheritdoc />
    public bool Contains(string word)
    {
        return Reader.Contains(word);
    }

    /// <inheritdoc />
    public IDoubleVector? this[string word]
    {
        get
        {
            if (Cache.Contains(word)) return (IDoubleVector) Cache[word];
            if (!Reader.Contains(word)) return default;
            var ret = Reader[word];
            Cache.Set(new CacheItem(word, ret), new CacheItemPolicy());

            return ret;
        }
    }

    /// <inheritdoc />
    public async Task<IDoubleVector?> GetAsync(string word)
    {
        if (Cache.Contains(word)) return (IDoubleVector) Cache[word];
        if (!Reader.Contains(word)) return default;
        var ret = await Reader.GetAsync(word);
        Cache.Set(new CacheItem(word, ret), new CacheItemPolicy());

        return ret;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
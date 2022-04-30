using System;
using System.Numerics;

namespace HitRefresh.GloVeWrapper.Models;

using static IDoubleVector;

/// <summary>
/// </summary>
public readonly ref struct RefDenseDoubleVector
{
    private readonly Span<Vector<double>> _vectors;

    /// <summary>
    /// </summary>
    public int Length { get; }

    /// <summary>
    ///     DefaultDv
    /// </summary>
    public static RefDenseDoubleVector Empty()
    {
        return new(0);
    }


    /// <summary>
    /// </summary>
    public bool IsEmpty => Length == 0;

    /// <summary>
    ///     Create a Double Vector with all-zero values
    /// </summary>
    /// <param name="length"></param>
    private RefDenseDoubleVector(int length)
    {
        _vectors = Array.Empty<Vector<double>>();
        Length = length;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public DenseDoubleVector AsPlain()
    {
        return new(Length, _vectors);
    }

    /// <summary>
    ///     Create a Double Vector with all-zero values
    /// </summary>
    /// <param name="length"></param>
    /// <param name="vectors"></param>
    internal RefDenseDoubleVector(int length, Span<Vector<double>> vectors)
    {
        _vectors = vectors;
        Length = length;
    }

    /// <summary>
    ///     Create a Double Vector From memory
    /// </summary>
    /// <param name="values"></param>
    public RefDenseDoubleVector(double[] values)
    {
        Length = values.Length;
        _vectors = new Vector<double>[Length >> ShiftSize];
        var span = new ReadOnlySpan<double>(values);
        Span<double> tmp = stackalloc double[BlockSize];
        for (var i = 0; i < _vectors.Length; i++)
        {
            var lo = i << ShiftSize;
            var hi = lo + BlockSize;
            if (hi >= Length)
            {
                span[lo..].CopyTo(tmp);
                _vectors[i] = new Vector<double>(tmp);
            }
            else
            {
                _vectors[i] = new Vector<double>(span[lo..hi]);
            }
        }
    }

    /// <summary>
    ///     Create a Double Vector From memory
    /// </summary>
    /// <param name="values"></param>
    public RefDenseDoubleVector(Span<double> values)
    {
        Length = values.Length;
        _vectors = new Vector<double>[Length >> ShiftSize];
        var span = values;
        Span<double> tmp = stackalloc double[BlockSize];
        for (var i = 0; i < _vectors.Length; i++)
        {
            var lo = i << ShiftSize;
            var hi = lo + BlockSize;
            if (hi >= Length)
            {
                span[lo..].CopyTo(tmp);
                _vectors[i] = new Vector<double>(tmp);
            }
            else
            {
                _vectors[i] = new Vector<double>(span[lo..hi]);
            }
        }
    }


    /// <summary>
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public void Add(RefDenseDoubleVector v)
    {
        if (v.Length != Length)
            throw new ArgumentException("_vectors' length aren't equaled OR not dense.", nameof(v));
        for (var i = 0; i < _vectors.Length; i++) _vectors[i] += v._vectors[i];
    }

    private static Vector<double> SingleValue(double scalar)
    {
        return new Vector<double>(scalar);
    }

    /// <inheritdoc />
    public void Add(double scalar)
    {
        var sv = SingleValue(scalar);
        for (var i = 0; i < _vectors.Length; i++) _vectors[i] += sv;
    }

    /// <inheritdoc />
    public void Subtract(RefDenseDoubleVector v)
    {
        if (v.Length != Length)
            throw new ArgumentException("_vectors' length aren't equaled OR not dense.", nameof(v));
        for (var i = 0; i < _vectors.Length; i++) _vectors[i] -= v._vectors[i];
    }

    /// <inheritdoc />
    public void Subtract(double scalar)
    {
        var sv = SingleValue(scalar);
        for (var i = 0; i < _vectors.Length; i++) _vectors[i] -= sv;
    }

    /// <inheritdoc />
    public void SubtractFrom(double scalar)
    {
        var sv = SingleValue(scalar);
        for (var i = 0; i < _vectors.Length; i++) _vectors[i] = sv - _vectors[i];
    }


    /// <inheritdoc />
    public void Abs()
    {
        for (var i = 0; i < _vectors.Length; i++) _vectors[i] = Vector.Abs(_vectors[i]);
    }


    /// <inheritdoc />
    public void Negate()
    {
        for (var i = 0; i < _vectors.Length; i++) _vectors[i] = Vector.Negate(_vectors[i]);
    }


    /// <summary>
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public double Dot(RefDenseDoubleVector v)
    {
        if (v.Length != Length)
            throw new ArgumentException("_vectors' length aren't equaled OR not dense.", nameof(v));
        var sum = 0.0;
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < _vectors.Length; i++) sum += Vector.Dot(_vectors[i], v._vectors[i]);
        return sum;
    }


    /// <inheritdoc />
    public double Modulus => Math.Sqrt(Dot(this));
}
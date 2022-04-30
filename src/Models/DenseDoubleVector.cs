using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace HitRefresh.GloVeWrapper.Models;

using static IDoubleVector;

/// <summary>
///     a dense double vector
/// </summary>
public readonly struct DenseDoubleVector : IDoubleVector
{
    /// <summary>
    ///     DefaultDv
    /// </summary>
    public static DenseDoubleVector Empty { get; } = new(0);

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public RefDenseDoubleVector AsRef()
    {
        return new(Length, Vectors);
    }

    internal readonly Vector<double>[] Vectors;

    /// <summary>
    /// </summary>
    public bool IsEmpty => Length == 0;

    /// <summary>
    /// </summary>
    /// <param name="length"></param>
    public DenseDoubleVector(int length)
    {
        Vectors = new Vector<double>[length >> ShiftSize];
        Length = length;
    }

    /// <summary>
    ///     Create a Double Vector with all-zero values
    /// </summary>
    /// <param name="length"></param>
    /// <param name="vectors"></param>
    private DenseDoubleVector(int length, Vector<double>[] vectors)
    {
        Vectors = vectors;
        Length = length;
    }

    /// <summary>
    ///     Create a Double Vector with all-zero values
    /// </summary>
    /// <param name="length"></param>
    /// <param name="vectors"></param>
    internal DenseDoubleVector(int length, Span<Vector<double>> vectors) : this(length, vectors.ToArray())
    {
    }

    /// <summary>
    ///     Create a Double Vector From memory
    /// </summary>
    /// <param name="values"></param>
    public DenseDoubleVector(double[] values)
    {
        Length = values.Length;
        Vectors = new Vector<double>[Length >> ShiftSize];
        var span = new ReadOnlySpan<double>(values);
        Span<double> tmp = stackalloc double[BlockSize];
        for (var i = 0; i < Vectors.Length; i++)
        {
            var lo = i << ShiftSize;
            var hi = lo + BlockSize;
            if (hi >= Length)
            {
                span[lo..].CopyTo(tmp);
                Vectors[i] = new Vector<double>(tmp
                );
            }
            else
            {
                Vectors[i] = new Vector<double>(span[lo..hi]);
            }
        }
    }

    /// <summary>
    ///     Create a Double Vector From memory
    /// </summary>
    /// <param name="values"></param>
    public DenseDoubleVector(Span<double> values)
    {
        Length = values.Length;
        Vectors = new Vector<double>[Length >> ShiftSize];
        var span = values;
        Span<double> tmp = stackalloc double[BlockSize];
        for (var i = 0; i < Vectors.Length; i++)
        {
            var lo = i << ShiftSize;
            var hi = lo + BlockSize;
            if (hi >= Length)
            {
                span[lo..].CopyTo(tmp);
                Vectors[i] = new Vector<double>(tmp
                );
            }
            else
            {
                Vectors[i] = new Vector<double>(span[lo..hi]);
            }
        }
    }

    /// <inheritdoc />
    public IEnumerator<double> GetEnumerator()
    {
        return GetEnumerable(this).GetEnumerator();
    }

    private static IEnumerable<double> GetEnumerable(DenseDoubleVector vec)
    {
        foreach (var vector in vec.Vectors)
            for (var i = 0; i < BlockSize; i++)
                yield return vector[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public double this[int x]
    {
        get => Vectors[x >> ShiftSize][x & Mask];
        set
        {
            var targetVecIndex = x >> ShiftSize;
            Span<double> vecValue = stackalloc double[BlockSize];
            Vectors[targetVecIndex].CopyTo(vecValue);
            vecValue[x & Mask] = value;
            Vectors[targetVecIndex] = new Vector<double>(vecValue);
        }
    }

    /// <inheritdoc />
    public IDoubleVector this[Range range] => throw new NotImplementedException();

    /// <inheritdoc />
    public int Length { get; }

    /// <inheritdoc />
    public IDoubleVector Clone()
    {
        var newArr = new Vector<double>[Vectors.Length];
        Array.Copy(Vectors, newArr, Vectors.Length);
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Add(IDoubleVector v)
    {
        if (v.Length != Length || v is not DenseDoubleVector dv)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        var newArr = new Vector<double>[Vectors.Length];
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = dv.Vectors[i] + Vectors[i];
        return Add(dv);
    }

    /// <summary>
    /// </summary>
    /// <param name="v"></param>
    /// <exception cref="ArgumentException"></exception>
    public void AddWith(DenseDoubleVector v)
    {
        if (v.Length != Length)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        for (var i = 0; i < Vectors.Length; i++) Vectors[i] += v.Vectors[i];
    }

    /// <summary>
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private DenseDoubleVector Add(DenseDoubleVector v)
    {
        if (v.Length != Length)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        var newArr = new Vector<double>[Vectors.Length];
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = v.Vectors[i] + Vectors[i];
        return new DenseDoubleVector(Length, newArr);
    }

    private static Vector<double> SingleValue(double scalar)
    {
        return new Vector<double>(scalar);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Add(double scalar)
    {
        var newArr = new Vector<double>[Vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = sv + Vectors[i];
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Subtract(IDoubleVector v)
    {
        if (v.Length != Length || v is not DenseDoubleVector dv)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        var newArr = new Vector<double>[Vectors.Length];
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = Vectors[i] - dv.Vectors[i];
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Subtract(double scalar)
    {
        var newArr = new Vector<double>[Vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = Vectors[i] - sv;
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.SubtractFrom(double scalar)
    {
        var newArr = new Vector<double>[Vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = sv - Vectors[i];
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Multiply(double scalar)
    {
        var newArr = new Vector<double>[Vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = sv * Vectors[i];
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Multiply(IDoubleVector v)
    {
        if (v.Length != Length || v is not DenseDoubleVector dv)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        var newArr = new Vector<double>[Vectors.Length];
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = Vectors[i] - dv.Vectors[i];
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Divide(double scalar)
    {
        var newArr = new Vector<double>[Vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = Vectors[i] / sv;
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.DivideFrom(double scalar)
    {
        var newArr = new Vector<double>[Vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = sv / Vectors[i];
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Divide(IDoubleVector v)
    {
        if (v.Length != Length || v is not DenseDoubleVector dv)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        var newArr = new Vector<double>[Vectors.Length];
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = Vectors[i] - dv.Vectors[i];
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Pow(double x)
    {
        var newArr = new Vector<double>[Vectors.Length];
        Span<double> span = stackalloc double[BlockSize];
        for (var i = 0; i < Vectors.Length; i++)
        {
            for (var j = 0; j < BlockSize; j++) span[j] = Math.Pow(Vectors[i][j], x);

            newArr[i] = new Vector<double>(span);
        }

        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    public IDoubleVector Abs()
    {
        var newArr = new Vector<double>[Vectors.Length];
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = Vector.Abs(Vectors[i]);
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    public IDoubleVector Sqrt()
    {
        var newArr = new Vector<double>[Vectors.Length];
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = Vector.SquareRoot(Vectors[i]);
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Negate()
    {
        var newArr = new Vector<double>[Vectors.Length];
        for (var i = 0; i < Vectors.Length; i++) newArr[i] = Vector.Negate(Vectors[i]);
        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    public IDoubleVector Log()
    {
        var newArr = new Vector<double>[Vectors.Length];
        Span<double> span = stackalloc double[BlockSize];
        for (var i = 0; i < Vectors.Length; i++)
        {
            for (var j = 0; j < BlockSize; j++) span[j] = Math.Log(Vectors[i][j]);

            newArr[i] = new Vector<double>(span);
        }

        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    public IDoubleVector Exp()
    {
        var newArr = new Vector<double>[Vectors.Length];
        Span<double> span = stackalloc double[BlockSize];
        for (var i = 0; i < Vectors.Length; i++)
        {
            for (var j = 0; j < BlockSize; j++) span[j] = Math.Exp(Vectors[i][j]);

            newArr[i] = new Vector<double>(span);
        }

        return new DenseDoubleVector(Length, newArr);
    }

    /// <inheritdoc />
    public double Sum()
    {
        return Vectors.Sum(Vector.Sum);
    }

    /// <inheritdoc />
    public double Dot(IDoubleVector v)
    {
        if (v is not DenseDoubleVector dv)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        return Dot(dv);
    }

    /// <summary>
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public double Dot(DenseDoubleVector v)
    {
        if (v.Length != Length)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        var sum = 0.0;
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < Vectors.Length; i++) sum += Vector.Dot(Vectors[i], v.Vectors[i]);
        return sum;
    }

    /// <inheritdoc />
    public (double, int) Max()
    {
        var (maxv, maxi) = (double.MinValue, 0);
        for (var i = 0; i < Length; i++)
            if (this[i] > maxv)
                (maxv, maxi) = (this[i], i);

        return (maxv, maxi);
    }

    /// <inheritdoc />
    public (double, int) Min()
    {
        var (minv, mini) = (double.MaxValue, 0);
        for (var i = 0; i < Length; i++)
            if (this[i] < minv)
                (minv, mini) = (this[i], i);

        return (minv, mini);
    }

    /// <inheritdoc />
    public IEnumerable<(int, double)> AsIndexed()
    {
        for (var i = 0; i < Length; i++)
            yield return (i, this[i]);
    }

    /// <inheritdoc />
    public IEnumerable<(int, double)> AsIndexedNonZero()
    {
        for (var i = 0; i < Length; i++)
            if (0.0 != this[i])
                yield return (i, this[i]);
    }

    /// <inheritdoc />
    public bool Sparse => false;

    /// <inheritdoc />
    public double Modulus => Math.Sqrt(Dot(this));
}
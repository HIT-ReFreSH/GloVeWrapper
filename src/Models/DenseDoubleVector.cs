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
    private readonly Vector<double>[] _vectors;

    /// <summary>
    ///     Create a Double Vector with all-zero values
    /// </summary>
    /// <param name="length"></param>
    public DenseDoubleVector(int length)
    {
        _vectors = new Vector<double>[length >> ShiftSize];
        Length = length;
    }

    /// <summary>
    ///     Create a Double Vector with all-zero values
    /// </summary>
    /// <param name="length"></param>
    /// <param name="vectors"></param>
    private DenseDoubleVector(int length, Vector<double>[] vectors)
    {
        _vectors = vectors;
        Length = length;
    }

    /// <summary>
    ///     Create a Double Vector From memory
    /// </summary>
    /// <param name="values"></param>
    public DenseDoubleVector(double[] values)
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
                _vectors[i] = new Vector<double>(tmp
                );
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
    public DenseDoubleVector(Span<double> values)
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
                _vectors[i] = new Vector<double>(tmp
                );
            }
            else
            {
                _vectors[i] = new Vector<double>(span[lo..hi]);
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
        foreach (var vector in vec._vectors)
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
        get => _vectors[x >> ShiftSize][x & Mask];
        set
        {
            var targetVecIndex = x >> ShiftSize;
            Span<double> vecValue = stackalloc double[BlockSize];
            _vectors[targetVecIndex].CopyTo(vecValue);
            vecValue[x & Mask] = value;
            _vectors[targetVecIndex] = new Vector<double>(vecValue);
        }
    }

    /// <inheritdoc />
    public IDoubleVector this[Range range] => throw new NotImplementedException();

    /// <inheritdoc />
    public int Length { get; }

    /// <inheritdoc />
    public IDoubleVector Clone()
    {
        var newArr = new Vector<double>[_vectors.Length];
        Array.Copy(_vectors, newArr, _vectors.Length);
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Add(IDoubleVector v)
    {
        if (v.Length != Length || v is not DenseDoubleVector dv)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        var newArr = new Vector<double>[_vectors.Length];
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = dv._vectors[i] + _vectors[i];
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    private static Vector<double> SingleValue(double scalar)
    {
        return new Vector<double>(scalar);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Add(double scalar)
    {
        var newArr = new Vector<double>[_vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = sv + _vectors[i];
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Subtract(IDoubleVector v)
    {
        if (v.Length != Length || v is not DenseDoubleVector dv)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        var newArr = new Vector<double>[_vectors.Length];
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = _vectors[i] - dv._vectors[i];
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Subtract(double scalar)
    {
        var newArr = new Vector<double>[_vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = _vectors[i] - sv;
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.SubtractFrom(double scalar)
    {
        var newArr = new Vector<double>[_vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = sv - _vectors[i];
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Multiply(double scalar)
    {
        var newArr = new Vector<double>[_vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = sv * _vectors[i];
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Multiply(IDoubleVector v)
    {
        if (v.Length != Length || v is not DenseDoubleVector dv)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        var newArr = new Vector<double>[_vectors.Length];
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = _vectors[i] - dv._vectors[i];
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Divide(double scalar)
    {
        var newArr = new Vector<double>[_vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = _vectors[i] / sv;
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.DivideFrom(double scalar)
    {
        var newArr = new Vector<double>[_vectors.Length];
        var sv = SingleValue(scalar);
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = sv / _vectors[i];
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Divide(IDoubleVector v)
    {
        if (v.Length != Length || v is not DenseDoubleVector dv)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));
        var newArr = new Vector<double>[_vectors.Length];
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = _vectors[i] - dv._vectors[i];
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Pow(double x)
    {
        var newArr = new Vector<double>[_vectors.Length];
        Span<double> span = stackalloc double[BlockSize];
        for (var i = 0; i < _vectors.Length; i++)
        {
            for (var j = 0; j < BlockSize; j++) span[j] = Math.Pow(_vectors[i][j], x);

            newArr[i] = new Vector<double>(span);
        }

        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    public IDoubleVector Abs()
    {
        var newArr = new Vector<double>[_vectors.Length];
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = Vector.Abs(_vectors[i]);
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    public IDoubleVector Sqrt()
    {
        var newArr = new Vector<double>[_vectors.Length];
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = Vector.SquareRoot(_vectors[i]);
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    IDoubleVector IDoubleVector.Negate()
    {
        var newArr = new Vector<double>[_vectors.Length];
        for (var i = 0; i < _vectors.Length; i++) newArr[i] = Vector.Negate(_vectors[i]);
        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    public IDoubleVector Log()
    {
        var newArr = new Vector<double>[_vectors.Length];
        Span<double> span = stackalloc double[BlockSize];
        for (var i = 0; i < _vectors.Length; i++)
        {
            for (var j = 0; j < BlockSize; j++) span[j] = Math.Log(_vectors[i][j]);

            newArr[i] = new Vector<double>(span);
        }

        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    public IDoubleVector Exp()
    {
        var newArr = new Vector<double>[_vectors.Length];
        Span<double> span = stackalloc double[BlockSize];
        for (var i = 0; i < _vectors.Length; i++)
        {
            for (var j = 0; j < BlockSize; j++) span[j] = Math.Exp(_vectors[i][j]);

            newArr[i] = new Vector<double>(span);
        }

        return new DenseDoubleVector(_vectors.Length, newArr);
    }

    /// <inheritdoc />
    public double Sum()
    {
        return _vectors.Sum(Vector.Sum);
    }

    /// <inheritdoc />
    public double Dot(IDoubleVector v)
    {
        if (v.Length != Length || v is not DenseDoubleVector dv)
            throw new ArgumentException("Vectors' length aren't equaled OR not dense.", nameof(v));

        return _vectors.Select((t, i) => Vector.Dot(t, dv._vectors[i])).Sum();
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
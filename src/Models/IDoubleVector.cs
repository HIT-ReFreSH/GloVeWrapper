using System;
using System.Collections.Generic;
using System.Numerics;

namespace HitRefresh.GloVeWrapper.Models;

/**
 * Vector with doubles. Some of the operations are mutable, unlike the apply and
 * math functions, they return a fresh instance every time.
 */
public interface IDoubleVector : IEnumerable<double>
{
    /// <summary>
    ///     BlockSize of Base type(Vector&lt;double&gt;.Count)
    /// </summary>
    protected internal static int BlockSize { get; } = Vector<double>.Count;

    /// <summary>
    ///     Bit-accelerated calculation for / BlockSize
    /// </summary>
    protected internal static int ShiftSize { get; } = BlockSize switch
    {
        1 => 0,
        2 => 1,
        4 => 2,
        8 => 3,
        16 => 4,
        32 => 5,
        64 => 6,
        128 => 7,
        256 => 8,
        _ => throw new ArgumentOutOfRangeException(nameof(BlockSize))
    };

    /// <summary>
    ///     Bit-accelerated calculation for mod BlockSize
    /// </summary>
    protected static int Mask { get; } = BlockSize - 1;

    /// <summary>
    ///     Get or Set value of certain Index
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public double this[int x] { get; set; }

    /// <summary>
    ///     Get or Set value of certain Index
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public double this[Index x]
    {
        get => this[x.IsFromEnd ? Length - x.Value : x.Value];
        set => this[x.IsFromEnd ? Length - x.Value : x.Value] = value;
    }

    /// <summary>
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public IDoubleVector this[Range range] { get; }

    /// <summary>
    ///     Length of Vector
    /// </summary>
    /// <returns></returns>
    public int Length { get; }

    /**
     * @return true if this instance is a sparse vector.
     */
    public bool Sparse { get; }

    /// <summary>
    ///     Modulus of Vector.
    /// </summary>
    public double Modulus { get; }

    /**
     * @return a fresh new copy of this vector, copies all elements to a new
     * vector. (Does not reuse references or stuff).
     */
    public IDoubleVector Clone();

    /**
   * Adds the given {@link IDoubleVector} to this vector.
   * 
   * @param v the other vector.
   * @return a new vector with the sum of both vectors at each element index.
   */
    protected IDoubleVector Add(IDoubleVector v);

    /**
     * Adds the given scalar to this vector.
     * 
     * @param scalar the scalar.
     * @return a new vector with the result at each element index.
     */
    protected IDoubleVector Add(double scalar);

    /**
     * Subtracts this vector by the given {@link IDoubleVector}.
     * 
     * @param v the other vector.
     * @return a new vector with the difference of both vectors.
     */
    protected IDoubleVector Subtract(IDoubleVector v);

    /**
     * Subtracts the given scalar to this vector. (vector - scalar).
     * 
     * @param scalar the scalar.
     * @return a new vector with the result at each element index.
     */
    protected IDoubleVector Subtract(double scalar);

    /**
     * Subtracts the given scalar from this vector. (scalar - vector).
     * 
     * @param scalar the scalar.
     * @return a new vector with the result at each element index.
     */
    protected IDoubleVector SubtractFrom(double scalar);

    /**
     * Multiplies the given scalar to this vector.
     * 
     * @param scalar the scalar.
     * @return a new vector with the result of the operation.
     */
    protected IDoubleVector Multiply(double scalar);

    /**
     * Multiplies the given {@link IDoubleVector} with this vector.
     * 
     * @param vector the other vector.
     * @return a new vector with the result of the operation.
     */
    protected IDoubleVector Multiply(IDoubleVector vector);

    /**
     * Divides this vector by the given scalar. (= vector/scalar).
     * 
     * @param scalar the given scalar.
     * @return a new vector with the result of the operation.
     */
    protected IDoubleVector Divide(double scalar)
    {
        var newArr = new double[Length];
        for (var i = 0; i < Length; i++) newArr[i] = this[i] / scalar;
        return new DenseDoubleVector(newArr);
    }

    /**
     * Divides the given scalar by this vector. (= scalar/vector).
     * 
     * @param scalar the given scalar.
     * @return a new vector with the result of the operation.
     */
    protected IDoubleVector DivideFrom(double scalar)
    {
        var newArr = new double[Length];
        for (var i = 0; i < Length; i++) newArr[i] = scalar / this[i];
        return new DenseDoubleVector(newArr);
    }


    /**
     * Divides this vector by the given vector. (= vector/parameter vector).
     * 
     * @param vector the given vector.
     * @return a new vector with the result of the operation.
     */
    protected IDoubleVector Divide(IDoubleVector vector);

    /**
     * Powers this vector by the given amount. (=vector^x).
     * 
     * @param x the given exponent.
     * @return a new vector with the result of the operation.
     */
    protected IDoubleVector Pow(double x);

    /**
     * Absolutes the vector at each element.
     * 
     * @return a new vector that does not contain negative values anymore.
     */
    public IDoubleVector Abs();

    /**
     * Square-roots each element.
     * 
     * @return a new vector.
     */
    public IDoubleVector Sqrt();

    /**
     * Inverse each element(x=-x).
     * 
     * @return a new vector.
     */
    protected IDoubleVector Negate();

    /**
     * Log base e for each element.
     * 
     * @return a new vector.
     */
    public IDoubleVector Log();

    /**
     * Pow every element with base e.
     * 
     * @return a new vector.
     */
    public IDoubleVector Exp();

    /**
     * @return the sum of all elements in this vector.
     */
    public double Sum();

    /**
     * Calculates the dot product between this vector and the given vector.
     * 
     * @param s the given vector s.
     * @return the dot product as a double.
     */
    public double Dot(IDoubleVector s);


    /**
     * @return the maximum element value in this vector. Note that on sparse
     * instances you may not see zero as the maximum.
     */
    public (double, int) Max();

    /**
     * @return the minimum element value in this vector. Note that on sparse
     * instances you may not see zero as the minimum.
     */
    public (double, int) Min();


    /**
     * @return an IEnumerable that only iterates over vector.
     */
    public IEnumerable<(int, double)> AsIndexed();

    /**
 * @return an IEnumerable that only iterates over non zero elements.
 */
    public IEnumerable<(int, double)> AsIndexedNonZero();

    /**
     * Adds the given {@link DoubleVector} to this vector.
     * 
     * @param v the other vector.
     * @return a new vector with the sum of both vectors at each element index.
     */
    public static IDoubleVector operator +(IDoubleVector v1, IDoubleVector v2)
    {
        return v1.Add(v2);
    }

    /**
     * Adds the given scalar to this vector.
     * 
     * @param scalar the scalar.
     * @return a new vector with the result at each element index.
     */
    public static IDoubleVector operator +(double v1, IDoubleVector v2)
    {
        return v2.Add(v1);
    }

    /**
     * Adds the given scalar to this vector.
     * 
     * @param scalar the scalar.
     * @return a new vector with the result at each element index.
     */
    public static IDoubleVector operator +(IDoubleVector v1, double v2)
    {
        return v1.Add(v2);
    }

    /**
     * Subtracts this vector by the given {@link DoubleVector}.
     * 
     * @param v the other vector.
     * @return a new vector with the difference of both vectors.
     */
    public static IDoubleVector operator -(IDoubleVector v1, IDoubleVector v2)
    {
        return v1.Subtract(v2);
    }

    /**
     * Subtracts the given scalar from this vector. (scalar - vector).
     * 
     * @param scalar the scalar.
     * @return a new vector with the result at each element index.
     */
    public static IDoubleVector operator -(double v1, IDoubleVector v2)
    {
        return v2.SubtractFrom(v1);
    }

    /**
     * Subtracts the given scalar to this vector. (vector - scalar).
     * 
     * @param scalar the scalar.
     * @return a new vector with the result at each element index.
     */
    public static IDoubleVector operator -(IDoubleVector v1, double v2)
    {
        return v1.Subtract(v2);
    }

    /**
     * Multiplies the given {@link DoubleVector} with this vector.
     * 
     * @param vector the other vector.
     * @return a new vector with the result of the operation.
     */
    public static IDoubleVector operator *(IDoubleVector v1, IDoubleVector v2)
    {
        return v1.Multiply(v2);
    }

    /**
     * Multiplies the given scalar to this vector.
     * 
     * @param scalar the scalar.
     * @return a new vector with the result of the operation.
     */
    public static IDoubleVector operator *(double v1, IDoubleVector v2)
    {
        return v2.Multiply(v1);
    }

    /**
     * Multiplies the given scalar to this vector.
     * 
     * @param scalar the scalar.
     * @return a new vector with the result of the operation.
     */
    public static IDoubleVector operator *(IDoubleVector v1, double v2)
    {
        return v1.Multiply(v2);
    }

    /**
     * Multiplies the given scalar to this vector.
     * 
     * @param scalar the scalar.
     * @return a new vector with the result of the operation.
     */
    public static IDoubleVector operator /(IDoubleVector v1, IDoubleVector v2)
    {
        return v1.Divide(v2);
    }

    /**
     * Divides the given scalar by this vector. (= scalar/vector).
     * 
     * @param scalar the given scalar.
     * @return a new vector with the result of the operation.
     */
    public static IDoubleVector operator /(double v1, IDoubleVector v2)
    {
        return v2.DivideFrom(v1);
    }

    /**
     * Divides this vector by the given scalar. (= vector/scalar).
     * 
     * @param scalar the given scalar.
     * @return a new vector with the result of the operation.
     */
    public static IDoubleVector operator /(IDoubleVector v1, double v2)
    {
        return v1.Divide(v2);
    }

    /**
     * Powers this vector by the given amount. (=vector^x).
     * 
     * @param x the given exponent.
     * @return a new vector with the result of the operation.
     */
    public static IDoubleVector operator ^(IDoubleVector v1, double v2)
    {
        return v1.Pow(v2);
    }

    /**
     * Calculates the dot product between this vector and the given vector.
     * 
     * @param s the given vector s.
     * @return the dot product as a double.
     */
    public static double operator ^(IDoubleVector v1, IDoubleVector v2)
    {
        return v1.Dot(v2);
    }

    /**
     * Inverse each element(x=-x).
     * 
     * @return a new vector.
     */
    public static IDoubleVector operator -(IDoubleVector v1)
    {
        return v1.Negate();
    }
}
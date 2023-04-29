using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Avx2;
using static System.Runtime.Intrinsics.X86.Avx;
using static System.Runtime.Intrinsics.X86.Sse2;
using static System.Runtime.Intrinsics.X86.Sse;

namespace AVXPerlinNoise;

public class VectorUtils
{
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector256<int> Create(int[] a)
    {
        if (a.Length != 8)
        {
            throw new ArgumentException($"{nameof(a)} needs to hold 8 numbers!");
        }

        fixed(int* add = &a[0])
        {
            return LoadVector256(add);
        }
    }
        
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector128<int> CreateVec128(int[] a)
    {
        if (a.Length != 4)
        {
            throw new ArgumentException($"{nameof(a)} needs to hold 4 numbers!");
        }

        fixed(int* add = &a[0])
        {
            return LoadVector128(add);
        }
    }
        
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector256<float> Create(Span<float> a)
    {
        if (a.Length != 8)
        {
            throw new ArgumentException($"{nameof(a)} needs to hold 8 numbers!");
        }

        fixed(float* add = &a.GetPinnableReference())
        {
            return LoadVector256(add);
        }
    }
        
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector256<float> Create(float[] a)
    {
        if (a.Length != 8)
        {
            throw new ArgumentException($"{nameof(a)} needs to hold 8 numbers!");
        }

        fixed(float* add = &a[0])
        {
            return LoadVector256(add);
        }
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector128<float> CreateVec128(float[] a)
    {
        if (a.Length != 4)
        {
            throw new ArgumentException($"{nameof(a)} needs to hold 4 numbers!");
        }

        fixed(float* add = &a[0])
        {
            return LoadVector128(add);
        }
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector128<float> LoadVector128Correctly(float count)
    {
        var tobe = stackalloc float[1];
        tobe[0] = count;
        return BroadcastScalarToVector128(tobe);
    }
        
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector128<int> LoadVector128Correctly(int count)
    {
        var tobe = stackalloc int[1];
        tobe[0] = count;
        return BroadcastScalarToVector128(tobe);
    }
        
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector256<float> LoadVectorCorrectly(float count)
    {
        var tobe = stackalloc float[1];
        tobe[0] = count;
        return BroadcastScalarToVector256(tobe);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector256<int> LoadVectorCorrectly(int count)
    {
        var tobe = stackalloc int[1];
        tobe[0] = count;
        return BroadcastScalarToVector256(tobe);
    }
}
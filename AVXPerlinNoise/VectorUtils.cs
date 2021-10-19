using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Avx2;
using static System.Runtime.Intrinsics.X86.Avx;
using static System.Runtime.Intrinsics.X86.Sse42;
using static System.Runtime.Intrinsics.X86.Sse41;
using static System.Runtime.Intrinsics.X86.Sse3;
using static System.Runtime.Intrinsics.X86.Sse2;
using static System.Runtime.Intrinsics.X86.Sse;

namespace AVXPerlinNoise
{
    public class VectorUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector256<int> Create(int[] a)
        {
            if (a.Length != 8)
                throw new ArgumentException($"{nameof(a)} needs to hold 8 numbers!");
            
            fixed(int* add = &a[0])
            {
                return LoadVector256(add);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector128<int> CreateVec128(int[] a)
        {
            if (a.Length != 4)
                throw new ArgumentException($"{nameof(a)} needs to hold 4 numbers!");
            
            fixed(int* add = &a[0])
            {
                return LoadVector128(add);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector256<float> Create(float[] a)
        {
            if (a.Length != 8)
                throw new ArgumentException($"{nameof(a)} needs to hold 8 numbers!");
            
            fixed(float* add = &a[0])
            {
                return LoadVector256(add);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector256<double> Create(double[] a)
        {
            if (a.Length != 4)
                throw new ArgumentException($"{nameof(a)} needs to hold 8 numbers!");
            
            fixed(double* add = &a[0])
            {
                return LoadVector256(add);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector128<float> CreateVec128(float[] a)
        {
            if (a.Length != 4)
                throw new ArgumentException($"{nameof(a)} needs to hold 4 numbers!");
            
            fixed(float* add = &a[0])
            {
                return LoadVector128(add);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe Vector128<float> LoadVector128Correctly(float count)
        {
            var tobe = stackalloc float[1];
            tobe[0] = count;
            return BroadcastScalarToVector128(tobe);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe Vector128<int> LoadVector128Correctly(int count)
        {
            var tobe = stackalloc int[1];
            tobe[0] = count;
            return BroadcastScalarToVector128(tobe);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe Vector256<float> LoadVectorCorrectly(float count)
        {
            var tobe = stackalloc float[1];
            tobe[0] = count;
            return BroadcastScalarToVector256(tobe);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe Vector256<double> LoadVectorCorrectly(double count)
        {
            var tobe = stackalloc double[1];
            tobe[0] = count;
            return BroadcastScalarToVector256(tobe);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe Vector256<int> LoadVectorCorrectly(int count)
        {
            var tobe = stackalloc int[1];
            tobe[0] = count;
            return BroadcastScalarToVector256(tobe);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe Vector256<long> LoadVectorCorrectly(long count)
        {
            var tobe = stackalloc long[1];
            tobe[0] = count;
            return BroadcastScalarToVector256(tobe);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe Vector128<float> LoadVector128WithMod(float count, float mod)
        {
            var countV = LoadVector128Correctly(count);
			
            var tobe = stackalloc float[4];
            tobe[0] = 1;
            tobe[1] = mod;
            tobe[2] = mod * mod;
            tobe[3] = mod * mod * mod;
            return Multiply(LoadVector128(tobe), countV);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe Vector256<float> LoadVectorWithMod(float count, float mod)
        {
            var countV = LoadVectorCorrectly(count);
			
            var tobe = stackalloc float[8];
            tobe[0] = 1;
            tobe[1] = mod;
            tobe[2] = mod                               * mod;
            tobe[3] = mod * mod                         * mod;
            tobe[4] = mod * mod * mod                   * mod;
            tobe[5] = mod * mod * mod * mod             * mod;
            tobe[6] = mod * mod * mod * mod * mod       * mod;
            tobe[7] = mod * mod * mod * mod * mod * mod * mod;
            return Multiply(LoadVector256(tobe), countV);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe Vector256<double> LoadVectorWithMod(double count, double mod)
        {
            var countV = LoadVectorCorrectly(count);
			
            var tobe = stackalloc double[4];
            tobe[0] = 1;
            tobe[1] = mod;
            tobe[2] = mod * mod;
            tobe[3] = mod * mod * mod;
            return Multiply(LoadVector256(tobe), countV);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector256<float> LoadVectorWithModScale(float count, float mod, float initial , float scale) 
            => Divide(Multiply(LoadVectorWithMod(count, mod), LoadVectorCorrectly(initial)), LoadVectorCorrectly(scale));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector256<double> LoadVectorWithModScale(double count, double mod, double initial , double scale) 
            => Divide(Multiply(LoadVectorWithMod(count, mod), LoadVectorCorrectly(initial)), LoadVectorCorrectly(scale));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector128<float> LoadVector128WithModScale(float count, float mod, float initial , float scale) 
            => Divide(Multiply(LoadVector128WithMod(count, mod), LoadVector128Correctly(initial)), LoadVector128Correctly(scale));
    }
}
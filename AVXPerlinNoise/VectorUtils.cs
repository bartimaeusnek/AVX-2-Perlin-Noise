using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Avx2;
using static System.Runtime.Intrinsics.X86.Avx;

namespace AVXPerlinNoise
{
    public class VectorUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector256<int> Create(int[] a)
        {
            fixed(int* add = &a[0])
            {
                return LoadVector256(add);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector256<float> Create(float[] a)
        {
            fixed(float* add = &a[0])
            {
                return LoadVector256(add);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe Vector256<float> LoadVectorCorrectly(float count)
        {
            var tobe = stackalloc float[1];
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
        public static Vector256<float> LoadVectorWithModScale(float count, float mod, float initial , float scale) 
            => Divide(Multiply(LoadVectorWithMod(count, mod), LoadVectorCorrectly(initial)), LoadVectorCorrectly(scale));
    }
}
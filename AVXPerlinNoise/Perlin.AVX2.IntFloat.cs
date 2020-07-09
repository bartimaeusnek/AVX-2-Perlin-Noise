using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Avx2;
using static System.Runtime.Intrinsics.X86.Avx;
using static AVXPerlinNoise.VectorUtils;

namespace AVXPerlinNoise
{
	public partial class Perlin
	{
		private static readonly Vector256<int> v255 = LoadVectorCorrectly(255);
		private static readonly Vector256<int> v1 = LoadVectorCorrectly(1);
		private static readonly Vector256<int> v2 = LoadVectorCorrectly(2);
		private static readonly Vector256<float> v2f = LoadVectorCorrectly(2f);
		private static readonly Vector256<float> v1f = LoadVectorCorrectly(1f);
		private static readonly Vector256<int> v4 = LoadVectorCorrectly(4);
		private static readonly Vector256<float> v4f = LoadVectorCorrectly(4f);
		private static readonly Vector256<float> v6f = LoadVectorCorrectly(6f);
		private static readonly Vector256<int> v8 = LoadVectorCorrectly(8);
		private static readonly Vector256<float> v10f = LoadVectorCorrectly(10f);
		private static readonly Vector256<int> v12 = LoadVectorCorrectly(12);
		private static readonly Vector256<int> v13 = LoadVectorCorrectly(13);
		private static readonly Vector256<int> v14 = LoadVectorCorrectly(14);
		private static readonly Vector256<float> v15f = LoadVectorCorrectly(15f);
		private static readonly Vector256<int> v15 = LoadVectorCorrectly(15);
		
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> perlinAVX(Vector256<float> x, Vector256<float> y, Vector256<float> z)
		{
			var xi = And(ConvertToVector256Int32WithTruncation(x), v255);
			var yi = And(ConvertToVector256Int32WithTruncation(y), v255);
			var zi = And(ConvertToVector256Int32WithTruncation(z), v255);

			var xf = Subtract(x, ConvertToVector256Single(ConvertToVector256Int32WithTruncation(x)));
			var yf = Subtract(y, ConvertToVector256Single(ConvertToVector256Int32WithTruncation(y)));
			var zf = Subtract(z, ConvertToVector256Single(ConvertToVector256Int32WithTruncation(z)));

			var u = fadeAVX(xf);
			var v = fadeAVX(yf);
			var w = fadeAVX(zf);

			var a  = UnpackPermutationArrayAndAdd(xi,                           yi);
			var aa = UnpackPermutationArrayAndAdd(a,                            zi);
			var ab = UnpackPermutationArrayAndAdd(Add(a,  v1), zi);
			var b  = UnpackPermutationArrayAndAdd(Add(xi, v1), yi);
			var ba = UnpackPermutationArrayAndAdd(b,                            zi);
			var bb = UnpackPermutationArrayAndAdd(Add(b, v1),  zi);

			var x1 = lerpAVX(gradAVX(UnpackPermutationArray(aa), xf,                                 yf, zf),
			                 gradAVX(UnpackPermutationArray(ba), Subtract(xf, v1f), yf, zf),
			                 u);
			var x2 = lerpAVX(gradAVX(UnpackPermutationArray(ab), xf, Subtract(yf, v1f), zf),
			                 gradAVX(UnpackPermutationArray(bb), Subtract(xf, v1f),
			                         Subtract(yf,                             v1f), zf),
			                 u);
			var y1 = lerpAVX(x1, x2, v);

			x1 =
				lerpAVX(gradAVX(UnpackPermutationArray(Add(aa, v1)), xf, yf, Subtract(zf, v1f)),
				        gradAVX(UnpackPermutationArray(ba), Subtract(xf, v1f), yf,
				                Subtract(zf,                             v1f)),
				        u);

			x2 =
				lerpAVX(gradAVX(UnpackPermutationArray(Add(ab, v1)), xf, Subtract(yf, v1f), Subtract(zf, v1f)),
				        gradAVX(UnpackPermutationArray(Add(bb, v1)),
				                Subtract(xf, v1f), Subtract(yf, v1f),
				                Subtract(zf, v1f)),
				        u);


			var y2 = lerpAVX(x1, x2, v);

			return Divide(Add(lerpAVX(y1, y2, w), v1f), v2f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> lerpAVX(Vector256<float> a, Vector256<float> b, Vector256<float> x)
			=> Add(a, Multiply(x, Subtract(b, a)));

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> fadeAVX(Vector256<float> t)
			=> Multiply(
			            Multiply(
			                     Multiply(
			                              t,
			                              t
			                             ),
			                     t
			                    ),
			            Add(
			                Multiply(
			                         t,
			                         Subtract(
			                                  Multiply(
			                                           t,
			                                           v6f
			                                          ),
			                                  v15f
			                                 )
			                        ),
			                v10f
			               )
			           );

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static Vector256<int> UnpackPermutationArrayAndAdd(Vector256<int> xi, Vector256<int> yi)
			=> Add(UnpackPermutationArray(xi), yi);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe Vector256<int> UnpackPermutationArray(Vector256<int> xi)
		{
			fixed (int* pData = &p[0])
			{
				return GatherVector256(pData, xi, sizeof(int));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVXUVector(Vector256<int> hashs, Vector256<float> xs, Vector256<float> ys)
		{
			// If the most signifigant bit (MSB) of the hash is 0 then set u = x.  Otherwise y.
			var h   = CompareGreaterThan(v8, hashs);

			var usX = And(h.AsSingle(), xs);
			var usY = AndNot(h.AsSingle(), ys);

			return Add(usX, usY);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVXVYVector(Vector256<int> hashs, Vector256<float> ys)
		{
			// If the first and second signifigant bits are 0 set v = y
			var ssbh = CompareGreaterThan(v4, hashs); // "< 4" == "4 >" 

			return And(ssbh.AsSingle(), ys);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVXVXVector(Vector256<int> hashs, Vector256<float> xs)
		{
			// If the first and second signifigant bits are 0 set v = y
			var TSB1h = CompareEqual(hashs, v12);
			var TSB2h = CompareEqual(hashs, v14);

			var vsX1 = And(TSB1h.AsSingle(), xs);
			var vsX2 = And(TSB2h.AsSingle(), xs);

			return Add(vsX1, vsX2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVXVZVector(Vector256<int> hashs, Vector256<float> zs)
		{
			// If the first and second signifigant bits are 0 set v = y
			
			var bigger4   = CompareGreaterThan(hashs, v4);
			var smaller12 = CompareGreaterThan(v12,  hashs);

			var range1 = And(bigger4, smaller12);

			var eq13 = CompareEqual(hashs, v13);

			var range = Or(range1, eq13);

			return And(range.AsSingle(), zs);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVXVVector(Vector256<int>   hashs, Vector256<float> xs, Vector256<float> ys,
		                                              Vector256<float> zs)
		{
			var VX = gradAVXVXVector(hashs, xs);
			var VY = gradAVXVYVector(hashs, ys);
			var VZ = gradAVXVZVector(hashs, zs);

			return Add(Add(VX, VY), VZ);
		}

		
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static Vector256<float> gradAVXPartialA(Vector256<int> hashs, Vector256<float> u)
		{
			var hAnd1  = And(hashs, v1);
			var hZeros = CompareEqual(hAnd1, Vector256<int>.Zero);
			var hOnes  = CompareGreaterThan(hAnd1, Vector256<int>.Zero);

			var ucomp    = And(hZeros.AsSingle(), u);
			var ucompneg = Subtract(Vector256<float>.Zero, And(hOnes.AsSingle(), u));
			return Add(ucomp, ucompneg);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static Vector256<float> gradAVXPartialB(Vector256<int> hashs, Vector256<float> u)
		{
			var hAnd1  = And(hashs, v2);
			var hZeros = CompareEqual(hAnd1, Vector256<int>.Zero);
			var hOnes  = CompareGreaterThan(hAnd1, Vector256<int>.Zero);

			var ucomp    = And(hZeros.AsSingle(), u);
			var ucompneg = Subtract(Vector256<float>.Zero, And(hOnes.AsSingle(), u));
			return Add(ucomp, ucompneg);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> gradAVX(Vector256<int>   hashs, Vector256<float> xs, Vector256<float> ys,
		                                       Vector256<float> zs)
		{
			var hs  = And(hashs, v15);

			var u = gradAVXUVector(hs, xs, ys);
			var v = gradAVXVVector(hs, xs, ys, zs);

			return Add(gradAVXPartialA(hs, u), gradAVXPartialB(hs, v));
		}
		
		//------------------ EXPERIMENTAL METHODES --------------------
		
		private static unsafe float SumVector(Vector256<float> input)
		{
			var ret = 0f;
			var flt = stackalloc float[8];
			Store(flt, input);
			for (var i = 0; i < 8; i++)
			{
				ret += flt[i];
			}

			return ret;
		} 
		
		internal static float OctavePerlinAVX(float x, float y, float z, int octaves, float persistence, float frequency = 1, float amplitude = 1) {
			if (octaves % 8 != 0)
				throw new InvalidOperationException("octaves must be divideable by 8!");
			var total = 0f;
			var i     = 0;

			var ax = LoadVectorCorrectly(x); 
			var bx = LoadVectorCorrectly(y); 
			var cx = LoadVectorCorrectly(z); 
			
			do
			{
				var ampl    = LoadVectorWithMod(amplitude, persistence); 
				var fq      = LoadVectorWithMod(frequency, 2);
				var ay      = Multiply(ax, fq);
				var by      = Multiply(bx, fq);
				var cy      = Multiply(cx, fq);
				var pavx    = perlinAVX(ay, by, cy);
				var pavxAmp = Multiply(pavx, ampl);
				
				total += SumVector(pavx);
				++i;
			} while (i < octaves / 8);

			return total;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe Vector256<float> LoadVectorWithMod(float count, float mod)
		{
			var tobe = stackalloc float[8];
			tobe[0] = count;
			tobe[1] = (count                                     * mod);
			tobe[2] = (count * mod                               * mod);
			tobe[3] = (count * mod * mod                         * mod);
			tobe[4] = (count * mod * mod * mod                   * mod);
			tobe[5] = (count * mod * mod * mod * mod             * mod);
			tobe[6] = (count * mod * mod * mod * mod * mod       * mod);
			tobe[7] = (count * mod * mod * mod * mod * mod * mod * mod);
			return LoadVector256(tobe);
		}
	}
}
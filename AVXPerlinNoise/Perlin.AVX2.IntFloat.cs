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
		private static readonly Vector256<int> v3 = LoadVectorCorrectly(3);
		private static readonly Vector256<int> v4 = LoadVectorCorrectly(4);
		private static readonly Vector256<float> v4f = LoadVectorCorrectly(4f);
		private static readonly Vector256<float> v6f = LoadVectorCorrectly(6f);
		private static readonly Vector256<int> v7 = LoadVectorCorrectly(7);
		private static readonly Vector256<int> v8 = LoadVectorCorrectly(8);
		private static readonly Vector256<float> v10f = LoadVectorCorrectly(10f);
		private static readonly Vector256<int> v11 = LoadVectorCorrectly(11);
		private static readonly Vector256<int> v12 = LoadVectorCorrectly(12);
		private static readonly Vector256<int> v13 = LoadVectorCorrectly(13);
		private static readonly Vector256<int> v14 = LoadVectorCorrectly(14);
		private static readonly Vector256<float> v15f = LoadVectorCorrectly(15f);
		private static readonly Vector256<int> v15 = LoadVectorCorrectly(15);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<int> MakeBitCutVector(Vector256<float> x)
			=> And(ConvertToVector256Int32WithTruncation(x), v255);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<float> MakeFloatCutVector(Vector256<float> x)
			=> Subtract(x, ConvertToVector256Single(ConvertToVector256Int32WithTruncation(x)));
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> perlinAVX(Vector256<float> x, Vector256<float> y, Vector256<float> z)
		{
#region Perlin Setup
			var xi = MakeBitCutVector(x);
			var yi = MakeBitCutVector(y);
			var zi = MakeBitCutVector(z);

			var xf = MakeFloatCutVector(x);
			var yf = MakeFloatCutVector(y);
			var zf = MakeFloatCutVector(z);

			var u = fadeAVX(xf);
			var v = fadeAVX(yf);
			var w = fadeAVX(zf);

			var a  = UnpackPermutationArrayAndAdd(xi,          yi);
			var aa = UnpackPermutationArrayAndAdd(a,           zi);
			var ab = UnpackPermutationArrayAndAdd(Add(a,  v1), zi);
			var b  = UnpackPermutationArrayAndAdd(Add(xi, v1), yi);
			var ba = UnpackPermutationArrayAndAdd(b,           zi);
			var bb = UnpackPermutationArrayAndAdd(Add(b, v1),  zi);
#endregion
			
#region Perlin Hash
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
				        gradAVX(UnpackPermutationArray(Add(ba, v1)), Subtract(xf, v1f), yf,
				                Subtract(zf,                             v1f)),
				        u);

			x2 =
				lerpAVX(gradAVX(UnpackPermutationArray(Add(ab, v1)), xf, Subtract(yf, v1f), Subtract(zf, v1f)),
				        gradAVX(UnpackPermutationArray(Add(bb, v1)),
				                Subtract(xf, v1f), Subtract(yf, v1f),
				                Subtract(zf, v1f)),
				        u);


			var y2 = lerpAVX(x1, x2, v);
#endregion
			return Divide(Add(lerpAVX(y1, y2, w), v1f), v2f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<float> lerpAVX(Vector256<float> a, Vector256<float> b, Vector256<float> x)
			=> Add(a, Multiply(x, Subtract(b, a)));

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<float> fadeAVX(Vector256<float> t)
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
		internal static Vector256<int> UnpackPermutationArrayAndAdd(Vector256<int> xi, Vector256<int> yi)
			=> Add(UnpackPermutationArray(xi), yi);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static unsafe Vector256<int> UnpackPermutationArray(Vector256<int> xi)
		{
			fixed (int* pData = &p[0])
			{
				return GatherVector256(pData, xi, sizeof(int));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<float> gradAVXUVector(Vector256<int> hashs, Vector256<float> xs, Vector256<float> ys)
		{
			var h   = CompareGreaterThan(v8, hashs);

			var usX = And(h.AsSingle(), xs);
			var usY = AndNot(h.AsSingle(), ys);

			return Add(usX, usY);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<float> gradAVXVVector(Vector256<int>   hashs, Vector256<float> xs, Vector256<float> ys,
		                                                Vector256<float> zs)
		{
			var TSB1h = CompareEqual(hashs, v12);
			var TSB2h = CompareEqual(hashs, v14);

			var ssbh = CompareGreaterThan(v4, hashs);

			var zMask = Xor(Xor(TSB1h, TSB2h), ssbh);
			
			var vsX1 = And(TSB1h.AsSingle(), xs);
			var vsX2 = And(TSB2h.AsSingle(), xs);

			return Add(Add(Add(vsX1, vsX2), And(ssbh.AsSingle(), ys)), AndNot(zMask.AsSingle(), zs));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<float> gradAVXPartialA(Vector256<int> hashs, Vector256<float> u)
		{
			var hAnd1  = And(hashs, v1);
			var hZeros = CompareEqual(hAnd1, Vector256<int>.Zero);
			var hOnes  = CompareGreaterThan(hAnd1, Vector256<int>.Zero);

			var ucomp    = And(hZeros.AsSingle(), u);
			var ucompneg = Subtract(Vector256<float>.Zero, And(hOnes.AsSingle(), u));
			return Add(ucomp, ucompneg);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<float> gradAVXPartialB(Vector256<int> hashs, Vector256<float> u)
		{
			var hAnd1  = And(hashs, v2);
			var hZeros = CompareEqual(hAnd1, Vector256<int>.Zero);
			var hOnes  = CompareGreaterThan(hAnd1, Vector256<int>.Zero);

			var ucomp    = And(hZeros.AsSingle(), u);
			var ucompneg = Subtract(Vector256<float>.Zero, And(hOnes.AsSingle(), u));
			return Add(ucomp, ucompneg);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<float> gradAVX(Vector256<int>   hashs, Vector256<float> xs, Vector256<float> ys,
		                                         Vector256<float> zs)
		{
			var hs  = And(hashs, v15);

			var u = gradAVXUVector(hs, xs, ys);
			var v = gradAVXVVector(hs, xs, ys, zs);

			return Add(gradAVXPartialA(hs, u), gradAVXPartialB(hs, v));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static float SumVector(Vector256<float> v)
			=> SumVectorInternal(v).GetElement(0);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<float> SumVectorInternal(Vector256<float> v)
		{
			var x = Permute2x128(v, v, 1);
			var y = Add(v, x);
			x = Shuffle(y, y, 177); //2 << 6 | 3 << 4 | 0 << 2 | 1;
			x = Add(x, y);
			y = Shuffle(x, x, 78); //1 << 6 | 0 << 4 | 3 << 2 | 2;
			return Add(x, y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static float OctavePerlinAVX(float x, float y, float z, int nOctaves = 8, float persistence = 0.5f, float lacunarity = 2.0f, float scale = 10.0f)
		{
			if (nOctaves % 8 != 0)
				throw new ArgumentException($"{nameof(nOctaves)} has to be divide able by 8!");
			
			var freq  = 1.0f;
			var amp   = 1.0f;
			var max   = 0.0f;
			var total = 0.0f;
			var i = 0;
			
			while(true) {
				var valueVector = perlinAVX(
				                            LoadVectorWithModScale(x, lacunarity, freq, scale),
				                            LoadVectorWithModScale(y, lacunarity, freq, scale),
				                            LoadVectorWithModScale(z, lacunarity, freq, scale)
				                           );

				var ampVector   = LoadVectorWithMod(amp, persistence);
				var totalVector = Multiply(ampVector, valueVector);

				max   += SumVector(ampVector);
				total += SumVector(totalVector);
				i     += 8;
				if (i >= nOctaves)
					break;
				amp  *= (float) Math.Pow(persistence, 8);
				freq *= (float) Math.Pow(lacunarity, 8);
			}
			
			return total / max;
		}
	}
}
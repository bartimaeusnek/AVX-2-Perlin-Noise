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
		private static readonly Vector256<int>   v255 = LoadVectorCorrectly(255);
		private static readonly Vector256<int>   v1   = LoadVectorCorrectly(1);
		private static readonly Vector256<int>   v2   = LoadVectorCorrectly(2);
		private static readonly Vector256<float> v2f  = LoadVectorCorrectly(2f);
		private static readonly Vector256<float> v1f  = LoadVectorCorrectly(1f);
		private static readonly Vector256<int>   v3   = LoadVectorCorrectly(3);
		private static readonly Vector256<int>   v4   = LoadVectorCorrectly(4);
		private static readonly Vector256<float> v4f  = LoadVectorCorrectly(4f);
		private static readonly Vector256<float> v6f  = LoadVectorCorrectly(6f);
		private static readonly Vector256<int>   v7   = LoadVectorCorrectly(7);
		private static readonly Vector256<int>   v8   = LoadVectorCorrectly(8);
		private static readonly Vector256<float> v10f = LoadVectorCorrectly(10f);
		private static readonly Vector256<int>   v11  = LoadVectorCorrectly(11);
		private static readonly Vector256<int>   v12  = LoadVectorCorrectly(12);
		private static readonly Vector256<int>   v13  = LoadVectorCorrectly(13);
		private static readonly Vector256<int>   v14  = LoadVectorCorrectly(14);
		private static readonly Vector256<float> v15f = LoadVectorCorrectly(15f);
		private static readonly Vector256<int>   v15  = LoadVectorCorrectly(15);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<int> MakeBitCutVector(Vector256<float> x)
			=> And(ConvertToVector256Int32WithTruncation(Floor(x)), v255);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal static Vector256<float> MakeFloatCutVector(Vector256<float> x)
			=> Subtract(x, ConvertToVector256Single(ConvertToVector256Int32WithTruncation(Floor(x))));
		
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
		public static unsafe float OctavePerlinAVXDynamic(float x,                  float y, float z, int nOctaves = 1,
		                                                  float persistence = 0.5f, float lacunarity = 2.0f,
		                                                  float scale       = 10.0f)
		{
			var freq      = stackalloc float[1]{1f};
			var amp       = stackalloc float[1]{1f};
			var i           = stackalloc int[1]{1};
			var max       = stackalloc float[1]{0.0f};
			var total     = stackalloc float[1]{0.0f};
			
			var octaveVectorPtr  = stackalloc float[8] {0,0,0,0,0,0,0,0};
			do
			{

				Vector256<float> octaveVector;
				if (nOctaves - (*i - 1) >= 8)
					octaveVector = LoadVectorCorrectly(1f);
				else
				{
					for (int j = 0; j < nOctaves - (*i - 1); j++)
					{
						*(octaveVectorPtr + j) = 1;
					}
					octaveVector = LoadVector256(octaveVectorPtr);
				}

				var valueVector = perlinAVX(
				                            LoadVectorWithModScale(x, lacunarity, *freq, scale),
				                            LoadVectorWithModScale(y, lacunarity, *freq, scale),
				                            LoadVectorWithModScale(z, lacunarity, *freq, scale)
				                           );

				var ampVector   = LoadVectorWithMod(*amp, persistence);
				var totalVector = Multiply(ampVector, valueVector);

				ampVector   = Multiply(octaveVector, ampVector);
				totalVector = Multiply(octaveVector, totalVector);

				*max   += SumVector(ampVector);
				*total += SumVector(totalVector);
				*i     += 8;
				
				if (*i > nOctaves)
					break;
				
				*amp  *= PseudoPow8(persistence);
				*freq *= PseudoPow8(lacunarity);
			} while (true);

			return *total / *max;
		}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe float OctavePerlinAVXDynamicBlend(float x,                  float y, float z, int nOctaves = 1,
		                                                  float persistence = 0.5f, float lacunarity = 2.0f,
		                                                  float scale       = 10.0f)
		{
			var freq      = stackalloc float[1]{1f};
			var amp       = stackalloc float[1]{1f};
			var i           = stackalloc int[1]{1};
			var max       = stackalloc float[1]{0.0f};
			var total     = stackalloc float[1]{0.0f};

			var octaveBitmask       = stackalloc byte[1] {0};
			var intermediateProduct  = stackalloc int[1] {0};
			//var octaveVectorPtr  = stackalloc float[8] {0,0,0,0,0,0,0,0};
			var vecZero             = Vector256<float>.Zero;
			do
			{

				//Vector256<float> octaveVector;
				//if (nOctaves >= 8)
				//	octaveVector = LoadVectorCorrectly(1f);
				//else
				//{
				//	for (int j = 0; j < (nOctaves-1); j++)
				//	{
				//		*(octaveVectorPtr + j) = 1;
				//	}
				//	octaveVector = LoadVector256(octaveVectorPtr);
				//}
				
				*intermediateProduct = nOctaves - (*i - 1);
				if (*intermediateProduct >= 8)
				{
					*octaveBitmask = 0xff;
				}
				else
				{
					*octaveBitmask = *intermediateProduct switch
					                 {
						                 7 => 0b0111_1111,
						                 6 => 0b0011_1111,
						                 5 => 0b0001_1111,
						                 4 => 0b0000_1111,
						                 3 => 0b0000_0111,
						                 2 => 0b0000_0011,
						                 _ => 0b0000_0001
					                 };
				}
				
				var valueVector = perlinAVX(
				                            LoadVectorWithModScale(x, lacunarity, *freq, scale),
				                            LoadVectorWithModScale(y, lacunarity, *freq, scale),
				                            LoadVectorWithModScale(z, lacunarity, *freq, scale)
				                           );

				var ampVector   = LoadVectorWithMod(*amp, persistence);
				var totalVector = Multiply(ampVector, valueVector);

				//ampVector   = Multiply(octaveVector, ampVector);
				//totalVector = Multiply(octaveVector, totalVector);
				
				ampVector   = Blend(vecZero, ampVector,   *octaveBitmask);
				totalVector = Blend(vecZero, totalVector, *octaveBitmask);
				
				*max   += SumVector(ampVector);
				*total += SumVector(totalVector);
				*i     += 8;
				
				if (*i > nOctaves)
					break;
				
				*amp  *= PseudoPow8(persistence);
				*freq *= PseudoPow8(lacunarity);
			} while (true);

			return *total / *max;
		}
		
		[Obsolete("use OctavePerlinAVXDynamic instead!"), MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe float OctavePerlinAVX(float x, float y, float z, int nOctaves = 8, float persistence = 0.5f, float lacunarity = 2.0f, float scale = 10.0f)
		{
			if (nOctaves % 8 != 0)
				throw new ArgumentException($"{nameof(nOctaves)} has to be divide able by 8!");

			var freq = stackalloc float[1]{1f};
			var amp = stackalloc float[1]{1f};
			var i = stackalloc int[1]{1};
			var max   = stackalloc float[1]{0.0f};
			var total = stackalloc float[1]{0.0f};
			
			do
			{
				var valueVector = perlinAVX(
				                            LoadVectorWithModScale(x, lacunarity, *freq, scale),
				                            LoadVectorWithModScale(y, lacunarity, *freq, scale),
				                            LoadVectorWithModScale(z, lacunarity, *freq, scale)
				                           );

				var ampVector   = LoadVectorWithMod(*amp, persistence);
				var totalVector = Multiply(ampVector, valueVector);

				*max   += SumVector(ampVector);
				*total += SumVector(totalVector);
				*i     += 8;
				if (*i > nOctaves)
					break;
				*amp  *= PseudoPow8(persistence);
				*freq *= PseudoPow8(lacunarity);
			} while (true);

			return *total / *max;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe float[] OctavePerlinAVX(Vector256<float> x, Vector256<float> y, Vector256<float> z, int nOctaves = 8, float persistence = 0.5f, float lacunarity = 2.0f, float scale = 10.0f)
		{
			var results = new float[8];
			fixed (float* ptr = &results[0])
			{
				Store(ptr, OctavePerlinAVXParallel(x, y, z,nOctaves,persistence,lacunarity,scale));
			}
			return results;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector256<float> OctavePerlinAVXParallel(Vector256<float> x,Vector256<float> y,Vector256<float> z, int nOctaves = 8, float persistence = 0.5f,float lacunarity = 2.0f,float scale = 10.0f)
		{
			var freq  = v1f;
			var amp   = v1f;
			var max   = Vector256<float>.Zero;
			var total = Vector256<float>.Zero;
			var persistenceV = LoadVectorCorrectly(persistence);
			var lacunarityV = LoadVectorCorrectly(lacunarity);
			var scaleV = LoadVectorCorrectly(scale);
			for (var i = 0; i < nOctaves; ++i)
			{

				var cX = Divide(Multiply(x, freq), scaleV);
				var cY = Divide(Multiply(y, freq), scaleV);
				var cZ = Divide(Multiply(z, freq), scaleV);
				var value = perlinAVX(cX,cY,cZ);
				
				total = Add(total,Multiply(amp,value));
				max   = Add(max,amp);
				freq  = Multiply(freq,lacunarityV);
				amp   = Multiply(amp,persistenceV);
			}

			return Divide(total, max);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static float PseudoPow8(float a) => a * a * a * a * a * a * a * a;

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static float PseudoPow(float a, int b)
		{
			for (int i = 0; i < b; i++)
			{
				a *= a;
			}

			return a;
		}
	}
}
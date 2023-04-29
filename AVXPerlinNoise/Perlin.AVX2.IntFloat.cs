using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Avx2;
using static System.Runtime.Intrinsics.X86.Avx;
using static AVXPerlinNoise.VectorUtils;

namespace AVXPerlinNoise;

public partial class Perlin
{
	private static readonly Vector256<int>   v255 = LoadVectorCorrectly(255);
	private static readonly Vector256<int>   v0   = Vector256<int>.Zero;
	private static readonly Vector256<float> v0f  = Vector256<float>.Zero;
	private static readonly Vector256<int>   v1   = LoadVectorCorrectly(1);
	private static readonly Vector256<int>   v2   = LoadVectorCorrectly(2);
	private static readonly Vector256<float> v2f  = LoadVectorCorrectly(2f);
	private static readonly Vector256<float> v1f  = LoadVectorCorrectly(1f);
	private static readonly Vector256<int>   v4   = LoadVectorCorrectly(4);
	private static readonly Vector256<float> v6f  = LoadVectorCorrectly(6f);
	private static readonly Vector256<int>   v8   = LoadVectorCorrectly(8);
	private static readonly Vector256<float> v10f = LoadVectorCorrectly(10f);
	private static readonly Vector256<int>   v12  = LoadVectorCorrectly(12);
	private static readonly Vector256<int>   v14  = LoadVectorCorrectly(14);
	private static readonly Vector256<float> v15f = LoadVectorCorrectly(15f);
	private static readonly Vector256<int>   v15  = LoadVectorCorrectly(15);

	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Vector256<int> MakeBitCutVector(Vector256<float> x)
	{
		return And(ConvertToVector256Int32WithTruncation(Floor(x)), v255);
	}

	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Vector256<float> MakeFloatCutVector(Vector256<float> x)
	{
		return Subtract(x, ConvertToVector256Single(ConvertToVector256Int32WithTruncation(Floor(x))));
	}

	[SkipLocalsInit]
	public Vector256<float> perlinAVX(Vector256<float> x, Vector256<float> y, Vector256<float> z)
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
		
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Vector256<float> lerpAVX(Vector256<float> a, Vector256<float> b, Vector256<float> x)
	{
		return Add(a, Multiply(x, Subtract(b, a)));
	}

	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Vector256<float> fadeAVX(Vector256<float> t)
	{
		return Multiply(
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
	}

	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Vector256<int> UnpackPermutationArrayAndAdd(Vector256<int> xi, Vector256<int> yi)
	{
		return Add(UnpackPermutationArray(xi), yi);
	}

	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe Vector256<int> UnpackPermutationArray(Vector256<int> xi)
	{
		return GatherVector256(p, xi, sizeof(int));
	}

	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Vector256<float> gradAVXUVector(Vector256<int> hashs, Vector256<float> xs, Vector256<float> ys)
	{
		var h   = CompareGreaterThan(v8, hashs);

		var usX = And(h.AsSingle(), xs);
		var usY = AndNot(h.AsSingle(), ys);

		return Add(usX, usY);
	}

	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Vector256<float> gradAVXPartialA(Vector256<int> hashs, Vector256<float> u)
	{
		var hAnd1  = And(hashs, v1);
		var hZeros = CompareEqual(hAnd1, v0);
		var hOnes  = CompareGreaterThan(hAnd1, v0);

		var ucomp    = And(hZeros.AsSingle(), u);
		var ucompneg = Subtract(v0f, And(hOnes.AsSingle(), u));
		return Add(ucomp, ucompneg);
	}
		
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Vector256<float> gradAVXPartialB(Vector256<int> hashs, Vector256<float> u)
	{
		var hAnd1  = And(hashs, v2);
		var hZeros = CompareEqual(hAnd1, v0);
		var hOnes  = CompareGreaterThan(hAnd1, v0);

		var ucomp    = And(hZeros.AsSingle(), u);
		var ucompneg = Subtract(v0f, And(hOnes.AsSingle(), u));
		return Add(ucomp, ucompneg);
	}
		
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Vector256<float> gradAVX(Vector256<int>   hashs, Vector256<float> xs, Vector256<float> ys,
											 Vector256<float> zs)
	{
		var hs  = And(hashs, v15);

		var u = gradAVXUVector(hs, xs, ys);
		var v = gradAVXVVector(hs, xs, ys, zs);

		return Add(gradAVXPartialA(hs, u), gradAVXPartialB(hs, v));
	}

	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static float SumVector(Vector256<float> v)
	{
		return SumVectorInternal(v).GetElement(0);
	}

	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Vector256<float> SumVectorInternal(Vector256<float> v)
	{
		var x = Permute2x128(v, v, 1);
		var y = Add(v, x);
		x = Shuffle(y, y, 177); //2 << 6 | 3 << 4 | 0 << 2 | 1;
		x = Add(x, y);
		y = Shuffle(x, x, 78); //1 << 6 | 0 << 4 | 3 << 2 | 2;
		return Add(x, y);
	}

	[SkipLocalsInit]
	public unsafe float OctavePerlinAVXDynamic(float x, float y, float z, int nOctaves = 1,
											   float persistence = 0.5f, float lacunarity = 2.0f,
											   float scale = 10.0f)
	{
		float freq = 1f;
		float amp = 1f;
		float max = 0.0f;
		float total = 0.0f;
		int i = 1;
		Unsafe.InitBlock(_octaveVectorPtr, 0, 8 * sizeof(float));
		var octaveVector = v1f;
		int octLoad;
		do
		{
			octLoad = nOctaves - (i - 1);
			if (octLoad < 8)
			{
				for (int j = 0; j < octLoad; j++)
				{
					*(_octaveVectorPtr + j) = 1f;
				}
				octaveVector = LoadAlignedVector256(_octaveVectorPtr);
			}

			var valueVector = perlinAVX(
				LoadVectorWithModScale(x, lacunarity, freq, scale),
				LoadVectorWithModScale(y, lacunarity, freq, scale),
				LoadVectorWithModScale(z, lacunarity, freq, scale)
			);

			var ampVector = LoadVectorWithMod(amp, persistence);
			var totalVector = Multiply(ampVector, valueVector);

			ampVector = Multiply(octaveVector, ampVector);
			totalVector = Multiply(octaveVector, totalVector);

			max += SumVector(ampVector);
			total += SumVector(totalVector);
			i += 8;

			if (i > nOctaves)
			{
				break;
			}

			amp *= PseudoPow8(persistence);
			freq *= PseudoPow8(lacunarity);
		} while (true);

		return total / max;
	}

	[SkipLocalsInit]
	public unsafe void OctavePerlinAVX(Vector256<float> x, Vector256<float> y, Vector256<float> z, Span<float> results, int nOctaves = 8, float persistence = 0.5f, float lacunarity = 2.0f, float scale = 10.0f)
	{
		if (results.Length < 8)
		{
			throw new ArgumentException($"{nameof(results)} needs to hold at least 8 numbers, but holds only {results.Length}!");
		}

		fixed (float* ptr = &results.GetPinnableReference())
		{
			Store(ptr, OctavePerlinAVXParallel(x, y, z, nOctaves, persistence, lacunarity, scale));
		}
	}
		
	[SkipLocalsInit]
	public unsafe void OctavePerlinAVX(float[] x, float[] y, float[] z, float[] results, int nOctaves = 8, float persistence = 0.5f, float lacunarity = 2.0f, float scale = 10.0f)
	{
		if (results.Length < 8)
		{
			throw new ArgumentException($"{nameof(results)} needs to hold at least 8 numbers!");
		}

		fixed (float* ptr = &results[0])
		{
			Store(ptr, OctavePerlinAVXParallel(Create(x), Create(y), Create(z), nOctaves, persistence, lacunarity, scale));
		}
	}

	[SkipLocalsInit]
	public unsafe float[] OctavePerlinAVX(Vector256<float> x, Vector256<float> y, Vector256<float> z, int nOctaves = 8, float persistence = 0.5f, float lacunarity = 2.0f, float scale = 10.0f)
	{
		var results = new float[8];
		fixed (float* ptr = &results[0])
		{
			Store(ptr, OctavePerlinAVXParallel(x, y, z,nOctaves,persistence,lacunarity,scale));
		}
		return results;
	}
		
	[SkipLocalsInit]
	public Vector256<float> OctavePerlinAVXParallel(Vector256<float> x,Vector256<float> y,Vector256<float> z, int nOctaves = 8, float persistence = 0.5f,float lacunarity = 2.0f,float scale = 10.0f)
	{
		var freq  = v1f;
		var amp   = v1f;
		var max   = v0f;
		var total = v0f;
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
		
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float PseudoPow8(float a)
	{
		return a * a * a * a * a * a * a * a;
	}

	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe Vector256<float> LoadVectorWithMod(float count, float mod)
	{
		var countV = LoadVectorCorrectly(count);
		_tobe[0] = 1;
		_tobe[1] = mod;
		_tobe[2] = mod * mod;
		_tobe[3] = mod * mod * mod;
		_tobe[4] = mod * mod * mod * mod;
		_tobe[5] = mod * mod * mod * mod * mod;
		_tobe[6] = mod * mod * mod * mod * mod * mod;
		_tobe[7] = mod * mod * mod * mod * mod * mod * mod;
		return Multiply(LoadAlignedVector256(_tobe), countV);
	}
	
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector256<float> LoadVectorWithModScale(float count, float mod, float initial , float scale)
	{
		return Divide(Multiply(LoadVectorWithMod(count, mod), LoadVectorCorrectly(initial)), LoadVectorCorrectly(scale));
	}

}
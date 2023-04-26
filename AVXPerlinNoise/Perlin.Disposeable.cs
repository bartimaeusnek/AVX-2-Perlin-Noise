namespace AVXPerlinNoise;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading;

public unsafe partial class Perlin : IDisposable
{
	internal int*   p;
	internal int*   pL;
	private  float* _octaveVectorPtr;
	private  float* _tobe;

	[ExcludeFromCodeCoverage]
	public Perlin()
	{
		_tobe = (float*)NativeMemory.AlignedAlloc(16 * sizeof(float), 64);
		_octaveVectorPtr = _tobe + 8;
		Unsafe.InitBlock(_tobe, 0, 16 * sizeof(float));
		p = (int*)NativeMemory.AlignedAlloc(512 * sizeof(int), 64);
		pL = (int*)NativeMemory.AlignedAlloc(512 * sizeof(long), 64);
		for (var x = 0; x < 512; x++)
		{
			pL[x] = p[x] = permutation[x % 256];
		}
	}

	[ExcludeFromCodeCoverage]
	public float OptimizedOctavePerlin(float x, float y, float z, int nOctaves = 1,
									   float persistence = 0.5f, float lacunarity = 2.0f,
									   float scale = 10.0f)
	{
		if (!Avx2.IsSupported || nOctaves < 6)
		{
			return OctavePerlin(x, y, z, nOctaves, persistence, lacunarity, scale);
		}

		return OctavePerlinAVXDynamic(x, y, z, nOctaves, persistence, lacunarity, scale);
	}

	private ulong _wasDisposed = 0UL;
	
	[ExcludeFromCodeCoverage]
	protected virtual void Dispose(bool disposing)
	{
		if (Interlocked.Read(ref _wasDisposed) != 0)
			return;
		Interlocked.Increment(ref _wasDisposed);
		if (disposing)
		{
			NativeMemory.AlignedFree(p);
			NativeMemory.AlignedFree(pL);
			NativeMemory.AlignedFree(_tobe);
		}
	}
	
	[ExcludeFromCodeCoverage]
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
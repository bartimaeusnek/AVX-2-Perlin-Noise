namespace AVXPerlinNoise;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading;

public unsafe partial class Perlin : IDisposable
{
	private ulong _wasDisposed = 0UL;
	
	[ExcludeFromCodeCoverage]
	protected virtual void Dispose(bool disposing)
	{
		if (Interlocked.Read(ref _wasDisposed) != 0)
		{
			return;
		}
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
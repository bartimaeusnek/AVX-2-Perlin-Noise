namespace PerlinTests;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using AVXPerlinNoise;
using BenchmarkDotNet.Attributes;

[ExcludeFromCodeCoverage]
public abstract class BaseBenchmark
{
    protected Perlin _perlin;
    protected int[]  _hashs;

    protected float[]  _xs;
    protected float[]  _ys;
    protected float[]  _zs;
    
    protected float[] _as;
    protected float[] _bs;
    protected float[] _cs;
    
    protected Vector256<int>   hashsV;
    protected Vector256<float> yV;
    protected Vector256<float> xV;
    protected Vector256<float> zV;
        
    protected Vector256<float> aV;
    protected Vector256<float> bV;
    protected Vector256<float> cV;
    
    protected int[]   retGrad = new int[8];
    protected float[] retlerp = new float[8];

    [GlobalCleanup]
    public void Dispose()
    {
        _perlin.Dispose();
    }
    
    [GlobalSetup]
    public void Init()
    {
        _perlin = new Perlin();
        _hashs = new[]
        {
            12, 123, 66, 8, 1, 1200, 9, 3
        };
        _xs = new[]
        {
            1f, 2f, 2f, 5f, 1f, 6f, 77f, 6f
        };
        _ys = new[]
        {
            2f, 5f, 6f, 7f, 1f, 88f, 5f, 4f
        };
        _zs = new[]
        {
            3f, 7f, 4f, 3f, 1f, 6f, 5f, 5f
        };

        hashsV = VectorUtils.Create(_hashs);
        yV     = VectorUtils.Create(_ys);
        xV     = VectorUtils.Create(_xs);
        zV     = VectorUtils.Create(_zs);

        _as = new []
        {
            2f, 3f, 5f, 7f, 2f, 3f, 5f, 7f
        };

        _bs = new []
        {
            3f, 5f, 7f, 2f, 3f, 5f, 7f, 2f
        };
        _cs = new []
        {
            5f, 7f, 2f, 3f, 5f, 7f, 2f, 3f
        };
        aV = VectorUtils.Create(_as);
        bV = VectorUtils.Create(_bs);
        cV = VectorUtils.Create(_cs);
    }
}
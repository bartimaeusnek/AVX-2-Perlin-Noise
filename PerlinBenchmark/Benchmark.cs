using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using AVXPerlinNoise;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace PerlinTests;

[ExcludeFromCodeCoverage]
[MarkdownExporter]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net60, baseline: true)]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net70)]
public class Grad : BaseBenchmark
{
    [Benchmark]
    public Vector256<float> AVX2()
    {
        var result = Perlin.gradAVX(hashsV, xV, yV, zV);
        return result;
    }

    [Benchmark(Baseline = true)]
    public int[] Regular()
    {
        for (int i = 0; i < 8; i++)
        {
            retGrad[i] = (int) Perlin.grad(_hashs[i], _xs[i], _ys[i], _zs[i]);
        }

        return retGrad;
    }
}

[ExcludeFromCodeCoverage]
[MarkdownExporter]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net60, baseline: true)]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net70)]
public class Lerp : BaseBenchmark
{
    [Benchmark]
    public Vector256<float> AVX2()
    {
        var result = Perlin.lerpAVX(aV, bV, cV);
        return result;
    }

    [Benchmark(Baseline = true)]
    public float[] Regular()
    {
        for (int i = 0; i < 4; i++)
        {
            retlerp[i] = Perlin.lerp(_as[i], _bs[i], _cs[i]);
        }

        return retlerp;
    }
}

[ExcludeFromCodeCoverage]
[MarkdownExporter]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net60, baseline: true)]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net70)]
public class Fade : BaseBenchmark
{
    [Benchmark]
    public Vector256<float> AVX2()
    {
        var result = Perlin.fadeAVX(aV);
        return result;
    }

    [Benchmark(Baseline = true)]
    public float[] Regular()
    {
        for (int i = 0; i < 4; i++)
        {
            retlerp[i] = Perlin.fade(_as[i]);
        }

        return retlerp;
    }
}

[ExcludeFromCodeCoverage]
[MarkdownExporter]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net60, baseline: true)]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net70)]
public class PerlinBench : BaseBenchmark
{
    
    [Benchmark]
    public Vector256<float> AVX2()
    {
        var result = _perlin.perlinAVX(xV, yV, zV);
        return result;
    }

    [Benchmark(Baseline = true)]
    public float[] Regular()
    {
        for (int i = 0; i < 8; i++)
        {
            retlerp[i] = _perlin.perlin(_xs[i], _ys[i], _zs[i]);
        }

        return retlerp;
    }
}

[ExcludeFromCodeCoverage]
[MarkdownExporter]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net60, baseline: true)]
[SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net70)]
public class PerlinOctaveBench : BaseBenchmark
{
    [Params(1, 8)]
    public int nOctaves;
        
    [Benchmark]
    public float AVX2Dynamic()
    {
        float results;
        results = _perlin.OctavePerlinAVXDynamic(_xs[0], _ys[0], _zs[0], nOctaves);
        return results;
    }

    [Benchmark(Baseline = true)]
    public float Regular()
    {
        float results;
        results = _perlin.OctavePerlin(_xs[0], _ys[0], _zs[0], nOctaves);
        return results;
    }
}

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        // BenchmarkRunner.Run<Grad>();
        // BenchmarkRunner.Run<Lerp>();
        // BenchmarkRunner.Run<Fade>();
        // BenchmarkRunner.Run<PerlinBench>();
        BenchmarkRunner.Run<PerlinOctaveBench>();
    }
}
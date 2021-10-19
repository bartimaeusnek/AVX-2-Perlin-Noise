using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using AVXPerlinNoise;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace PerlinTests
{
    [ExcludeFromCodeCoverage,MarkdownExporterAttribute]
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
                this.retGrad[i] = (int) Perlin.grad(this._hashs[i], this._xs[i], this._ys[i], this._zs[i]);
            }

            return this.retGrad;
        }
    }

    [ExcludeFromCodeCoverage,MarkdownExporterAttribute]
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
                this.retlerp[i] = Perlin.lerp(this._as[i], this._bs[i], this._cs[i]);
            }

            return this.retlerp;
        }
    }

    [ExcludeFromCodeCoverage,MarkdownExporterAttribute]
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
                this.retlerp[i] = Perlin.fade(this._as[i]);
            }

            return this.retlerp;
        }
    }

    [ExcludeFromCodeCoverage,MarkdownExporterAttribute]
    public class PerlinBench : BaseBenchmark
    {
        [Benchmark]
        public Vector256<float> AVX2()
        {
            var result = Perlin.perlinAVX(this.xV, this.yV, this.zV);
            return result;
        }

        [Benchmark(Baseline = true)]
        public float[] Regular()
        {
            for (int i = 0; i < 8; i++)
            {
                this.retlerp[i] = Perlin.perlin(this._xs[i], this._ys[i], this._zs[i]);
            }

            return this.retlerp;
        }
    }

    [
        ExcludeFromCodeCoverage,
        MarkdownExporterAttribute,
        SimpleJob(RunStrategy.Throughput, RuntimeMoniker.NetCoreApp31, baseline: true), 
        //SimpleJob(RunStrategy.Throughput, RuntimeMoniker.NetCoreApp50)
    ]
    public class PerlinOctaveBench : BaseBenchmark
    {
        [Params(1, 2, 3, 4, 5, 6, 7, 8)]
        public int nOctaves;
        
        [Benchmark]
        public float AVX2Dynamic()
        {
            float results;
            results = Perlin.OctavePerlinAVXDynamic(this._xs[0], this._ys[0], this._zs[0], nOctaves: nOctaves);
            return results;
        }

        [Benchmark(Baseline = true)]
        public float Regular()
        {
            float results;
            results = Perlin.OctavePerlin(this._xs[0], this._ys[0], this._zs[0], nOctaves);
            return results;
        }
    }
    
    [
        ExcludeFromCodeCoverage,
        MarkdownExporterAttribute,
        SimpleJob(RunStrategy.Throughput, RuntimeMoniker.NetCoreApp31, baseline: true), 
        //SimpleJob(RunStrategy.Throughput, RuntimeMoniker.NetCoreApp50)
    ]
    public class PerlinOctaveBenchDoublePrecision : BaseBenchmark
    {
        [Params( 1, 2, 3, 4, 5, 6, 7, 8)]
        public int nOctaves;
        
        [Benchmark]
        public double AVX2DynamicDoublePrecision()
        {
            double results;
            results = Perlin.OctavePerlinAVXDynamic(this._xsd[0], this._ysd[0], this._zsd[0], nOctaves);
            return results;
        }
        
        [Benchmark]
        public double AVX2DynamicDoublePrecisionBlend()
        {
            double results;
            results = Perlin.OctavePerlinAVXDynamicBlend(this._xsd[0], this._ysd[0], this._zsd[0], nOctaves);
            return results;
        }

        [Benchmark(Baseline = true)]
        public double RegularDoublePrecision()
        {
            double results;
            results = Perlin.OctavePerlin(this._xsd[0], this._ysd[0], this._zsd[0], nOctaves);
            return results;
        }
    }
    
    public class PerlinOctaveBenchSse : BaseBenchmark
    {
        [Params(1, 2, 3, 4, 5, 6, 7, 8)]
        public int nOctaves;
        
        [Benchmark]
        public float Sse()
        {
            float results;
            results = Perlin.OctavePerlinSseDynamic(this._as[0], this._bs[0], this._cs[0], nOctaves);
            return results;
        }
        
        [Benchmark]
        public float SseBlend()
        {
            float results;
            results = Perlin.OctavePerlinSseDynamicBlend(this._as[0], this._bs[0], this._cs[0], nOctaves);
            return results;
        }
        
        
        [Benchmark(Baseline = true)]
        public float RegularDoublePrecision()
        {
            float results;
            results = Perlin.OctavePerlin(this._as[0], this._bs[0], this._cs[0], nOctaves);
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
            // BenchmarkRunner.Run<PerlinOctaveBenchDoublePrecision>();
        }
    }
}
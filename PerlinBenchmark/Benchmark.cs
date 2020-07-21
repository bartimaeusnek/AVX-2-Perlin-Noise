using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using AVXPerlinNoise;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
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

    [ExcludeFromCodeCoverage,MarkdownExporterAttribute,SimpleJob(RunStrategy.Throughput)]
    public class PerlinOctaveBench : BaseBenchmark
    {
        [Benchmark]
        public float[] AVX2Parallel()
        {
            var results = Perlin.OctavePerlinAVX(this.xV, this.yV, this.zV);
            return results;
        }
        
        [Benchmark]
        public float[] AVX2()
        {
            float[] results = new float[8];
            for (int i = 0; i < 8; i++)
            {
                results[i] = Perlin.OctavePerlinAVX(this._xs[0], this._ys[0], this._zs[0]);
            }
            return results;
        }

        [Benchmark(Baseline = true)]
        public float[] Regular()
        {
            float[] results = new float[8];
            for (int i = 0; i < 8; i++)
            {
                results[i] = Perlin.OctavePerlin(this._xs[0], this._ys[0], this._zs[0]);
            }
            return results;
        }
    }
    
    [ExcludeFromCodeCoverage,MarkdownExporterAttribute,SimpleJob(RunStrategy.Throughput)]
    public class PerlinOctaveBenchx32 : BaseBenchmark
    {
        [Benchmark]
        public float AVX2()
        {
            var result = Perlin.OctavePerlinAVX(this._xs[0], this._ys[0], this._zs[0], 32);
            return result;
        }

        [Benchmark(Baseline = true)]
        public float Regular()
        {
            var result = Perlin.OctavePerlin(this._xs[0], this._ys[0], this._zs[0], 32);
            return result;
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
            //BenchmarkRunner.Run<PerlinOctaveBenchx32>();
        }
    }
}
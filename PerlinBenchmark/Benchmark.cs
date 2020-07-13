using System.Linq;
using System.Runtime.Intrinsics;
using AVXPerlinNoise;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace PerlinTests
{
    [RPlotExporter, MarkdownExporterAttribute]
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
    
    [RPlotExporter, MarkdownExporterAttribute]
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
                this.retlerp[i] = Perlin.lerp( this._as[i], this._bs[i], this._cs[i]);
            }

            return this.retlerp;
        }
    }
    
    [RPlotExporter, MarkdownExporterAttribute]
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
                this.retlerp[i] = Perlin.fade( this._as[i]);
            }

            return this.retlerp;
        }
    }
    
    [RPlotExporter, MarkdownExporterAttribute]
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
                this.retlerp[i] = Perlin.perlin(this._xs[i], this._ys[i],this._zs[i]);
            }

            return this.retlerp;
        }
    }
    
    [RPlotExporter, MarkdownExporterAttribute]
    public class PerlinOctaveBench : BaseBenchmark
    {
        [Benchmark]
        public float AVX2()
        {
            var result = Perlin.OctavePerlinAVX(this._xs[0], this._ys[0], this._zs[0]);
            return result;
        }

        [Benchmark(Baseline = true)]
        public float Regular()
        {
            var result = Perlin.OctavePerlin(this._xs[0], this._ys[0], this._zs[0]);
            return result;
        }
    }

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
}
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Intrinsics;
using AVXPerlinNoise;
using BenchmarkDotNet.Attributes;

namespace PerlinTests
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseBenchmark
    {
        protected int[]  _hashs;
        protected int[]  _hashsAnd15;
        protected float[]  _xs;
        protected float[]  _ys;
        protected float[]  _zs;
        
        protected double[] _xsd;
        protected double[] _ysd;
        protected double[] _zsd;
        
        protected float[] _as;
        protected float[] _bs;
        protected float[] _cs;
        
        protected double[] _asd;
        protected double[] _bsd;
        protected double[] _csd;
        
        protected Vector256<int> hashsV;
        protected Vector256<float> yV;
        protected Vector256<float> xV;
        protected Vector256<float> zV;
        
        protected Vector256<float> aV;
        protected Vector256<float> bV;
        protected Vector256<float> cV;
        
        protected Vector256<double> aVd;
        protected Vector256<double> bVd;
        protected Vector256<double> cVd;
        
        protected int[]          retGrad = new int[8];
        protected float[] retlerp = new float[8];

        [GlobalSetup]
        public void init()
        {
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
            
            _xsd = new[]
                  {
                      1d, 2d, 2d, 5d, 1d, 6d, 77d, 6d
                  };
            _ysd = new[]
                  {
                      2d, 5d, 6d, 7d, 1d, 88d, 5d, 4d
                  };
            _zsd = new[]
                  {
                      3d, 7d, 4d, 3d, 1d, 6d, 5d, 5d
                  };
            _hashsAnd15 = this._hashs.Select(x => x & 15).ToArray();

            hashsV = VectorUtils.Create(this._hashs);
            yV     = VectorUtils.Create(this._ys);
            xV     = VectorUtils.Create(this._xs);
            zV     = VectorUtils.Create(this._zs);

            this._as = new []
                       {
                           2f, 3f, 5f, 7f, 2f, 3f, 5f, 7f
                       };

            this._bs = new []
                       {
                           3f, 5f, 7f, 2f, 3f, 5f, 7f, 2f
                       };
            this._cs = new []
                       {
                           5f, 7f, 2f, 3f, 5f, 7f, 2f, 3f
                       };
            aV = VectorUtils.Create(this._as);
            bV = VectorUtils.Create(this._bs);
            cV = VectorUtils.Create(this._cs);

            this._asd = new []
                       {
                           2d, 3d, 5d, 7d
                       };

            this._bsd = new []
                       {
                           3d, 5d, 7d, 2d
                       };
            this._csd = new []
                       {
                           5d, 7d, 2d, 3d
                       };
            
            aVd = VectorUtils.Create(this._asd);
            bVd = VectorUtils.Create(this._bsd);
            cVd = VectorUtils.Create(this._csd);
            
            Perlin.init();
        }
    }
}
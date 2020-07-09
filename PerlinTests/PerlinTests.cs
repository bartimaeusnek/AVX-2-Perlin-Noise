using System;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using AVXPerlinNoise;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PerlinTests
{
    [TestClass]
    public class PerlinTests
    {
        private int[] _hashs;
        private int[] _hashsAnd15;
        private int[] _xs;
        private int[] _ys;
        private int[] _zs;
        
        private int[] _hashsL;
        private int[] _hashsAnd15L;
        private float[] _xsD;
        private float[] _ysD;
        private float[] _zsD;
        
        [TestInitialize]
        public void init()
        {
            _hashs = new[]
                     {
                         12, 123, 66, 8, 1, 1200, 9, 3
                     };
            _xs = new[]
                  {
                      1,  2,   2,  5, 1, 6,    77, 6
                  };
            _ys = new[]
                  {
                      2,  5,   6,  7, 1, 88,   5,  4
                  };
            _zs = new[]
                  {
                      3,  7,   4,  3, 1, 6,    5,  5
                  };
            _hashsAnd15 = this._hashs.Select(x => x & 15).ToArray();

            _hashsL = this._hashs.Select(x => (int) x).ToArray();
            _hashsAnd15L = this._hashsAnd15.Select(x => (int) x).ToArray();
            _xsD = new[]
                  {
                      1.2f,  2.6f,   2f,  5.5f, 1.8f, 6.4f,    77f, 6.3655f
                  };
            _ysD = this._ys.Select(x => (float) x).ToArray();
            _zsD = this._zs.Select(x => (float) x).ToArray();
        }

        [TestMethod]
        public void TestFade()
        {
            var xV = Vector256.Create(2d, 3d, 5d, 7d);
            var result = Perlin.fadeAVX(xV);
            Assert.AreEqual(Perlin.fade(2), result.GetElement(0));
            Assert.AreEqual(Perlin.fade(3), result.GetElement(1));
            Assert.AreEqual(Perlin.fade(5), result.GetElement(2));
            Assert.AreEqual(Perlin.fade(7), result.GetElement(3));
        }
        
        [TestMethod]
        public void TestLerp()
        {
            var xV = Vector256.Create(2d, 3d, 5d, 7d);
            var yV = Vector256.Create(3d, 5d, 7d, 2d);
            var zV = Vector256.Create(5d, 7d, 2d, 3d);
            var result = Perlin.lerpAVX( xV, yV, zV);
            Assert.AreEqual(Perlin.lerp(2, 3, 5), result.GetElement(0));
            Assert.AreEqual(Perlin.lerp(3, 5, 7), result.GetElement(1));
            Assert.AreEqual(Perlin.lerp(5, 7, 2), result.GetElement(2));
            Assert.AreEqual(Perlin.lerp(7, 2, 3), result.GetElement(3));
        }
        
        
        [TestMethod]
        public void TestGradV()
        {
            var hashsV = VectorUtils.Create(this._hashsAnd15L);
            var yV     = VectorUtils.Create(this._ysD);
            var xV = VectorUtils.Create(this._xsD);
            var zV = VectorUtils.Create(this._zsD);
            var result = Perlin.gradAVXVVector(hashsV, xV,yV,zV);
            for (var i = 0; i < this._hashsAnd15L.Length; i++)
            {
                Assert.AreEqual(Perlin.gradV(this._hashsAnd15[i], this._xsD[i],this._ysD[i],this._zsD[i]), result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestGradVY()
        {
            var hashsV = VectorUtils.Create(this._hashsAnd15L);
            var yV = VectorUtils.Create(this._ysD);
            var result = Perlin.gradAVXVYVector(hashsV, yV);
            for (var i = 0; i < this._hashsAnd15L.Length; i++)
            {
                Assert.AreEqual((int) Perlin.gradVY(this._hashsAnd15[i], this._ys[i]), result.GetElement(i));
            }
        }
        
        [TestMethod]
        public void TestGradVX()
        {
            var hashsV = VectorUtils.Create(this._hashsAnd15L);
            var xV     = VectorUtils.Create(this._xsD);
            var result = Perlin.gradAVXVXVector(hashsV, xV);
            for (var i = 0; i < this._hashsAnd15L.Length; i++)
            {
                Assert.AreEqual(Perlin.gradVX(this._hashsAnd15[i], this._xsD[i]), result.GetElement(i));
            }
        }
        
        [TestMethod]
        public void TestGradVZ()
        {
            var hashsV = VectorUtils.Create(this._hashsAnd15L);
            var zV     = VectorUtils.Create(this._zsD);
            var result = Perlin.gradAVXVZVector(hashsV, zV);
            for (var i = 0; i < this._hashsAnd15L.Length; i++)
            {
                Assert.AreEqual( Perlin.gradVZ(this._hashsAnd15[i], this._zsD[i]), result.GetElement(i));
            }
        }
        
        [TestMethod]
        public void TestGradU()
        {
            var hashsV = VectorUtils.Create(this._hashsL);
            var xV = VectorUtils.Create(this._xsD);
            var yV = VectorUtils.Create(this._ysD);

            var result = Perlin.gradAVXUVector(hashsV, xV, yV);
            for (var i = 0; i < this._hashsL.Length; i++)
            {
                Assert.AreEqual(Perlin.gradu(this._hashs[i], this._xsD[i], this._ysD[i]), result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestGrad()
        {
            var hashsV = VectorUtils.Create(this._hashsL);
            var yV     = VectorUtils.Create(this._ysD);
            var xV     = VectorUtils.Create(this._xsD);
            var zV     = VectorUtils.Create(this._zsD);
            var result = Perlin.gradAVX(hashsV, xV, yV, zV);
            for (var i = 0; i < this._hashsL.Length; i++)
            {
                Assert.AreEqual(Perlin.grad(this._hashs[i], this._xsD[i], this._ysD[i], this._zsD[i]), result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestPerlin()
        {
            var yV = VectorUtils.Create(this._ysD);
            var xV = VectorUtils.Create(this._xsD);
            var zV = VectorUtils.Create(this._zsD);
            var result = Perlin.perlinAVX(xV, yV, zV);
            for (var i = 0; i < this._hashsL.Length; i++)
            {
                var prl = Perlin.perlin(this._xsD[i], this._ysD[i], this._zsD[i]);
                Assert.AreEqual(prl, result.GetElement(i));
            }
        }

    }
}
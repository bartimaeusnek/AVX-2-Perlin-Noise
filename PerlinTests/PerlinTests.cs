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
            
            _xsD = new[]
                  {
                      1.2f,  2.6f,   2f,  5.5f, 1.8f, 6.4f,    77f, 6.3655f
                  };
            
            _ysD = new[]
                 {
                     0.4F,  1.0F,   1.2F,  1.4F, 0.2F, 17.6F,   1.0F,  0.8F
                 };
            _zsD = new[]
                   {
                       0.6F,  1.4F,   0.8F,  0.6F, 0.4F, 1.2F,    1F,  1F
                   };
            
            _hashsAnd15 = this._hashs.Select(x => x & 15).ToArray();
            _hashsL = this._hashs.Select(x => x).ToArray();
            _hashsAnd15L = this._hashsAnd15.Select(x => x).ToArray();
            
            
        }

        [TestMethod]
        public void TestFade()
        {
            var xV = VectorUtils.Create(this._xsD);
            var result = Perlin.fadeAVX(xV);
            Assert.AreEqual(Perlin.fade(2), result.GetElement(0));
            Assert.AreEqual(Perlin.fade(3), result.GetElement(1));
            Assert.AreEqual(Perlin.fade(5), result.GetElement(2));
            Assert.AreEqual(Perlin.fade(7), result.GetElement(3));
        }
        
        [TestMethod]
        public void TestLerp()
        {
            var xV = VectorUtils.Create(this._xsD);
            var yV = VectorUtils.Create(this._ysD);
            var zV = VectorUtils.Create(this._zsD);
            var result = Perlin.lerpAVX( xV, yV, zV);
            Console.WriteLine(result);
            for (int i = 0; i < 8; i++)
            {
                var lerp = Perlin.lerp(this._xsD[i], this._ysD[i], this._zsD[i]);
                Console.WriteLine(lerp);
                Assert.AreEqual(lerp, result.GetElement(i));
            }
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
                Assert.AreEqual(Perlin.gradVY(this._hashsAnd15[i], this._ysD[i]), result.GetElement(i));
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
        public void TestBitCut()
        {
            var xV = VectorUtils.Create(this._ysD);
            var result = Perlin.MakeBitCutVector(xV);
            for (var i = 0; i < this._hashsL.Length; i++)
            {
                Assert.AreEqual((int) this._ysD[i] % 255, result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestFloatCut()
        {
            var xV     = VectorUtils.Create(this._ysD);
            var result = Perlin.MakeFloatCutVector(xV);
            for (var i = 0; i < this._hashsL.Length; i++)
            {
                Assert.AreEqual(this._ysD[i] - (int) this._ysD[i], result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestHash1()
        {
            var x     = VectorUtils.Create(this._xsD);
            var y     = VectorUtils.Create(this._ysD);
            var z = VectorUtils.Create(this._zsD);
            
            var xi = Perlin.MakeBitCutVector(x);
            var yi = Perlin.MakeBitCutVector(y);
            var zi = Perlin.MakeBitCutVector(z);

            var xf = Perlin.MakeFloatCutVector(x);
            var yf = Perlin.MakeFloatCutVector(y);
            var zf = Perlin.MakeFloatCutVector(z);

            var u = Perlin.fadeAVX(xf);
            var v = Perlin.fadeAVX(yf);
            var w = Perlin.fadeAVX(zf);

            var a  = Perlin.UnpackPermutationArrayAndAdd(xi,          yi);
            var aa = Perlin.UnpackPermutationArrayAndAdd(a,           zi);
            var ab = Perlin.UnpackPermutationArrayAndAdd(Avx2.Add(a, VectorUtils.LoadVectorCorrectly(1)), zi);
            var b  = Perlin.UnpackPermutationArrayAndAdd(Avx2.Add(xi, VectorUtils.LoadVectorCorrectly(1)), yi);
            var ba = Perlin.UnpackPermutationArrayAndAdd(b,           zi);
            var bb = Perlin.UnpackPermutationArrayAndAdd(Avx2.Add(b, VectorUtils.LoadVectorCorrectly(1)),  zi);

            var x1AGrad = Perlin.gradAVX(Perlin.UnpackPermutationArray(aa), xf, yf, zf);
            var x1BGrad = Perlin.gradAVX(Perlin.UnpackPermutationArray(ba), Avx2.Subtract(xf, VectorUtils.LoadVectorCorrectly(1f)), yf, zf);

            var x2AGrad = Perlin.gradAVX(Perlin.UnpackPermutationArray(ab), xf, Avx2.Subtract(yf, VectorUtils.LoadVectorCorrectly(1f)), zf);
            var x2BGrad = Perlin.gradAVX(Perlin.UnpackPermutationArray(bb), Avx2.Subtract(xf, VectorUtils.LoadVectorCorrectly(1f)), Avx2.Subtract(yf,VectorUtils.LoadVectorCorrectly(1f)), zf);

            var x1 = Perlin.lerpAVX(x1AGrad, x1BGrad,u);
            var x2 = Perlin.lerpAVX(x2AGrad, x2BGrad, u);
            var y1 = Perlin.lerpAVX(x1, x2, v);
           
            var x1BAGrad = Perlin.gradAVX(Perlin.UnpackPermutationArray(Avx2.Add(aa, VectorUtils.LoadVectorCorrectly(1))), xf, yf, Avx2.Subtract(zf, VectorUtils.LoadVectorCorrectly(1f)));
            var x1BBGrad = Perlin.gradAVX(Perlin.UnpackPermutationArray(Avx2.Add(ba, VectorUtils.LoadVectorCorrectly(1))), Avx2.Subtract(xf, VectorUtils.LoadVectorCorrectly(1f)), yf, Avx2.Subtract(zf, VectorUtils.LoadVectorCorrectly(1f)));
            var x2BAGrad = Perlin.gradAVX(Perlin.UnpackPermutationArray(Avx2.Add(ab, VectorUtils.LoadVectorCorrectly(1))), xf, Avx2.Subtract(yf, VectorUtils.LoadVectorCorrectly(1f)), Avx2.Subtract(zf, VectorUtils.LoadVectorCorrectly(1f)));
            var x2BBGrad = Perlin.gradAVX(Perlin.UnpackPermutationArray(Avx2.Add(bb, VectorUtils.LoadVectorCorrectly(1))),
                                          Avx2.Subtract(xf, VectorUtils.LoadVectorCorrectly(1f)), Avx2.Subtract(yf, VectorUtils.LoadVectorCorrectly(1f)),
                                          Avx2.Subtract(zf, VectorUtils.LoadVectorCorrectly(1f)));

            
            var x1b = Perlin.lerpAVX(x1BAGrad, x1BBGrad, u);

            var x2b =
                Perlin.lerpAVX(x2BAGrad,
                               x2BBGrad,
                               u);


            var y2 = Perlin.lerpAVX(x1b, x2b, v);
            
            for (var i = 0; i < 8; i++)
            {
                var    xiSingle = (int) this._xsD[i] & 255; 
                var    yiSingle = (int) this._ysD[i] & 255; 
                var    ziSingle = (int) this._zsD[i] & 255; 
                float    xfSingle = this._xsD[i] - (int) this._xsD[i];   
                float    yfSingle = this._ysD[i] - (int) this._ysD[i];
                float    zfSingle = this._zsD[i] - (int) this._zsD[i];
                float    uSingle  = Perlin.fade(xfSingle);
                float    vSingle  = Perlin.fade(yfSingle);
                float    wSingle  = Perlin.fade(zfSingle);
                var    aSingle  = Perlin.p[xiSingle]     + yiSingle; 
                var    aaSingle = Perlin.p[aSingle]      + ziSingle; 
                var    abSingle = Perlin.p[aSingle + 1] + ziSingle; 
                var    bSingle  = Perlin.p[xiSingle + 1] + yiSingle; 
                var    baSingle = Perlin.p[bSingle]      + ziSingle; 
                var    bbSingle = Perlin.p[bSingle + 1]  + ziSingle; 
			
                float x1Single, x2Single, y1Single, x1aGrad, x1bGrad, x2aGrad, x2bGrad, y2Single;

                x1aGrad = Perlin.grad(Perlin.p[aaSingle], xfSingle, yfSingle, zfSingle);
                x1bGrad = Perlin.grad(Perlin.p[baSingle], xfSingle - 1f, yfSingle, zfSingle);
                x1Single = Perlin.lerp(x1aGrad, x1bGrad, uSingle);
                x2aGrad = Perlin.grad(Perlin.p[abSingle], xfSingle, yfSingle - 1f, zfSingle);
                x2bGrad = Perlin.grad(Perlin.p[bbSingle], xfSingle - 1f, yfSingle - 1f, zfSingle);
                x2Single = Perlin.lerp(x2aGrad, x2bGrad, uSingle); 
                y1Single = Perlin.lerp(x1Single, x2Single, vSingle);
                
                Assert.AreEqual(x1aGrad, x1AGrad.GetElement(i));
                Assert.AreEqual(x1bGrad, x1BGrad.GetElement(i));
                Assert.AreEqual(uSingle, u.GetElement(i));
                Assert.AreEqual(x2aGrad, x2AGrad.GetElement(i));
                Assert.AreEqual(x2bGrad, x2BGrad.GetElement(i));
                Assert.AreEqual(x1Single, x1.GetElement(i));
                Assert.AreEqual(x2Single, x2.GetElement(i));
                Assert.AreEqual(y1Single, y1.GetElement(i));
                
                x1aGrad = Perlin.grad(Perlin.p[aaSingle + 1], xfSingle, yfSingle, zfSingle - 1);
                x1bGrad = Perlin.grad(Perlin.p[baSingle + 1], xfSingle - 1, yfSingle, zfSingle - 1);
                x1Single = Perlin.lerp(x1aGrad, x1bGrad, uSingle);
                
                Assert.AreEqual(x1aGrad, x1BAGrad.GetElement(i));
                Assert.AreEqual(x1bGrad, x1BBGrad.GetElement(i));
                
                x2Single = Perlin.lerp(Perlin.grad(Perlin.p[abSingle + 1], xfSingle, yfSingle - 1, zfSingle - 1),
                                       Perlin.grad(Perlin.p[bbSingle + 1], xfSingle - 1, yfSingle - 1, zfSingle - 1),
                                       uSingle);
                y2Single = Perlin.lerp(x1Single, x2Single, vSingle);
                
                Assert.AreEqual(x1Single, x1b.GetElement(i));
                Assert.AreEqual(x2Single, x2b.GetElement(i));
                Assert.AreEqual(y2Single, y2.GetElement(i));
            }
            
        }
        
        [TestMethod]
        public void TestUnpack()
        {
            var xV = VectorUtils.Create(this._xsD);
            var yV = VectorUtils.Create(this._ysD);
            var xiV = Perlin.MakeBitCutVector(xV);
            var yiV = Perlin.MakeBitCutVector(yV);
            var result = Perlin.UnpackPermutationArrayAndAdd(xiV, yiV);
            for (var i = 0; i < 8; i++)
            {
                var xi = (int) this._xsD[i] & 255;
                var yi = (int)  this._ysD[i] & 255;
                var a  = Perlin.p[xi] + yi;
                Assert.AreEqual(a, result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestPerlin()
        {
            var yV = VectorUtils.Create(this._ysD);
            var xV = VectorUtils.Create(this._xsD);
            var zV = VectorUtils.Create(this._zsD);
            var result = Perlin.perlinAVX(xV, yV, zV);
            Console.WriteLine(result);
            for (var i = 0; i < this._hashsL.Length; i++)
            {
                var prl = Perlin.perlin(this._xsD[i], this._ysD[i], this._zsD[i]);
                Console.WriteLine(prl);
                Assert.AreEqual(prl, result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestRiggedNoise()
        {
            var yV     = this._ysD[1];
            var xV     = this._xsD[1];
            var zV     = this._zsD[1];
            var result = Perlin.OctavePerlinAVX(xV, yV, zV);
            var prl = Perlin.OctavePerlin(xV, yV, zV);
            Assert.AreEqual(prl, result);
        }
    }
}
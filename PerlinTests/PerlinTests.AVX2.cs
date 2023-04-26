using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using AVXPerlinNoise;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PerlinTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PerlinTests
    {
        private int[] _hashs;
        private int[] _hashsAnd15;

        private int[]   _hashsL;
        private int[]   _hashsAnd15L;
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
                       1.2f, 2.6f, 2f, 5.5f, 1.8f, 6.4f, 77f, 6.3655f
                   };

            _ysD = new[]
                   {
                       0.4F, 1.0F, 1.2F, 1.4F, 0.2F, 17.6F, 1.0F, 0.8F
                   };
            _zsD = new[]
                   {
                       0.6F, 1.4F, 0.8F, 0.6F, 0.4F, 1.2F, 1F, 1F
                   };
            _hashsAnd15  = _hashs.Select(x => x & 15).ToArray();
            _hashsL      = _hashs.Select(x => x).ToArray();
            _hashsAnd15L = _hashsAnd15.Select(x => x).ToArray();
        }

        [TestMethod]
        public void TestFade()
        {
            var xV     = VectorUtils.Create(_xsD);
            var result = Perlin.fadeAVX(xV);
            for (var i = 0; i < 8; i++)
            {
                Assert.AreEqual(Perlin.fade(_xsD[i]), result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestLerp()
        {
            var xV     = VectorUtils.Create(_xsD);
            var yV     = VectorUtils.Create(_ysD);
            var zV     = VectorUtils.Create(_zsD);
            var result = Perlin.lerpAVX(xV, yV, zV);
            for (int i = 0; i < 8; i++)
            {
                var lerp = Perlin.lerp(_xsD[i], _ysD[i], _zsD[i]);
                Assert.AreEqual(lerp, result.GetElement(i));
            }
        }


        [TestMethod]
        public void TestGradV()
        {
            var hashsV = VectorUtils.Create(_hashsAnd15L);
            var yV     = VectorUtils.Create(_ysD);
            var xV     = VectorUtils.Create(_xsD);
            var zV     = VectorUtils.Create(_zsD);
            var result = Perlin.gradAVXVVector(hashsV, xV, yV, zV);
            for (var i = 0; i < _hashsAnd15L.Length; i++)
            {
                Assert.AreEqual(Perlin.gradV(_hashsAnd15[i], _xsD[i], _ysD[i], _zsD[i]),
                                result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestGradU()
        {
            var hashsV = VectorUtils.Create(_hashsL);
            var xV     = VectorUtils.Create(_xsD);
            var yV     = VectorUtils.Create(_ysD);

            var result = Perlin.gradAVXUVector(hashsV, xV, yV);
            for (var i = 0; i < _hashsL.Length; i++)
            {
                Assert.AreEqual(Perlin.gradu(_hashs[i], _xsD[i], _ysD[i]), result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestGrad()
        {
            var hashsV = VectorUtils.Create(_hashsL);
            var yV     = VectorUtils.Create(_ysD);
            var xV     = VectorUtils.Create(_xsD);
            var zV     = VectorUtils.Create(_zsD);
            var result = Perlin.gradAVX(hashsV, xV, yV, zV);
            for (var i = 0; i < _hashsL.Length; i++)
            {
                Assert.AreEqual(Perlin.grad(_hashs[i], _xsD[i], _ysD[i], _zsD[i]),
                                result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestBitCut()
        {
            var xV     = VectorUtils.Create(_ysD);
            var result = Perlin.MakeBitCutVector(xV);
            for (var i = 0; i < _hashsL.Length; i++)
            {
                Assert.AreEqual((int) _ysD[i] % 255, result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestFloatCut()
        {
            var xV     = VectorUtils.Create(_ysD);
            var result = Perlin.MakeFloatCutVector(xV);
            for (var i = 0; i < _hashsL.Length; i++)
            {
                Assert.AreEqual(_ysD[i] - (int) _ysD[i], result.GetElement(i));
            }
        }

        [TestMethod]
        public unsafe void TestHash1()
        {
            using var perlin = new Perlin();
            var x = VectorUtils.Create(_xsD);
            var y = VectorUtils.Create(_ysD);
            var z = VectorUtils.Create(_zsD);

            var xi = Perlin.MakeBitCutVector(x);
            var yi = Perlin.MakeBitCutVector(y);
            var zi = Perlin.MakeBitCutVector(z);

            var xf = Perlin.MakeFloatCutVector(x);
            var yf = Perlin.MakeFloatCutVector(y);
            var zf = Perlin.MakeFloatCutVector(z);

            var u = Perlin.fadeAVX(xf);
            var v = Perlin.fadeAVX(yf);
            var w = Perlin.fadeAVX(zf);

            var a  = perlin.UnpackPermutationArrayAndAdd(xi,                                               yi);
            var aa = perlin.UnpackPermutationArrayAndAdd(a,                                                zi);
            var ab = perlin.UnpackPermutationArrayAndAdd(Avx2.Add(a,  VectorUtils.LoadVectorCorrectly(1)), zi);
            var b  = perlin.UnpackPermutationArrayAndAdd(Avx2.Add(xi, VectorUtils.LoadVectorCorrectly(1)), yi);
            var ba = perlin.UnpackPermutationArrayAndAdd(b,                                                zi);
            var bb = perlin.UnpackPermutationArrayAndAdd(Avx2.Add(b, VectorUtils.LoadVectorCorrectly(1)),  zi);

            var x1AGrad = Perlin.gradAVX(perlin.UnpackPermutationArray(aa), xf, yf, zf);
            var x1BGrad = Perlin.gradAVX(perlin.UnpackPermutationArray(ba),
                                         Avx.Subtract(xf, VectorUtils.LoadVectorCorrectly(1f)), yf, zf);

            var x2AGrad = Perlin.gradAVX(perlin.UnpackPermutationArray(ab), xf,
                                         Avx.Subtract(yf, VectorUtils.LoadVectorCorrectly(1f)), zf);
            var x2BGrad = Perlin.gradAVX(perlin.UnpackPermutationArray(bb),
                                         Avx.Subtract(xf, VectorUtils.LoadVectorCorrectly(1f)),
                                         Avx.Subtract(yf, VectorUtils.LoadVectorCorrectly(1f)), zf);

            var x1 = Perlin.lerpAVX(x1AGrad, x1BGrad, u);
            var x2 = Perlin.lerpAVX(x2AGrad, x2BGrad, u);
            var y1 = Perlin.lerpAVX(x1,      x2,      v);

            var x1BAGrad =
                Perlin.gradAVX(perlin.UnpackPermutationArray(Avx2.Add(aa, VectorUtils.LoadVectorCorrectly(1))), xf, yf,
                               Avx.Subtract(zf, VectorUtils.LoadVectorCorrectly(1f)));
            var x1BBGrad =
                Perlin.gradAVX(perlin.UnpackPermutationArray(Avx2.Add(ba, VectorUtils.LoadVectorCorrectly(1))),
                               Avx.Subtract(xf, VectorUtils.LoadVectorCorrectly(1f)), yf,
                               Avx.Subtract(zf, VectorUtils.LoadVectorCorrectly(1f)));
            var x2BAGrad =
                Perlin.gradAVX(perlin.UnpackPermutationArray(Avx2.Add(ab, VectorUtils.LoadVectorCorrectly(1))), xf,
                               Avx.Subtract(yf, VectorUtils.LoadVectorCorrectly(1f)),
                               Avx.Subtract(zf, VectorUtils.LoadVectorCorrectly(1f)));
            var x2BBGrad =
                Perlin.gradAVX(perlin.UnpackPermutationArray(Avx2.Add(bb, VectorUtils.LoadVectorCorrectly(1))),
                               Avx.Subtract(xf, VectorUtils.LoadVectorCorrectly(1f)),
                               Avx.Subtract(yf, VectorUtils.LoadVectorCorrectly(1f)),
                               Avx.Subtract(zf, VectorUtils.LoadVectorCorrectly(1f)));


            var x1b = Perlin.lerpAVX(x1BAGrad, x1BBGrad, u);

            var x2b =
                Perlin.lerpAVX(x2BAGrad,
                               x2BBGrad,
                               u);


            var y2 = Perlin.lerpAVX(x1b, x2b, v);

            for (var i = 0; i < 8; i++)
            {
                var   xiSingle = (int) _xsD[i] & 255;
                var   yiSingle = (int) _ysD[i] & 255;
                var   ziSingle = (int) _zsD[i] & 255;
                float xfSingle = _xsD[i] - (int) _xsD[i];
                float yfSingle = _ysD[i] - (int) _ysD[i];
                float zfSingle = _zsD[i] - (int) _zsD[i];
                float uSingle  = Perlin.fade(xfSingle);
                float vSingle  = Perlin.fade(yfSingle);
                float wSingle  = Perlin.fade(zfSingle);
                var   aSingle  = perlin.p[xiSingle]     + yiSingle;
                var   aaSingle = perlin.p[aSingle]      + ziSingle;
                var   abSingle = perlin.p[aSingle  + 1] + ziSingle;
                var   bSingle  = perlin.p[xiSingle + 1] + yiSingle;
                var   baSingle = perlin.p[bSingle]      + ziSingle;
                var   bbSingle = perlin.p[bSingle + 1]  + ziSingle;

                float x1Single, x2Single, y1Single, x1aGrad, x1bGrad, x2aGrad, x2bGrad, y2Single;

                x1aGrad  = Perlin.grad(perlin.p[aaSingle], xfSingle,      yfSingle, zfSingle);
                x1bGrad  = Perlin.grad(perlin.p[baSingle], xfSingle - 1f, yfSingle, zfSingle);
                x1Single = Perlin.lerp(x1aGrad, x1bGrad, uSingle);
                x2aGrad  = Perlin.grad(perlin.p[abSingle], xfSingle, yfSingle - 1f, zfSingle);
                x2bGrad  = Perlin.grad(perlin.p[bbSingle], xfSingle           - 1f, yfSingle - 1f, zfSingle);
                x2Single = Perlin.lerp(x2aGrad,  x2bGrad,  uSingle);
                y1Single = Perlin.lerp(x1Single, x2Single, vSingle);

                Assert.AreEqual(x1aGrad,  x1AGrad.GetElement(i));
                Assert.AreEqual(x1bGrad,  x1BGrad.GetElement(i));
                Assert.AreEqual(uSingle,  u.GetElement(i));
                Assert.AreEqual(x2aGrad,  x2AGrad.GetElement(i));
                Assert.AreEqual(x2bGrad,  x2BGrad.GetElement(i));
                Assert.AreEqual(x1Single, x1.GetElement(i));
                Assert.AreEqual(x2Single, x2.GetElement(i));
                Assert.AreEqual(y1Single, y1.GetElement(i));

                x1aGrad  = Perlin.grad(perlin.p[aaSingle + 1], xfSingle, yfSingle, zfSingle - 1);
                x1bGrad  = Perlin.grad(perlin.p[baSingle + 1], xfSingle - 1, yfSingle, zfSingle - 1);
                x1Single = Perlin.lerp(x1aGrad, x1bGrad, uSingle);

                Assert.AreEqual(x1aGrad, x1BAGrad.GetElement(i));
                Assert.AreEqual(x1bGrad, x1BBGrad.GetElement(i));

                x2Single = Perlin.lerp(Perlin.grad(perlin.p[abSingle + 1], xfSingle, yfSingle - 1, zfSingle - 1),
                                       Perlin.grad(perlin.p[bbSingle + 1], xfSingle - 1, yfSingle - 1, zfSingle - 1),
                                       uSingle);
                y2Single = Perlin.lerp(x1Single, x2Single, vSingle);

                Assert.AreEqual(x1Single, x1b.GetElement(i));
                Assert.AreEqual(x2Single, x2b.GetElement(i));
                Assert.AreEqual(y2Single, y2.GetElement(i));
            }

        }

        [TestMethod]
        public unsafe void TestUnpack()
        {
            using var perlin = new Perlin();
            var xV     = VectorUtils.Create(_xsD);
            var yV     = VectorUtils.Create(_ysD);
            var xiV    = Perlin.MakeBitCutVector(xV);
            var yiV    = Perlin.MakeBitCutVector(yV);
            var result = perlin.UnpackPermutationArrayAndAdd(xiV, yiV);
            for (var i = 0; i < 8; i++)
            {
                var xi = (int) _xsD[i] & 255;
                var yi = (int) _ysD[i] & 255;
                var a  = perlin.p[xi] + yi;
                Assert.AreEqual(a, result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestPerlin()
        {
            using var perlin = new Perlin();
            var yV     = VectorUtils.Create(_ysD);
            var xV     = VectorUtils.Create(_xsD);
            var zV     = VectorUtils.Create(_zsD);
            var result = perlin.perlinAVX(xV, yV, zV);
            for (var i = 0; i < 8; i++)
            {
                var prl = perlin.perlin(_xsD[i], _ysD[i], _zsD[i]);
                Assert.AreEqual(prl, result.GetElement(i));
            }
        }

        [TestMethod]
        public void TestOctaves()
        {
            using var perlin = new Perlin();
            var yV     = _ysD[1];
            var xV     = _xsD[1];
            var zV     = _zsD[1];
            var result = perlin.OctavePerlinAVXDynamic(xV, yV, zV, 8);
            var prl    = perlin.OctavePerlin(xV, yV, zV, 8);
            Assert.AreEqual(prl, result);

            //MutationTests
            result = perlin.OctavePerlinAVXDynamic(xV, yV, zV, 64);
            prl    = perlin.OctavePerlin(xV, yV, zV, 64);
            Assert.AreEqual(prl, result);

            result = perlin.OctavePerlinAVXDynamic(xV, yV, zV, 16);
            prl    = perlin.OctavePerlin(xV, yV, zV, 16);
            Assert.AreEqual(prl, result);

            result = perlin.OctavePerlinAVXDynamic(xV, yV, zV, 128);
            prl    = perlin.OctavePerlin(xV, yV, zV, 128);
            Assert.AreEqual(prl, result);
        }

        [TestMethod]
        public void TestOctavesParallel()
        {
            using var perlin = new Perlin();
            var yV             = VectorUtils.Create(_ysD);
            var xV             = VectorUtils.Create(_xsD);
            var zV             = VectorUtils.Create(_zsD);
            var result         = perlin.OctavePerlinAVX(xV, yV, zV);
            var resultParallel = perlin.OctavePerlinAVXParallel(xV, yV, zV);
            for (int i = 0; i < 8; i++)
            {
                var prl = perlin.OctavePerlin(_xsD[i], _ysD[i], _zsD[i]);
                Assert.AreEqual(prl, result[i]);
                Assert.AreEqual(prl, resultParallel.GetElement(i));
            }
        }

        [TestMethod]
        public void TestOctavesDynamic()
        {
            using var perlin = new Perlin();
            for (float xV = -10f; xV < 10f; xV += 0.1f)
                for (float yV = -10f; yV < 10f; yV += 0.1f)
                    for (float zV = -10f; zV < 10f; zV += 0.1f)
                        for (int octaves = 1; octaves < 9; octaves++)
                        {
                            var result = perlin.OctavePerlinAVXDynamic(xV, yV, zV, octaves);
                            var prl    = perlin.OctavePerlin(xV, yV, zV, octaves);
                            Assert.AreEqual(prl, result, 0.000001,
                                            $"xV = {xV}, yV = {yV}, zV = {zV}, Octaves = {octaves}");
                        }
        }

        [TestMethod]
        public void TestPerlinBound()
        {
            using var perlin = new Perlin();
            for (double i = -10; i < 10; i += 0.01D)
            {
                var prl = perlin.perlin(i, i, i);
                Assert.IsFalse(prl > 1,  $"{i} - " + prl);
                Assert.IsFalse(prl < -1, $"{i} - " + prl);
            }
        }

    }
}
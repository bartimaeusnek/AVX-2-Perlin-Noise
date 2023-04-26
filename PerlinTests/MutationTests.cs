namespace PerlinTests;

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using AVXPerlinNoise;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class MutationTests
{
    [TestMethod]
    public void TestPseudoPow8()
    {
        for (int i = 0; i < 10; i++)
        {
            Assert.AreEqual((float) Math.Pow(i, 8), Perlin.PseudoPow8(i));
        }
    }
    
    [TestMethod]
    public unsafe void LoadModVector()
    {
        const int count = 2;
        const int mod = 4;
        using var perlin = new Perlin();
        var vec = perlin.LoadVectorWithMod(count, mod);

        float* ptr = stackalloc float[8];
        Avx.Store(ptr, vec);
        
        Assert.AreEqual(count, ptr[0]);
        for (int i = 1; i < 8; i++)
        {
            float exp = count;
            for (int j = 0; j < i; j++)
            {
                exp *= mod;
            }
            Assert.AreEqual(exp, ptr[i], 0.1f);
        }
    }

    [TestMethod]
    public void SumVector()
    {
        Assert.AreEqual(36f,Perlin.SumVector(Vector256.Create(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f)), 0.1f);
    }

    [TestMethod]
    public void VectorUtilsCreate()
    {
        Assert.ThrowsException<ArgumentException>(() => VectorUtils.Create(Array.Empty<float>()));
        Assert.ThrowsException<ArgumentException>(() => VectorUtils.CreateVec128(Array.Empty<float>()));
        Assert.ThrowsException<ArgumentException>(() => VectorUtils.Create(Array.Empty<float>().AsSpan()));
        Assert.ThrowsException<ArgumentException>(() => VectorUtils.Create(Array.Empty<int>()));
        Assert.ThrowsException<ArgumentException>(() => VectorUtils.CreateVec128(Array.Empty<int>()));

        var eightnumbers = new Action[]
        {
            () => VectorUtils.Create(Array.Empty<float>()),
            () => VectorUtils.Create(Array.Empty<float>().AsSpan()),
            () => VectorUtils.Create(Array.Empty<int>())
        };
        foreach (var action in eightnumbers)
        {
            try
            {
                action();
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("a needs to hold 8 numbers!", e.Message);
            }
        }
        
        var fournumbers = new Action[]
        {
            () => VectorUtils.CreateVec128(Array.Empty<float>()),
            () => VectorUtils.CreateVec128(Array.Empty<int>())
        };

        foreach (var action in fournumbers)
        {
            try
            {
                action();
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("a needs to hold 4 numbers!", e.Message);
            }
        }

        Assert.AreEqual(Vector256.Create(1,1,1,1,1,1,1,1),VectorUtils.Create(new []{1,1,1,1,1,1,1,1}));
        Assert.AreEqual(Vector256.Create(1f,1f,1f,1f,1f,1f,1f,1f), VectorUtils.Create(new []{1f,1f,1f,1f,1f,1f,1f,1f}));
        Assert.AreEqual(Vector256.Create(1f,1f,1f,1f,1f,1f,1f,1f), VectorUtils.Create(new []{1f,1f,1f,1f,1f,1f,1f,1f}.AsSpan()));
        Assert.AreEqual(Vector128.Create(1,1,1,1),VectorUtils.CreateVec128(new []{1,1,1,1}));
        Assert.AreEqual(Vector128.Create(1f,1f,1f,1f),VectorUtils.CreateVec128(new []{1f,1f,1f,1f}));

        Assert.AreEqual(Vector256.Create(1,1,1,1,1,1,1,1), VectorUtils.LoadVectorCorrectly(1));
        Assert.AreEqual(Vector256.Create(1f,1f,1f,1f,1f,1f,1f,1f), VectorUtils.LoadVectorCorrectly(1f));
        Assert.AreEqual(Vector128.Create(1,1,1,1), VectorUtils.LoadVector128Correctly(1));
        Assert.AreEqual(Vector128.Create(1f,1f,1f,1f), VectorUtils.LoadVector128Correctly(1f));
    }
}
namespace AVXPerlinNoise;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

public unsafe partial class Perlin
{

    internal int*   p;
    internal int*   pL;
    private  float* _octaveVectorPtr;
    private  float* _tobe;

    [ExcludeFromCodeCoverage]
    public Perlin()
    {
        _tobe = (float*)NativeMemory.AlignedAlloc(16 * sizeof(float), 64);
        _octaveVectorPtr = _tobe + 8;
        Unsafe.InitBlock(_tobe, 0, 16 * sizeof(float));
        p = (int*)NativeMemory.AlignedAlloc(512 * sizeof(int), 64);
        pL = (int*)NativeMemory.AlignedAlloc(512 * sizeof(long), 64);
        for (var x = 0; x < 512; x++)
        {
            pL[x] = p[x] = permutation[x % 256];
        }

        if (Avx2.IsSupported)
        {
            _cheked = OctavePerlinAVXDynamic;
        }
        else
        {
            _cheked = OctavePerlin;
        }
        
    }
    
    private delegate float OctavePerlinWithAVXCheck(float x,
                                                    float y,
                                                    float z, 
                                                    int nOctaves,
                                                    float persistence,
                                                    float lacunarity,
                                                    float scale);

    private OctavePerlinWithAVXCheck _cheked;
    
    
    [ExcludeFromCodeCoverage]
    public float OptimizedOctavePerlin(float x, float y, float z, int nOctaves = 1,
                                       float persistence = 0.5f, float lacunarity = 2.0f,
                                       float scale = 10.0f)
    {
        return nOctaves < 6 
            ? OctavePerlin(x, y, z, nOctaves, persistence, lacunarity, scale) 
            : _cheked(x, y, z, nOctaves, persistence, lacunarity, scale);

    }
}
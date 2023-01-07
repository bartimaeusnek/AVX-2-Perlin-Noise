# AVX-2-Perlin-Noise
An AVX-2 improved implementation of Ken Perlin's Noise.

## Test Coverage:
100% of my Code is Covered.

## Build Status:
![.NET Core](https://github.com/bartimaeusnek/AVX-2-Perlin-Noise/workflows/.NET%20Core/badge.svg)

## Benchmark Status:
```ini
BenchmarkDotNet=v0.13.3, OS=Windows 10 (10.0.19044.2364/21H2/November2021Update)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.101
[Host]     : .NET 5.0.17 (5.0.1722.21314), X64 RyuJIT AVX2
Job-AVOMOZ : .NET 5.0.17 (5.0.1722.21314), X64 RyuJIT AVX2
Job-CUJAKV : .NET 6.0.12 (6.0.1222.56807), X64 RyuJIT AVX2
Job-CQLYLX : .NET 7.0.1 (7.0.122.56804), X64 RyuJIT AVX2
Job-LMCHZD : .NET Core 3.1.32 (CoreCLR 4.700.22.55902, CoreFX 4.700.22.56512), X64 RyuJIT AVX2

RunStrategy=Throughput
```

|      Method |       Runtime | nOctaves |        Mean |     Error |    StdDev | Ratio | RatioSD |
|------------ |-------------- |--------- |------------:|----------:|----------:|------:|--------:|
| **AVX2Dynamic** |      **.NET 5.0** |        **1** |   **137.99 ns** |  **1.140 ns** |  **0.890 ns** |  **3.74** |    **0.06** |
|     Regular |      .NET 5.0 |        1 |    32.75 ns |  0.370 ns |  0.328 ns |  0.89 |    0.01 |
| AVX2Dynamic |      .NET 6.0 |        1 |   169.02 ns |  2.911 ns |  2.723 ns |  4.60 |    0.07 |
|     Regular |      .NET 6.0 |        1 |    30.36 ns |  0.361 ns |  0.337 ns |  0.82 |    0.02 |
| AVX2Dynamic |      .NET 7.0 |        1 |   176.05 ns |  3.079 ns |  2.880 ns |  4.78 |    0.07 |
|     Regular |      .NET 7.0 |        1 |    27.33 ns |  0.325 ns |  0.304 ns |  0.74 |    0.01 |
| AVX2Dynamic | .NET Core 3.1 |        1 |   177.71 ns |  1.767 ns |  1.567 ns |  4.82 |    0.06 |
|     Regular | .NET Core 3.1 |        1 |    36.84 ns |  0.478 ns |  0.424 ns |  1.00 |    0.00 |
|             |               |          |             |           |           |       |         |
| **AVX2Dynamic** |      **.NET 5.0** |        **2** |   **141.16 ns** |  **2.170 ns** |  **2.030 ns** |  **1.96** |    **0.03** |
|     Regular |      .NET 5.0 |        2 |    59.42 ns |  0.402 ns |  0.356 ns |  0.82 |    0.02 |
| AVX2Dynamic |      .NET 6.0 |        2 |   171.06 ns |  3.141 ns |  2.938 ns |  2.37 |    0.06 |
|     Regular |      .NET 6.0 |        2 |    57.59 ns |  0.406 ns |  0.379 ns |  0.80 |    0.02 |
| AVX2Dynamic |      .NET 7.0 |        2 |   164.41 ns |  2.559 ns |  2.269 ns |  2.28 |    0.05 |
|     Regular |      .NET 7.0 |        2 |    50.21 ns |  0.748 ns |  0.700 ns |  0.70 |    0.01 |
| AVX2Dynamic | .NET Core 3.1 |        2 |   177.14 ns |  2.754 ns |  2.576 ns |  2.46 |    0.05 |
|     Regular | .NET Core 3.1 |        2 |    72.09 ns |  1.415 ns |  1.324 ns |  1.00 |    0.00 |
|             |               |          |             |           |           |       |         |
| **AVX2Dynamic** |      **.NET 5.0** |        **4** |   **156.50 ns** |  **2.758 ns** |  **2.580 ns** |  **1.08** |    **0.02** |
|     Regular |      .NET 5.0 |        4 |   117.83 ns |  0.490 ns |  0.409 ns |  0.81 |    0.01 |
| AVX2Dynamic |      .NET 6.0 |        4 |   157.24 ns |  2.901 ns |  2.714 ns |  1.08 |    0.02 |
|     Regular |      .NET 6.0 |        4 |   112.18 ns |  1.976 ns |  1.848 ns |  0.77 |    0.02 |
| AVX2Dynamic |      .NET 7.0 |        4 |   166.37 ns |  3.070 ns |  2.871 ns |  1.14 |    0.02 |
|     Regular |      .NET 7.0 |        4 |    99.38 ns |  1.073 ns |  0.951 ns |  0.68 |    0.01 |
| AVX2Dynamic | .NET Core 3.1 |        4 |   180.88 ns |  3.463 ns |  3.239 ns |  1.24 |    0.03 |
|     Regular | .NET Core 3.1 |        4 |   145.51 ns |  1.765 ns |  1.565 ns |  1.00 |    0.00 |
|             |               |          |             |           |           |       |         |
| **AVX2Dynamic** |      **.NET 5.0** |        **8** |   **139.78 ns** |  **2.224 ns** |  **1.971 ns** |  **0.49** |    **0.01** |
|     Regular |      .NET 5.0 |        8 |   228.05 ns |  2.494 ns |  2.333 ns |  0.80 |    0.02 |
| AVX2Dynamic |      .NET 6.0 |        8 |   166.44 ns |  1.268 ns |  1.186 ns |  0.59 |    0.01 |
|     Regular |      .NET 6.0 |        8 |   222.23 ns |  3.401 ns |  2.840 ns |  0.78 |    0.02 |
| AVX2Dynamic |      .NET 7.0 |        8 |   170.56 ns |  1.801 ns |  1.684 ns |  0.60 |    0.01 |
|     Regular |      .NET 7.0 |        8 |   190.47 ns |  0.908 ns |  0.805 ns |  0.67 |    0.01 |
| AVX2Dynamic | .NET Core 3.1 |        8 |   177.10 ns |  3.034 ns |  2.838 ns |  0.62 |    0.02 |
|     Regular | .NET Core 3.1 |        8 |   284.27 ns |  4.841 ns |  4.528 ns |  1.00 |    0.00 |
|             |               |          |             |           |           |       |         |
| **AVX2Dynamic** |      **.NET 5.0** |       **16** |   **296.70 ns** |  **5.739 ns** |  **5.368 ns** |  **0.53** |    **0.01** |
|     Regular |      .NET 5.0 |       16 |   460.10 ns |  5.712 ns |  5.343 ns |  0.82 |    0.01 |
| AVX2Dynamic |      .NET 6.0 |       16 |   323.27 ns |  4.598 ns |  4.301 ns |  0.58 |    0.01 |
|     Regular |      .NET 6.0 |       16 |   447.01 ns |  6.227 ns |  5.520 ns |  0.80 |    0.01 |
| AVX2Dynamic |      .NET 7.0 |       16 |   318.39 ns |  2.882 ns |  2.555 ns |  0.57 |    0.01 |
|     Regular |      .NET 7.0 |       16 |   383.77 ns |  5.054 ns |  4.480 ns |  0.68 |    0.01 |
| AVX2Dynamic | .NET Core 3.1 |       16 |   315.43 ns |  5.670 ns |  5.304 ns |  0.56 |    0.01 |
|     Regular | .NET Core 3.1 |       16 |   560.30 ns |  5.389 ns |  5.041 ns |  1.00 |    0.00 |
|             |               |          |             |           |           |       |         |
| **AVX2Dynamic** |      **.NET 5.0** |       **32** |   **519.67 ns** |  **9.187 ns** |  **8.594 ns** |  **0.47** |    **0.01** |
|     Regular |      .NET 5.0 |       32 |   912.79 ns | 15.702 ns | 14.688 ns |  0.83 |    0.02 |
| AVX2Dynamic |      .NET 6.0 |       32 |   635.81 ns |  8.064 ns |  7.149 ns |  0.58 |    0.01 |
|     Regular |      .NET 6.0 |       32 |   891.95 ns | 14.362 ns | 13.434 ns |  0.81 |    0.02 |
| AVX2Dynamic |      .NET 7.0 |       32 |   701.49 ns | 11.364 ns | 10.630 ns |  0.63 |    0.01 |
|     Regular |      .NET 7.0 |       32 |   767.82 ns |  7.425 ns |  6.946 ns |  0.70 |    0.01 |
| AVX2Dynamic | .NET Core 3.1 |       32 |   605.34 ns |  9.505 ns |  8.891 ns |  0.55 |    0.01 |
|     Regular | .NET Core 3.1 |       32 | 1,104.18 ns | 13.446 ns | 11.920 ns |  1.00 |    0.00 |
|
## How to use:

Install this library via NuGet, via GitHub Packages or just download and compile it yourself, whatever floats your boat.  
Be aware that this library is licensed under LGPL, so you can use it in your commercial Projects, but please comply with the License.  

For Optimal Performance use:

``Perlin.OptimizedOctavePerlin``

This Method will use regular Perlin Noise for Octaves < 6 or when AVX is not supported.  
Otherwise it will use the improved AVX2Dynamic Method.
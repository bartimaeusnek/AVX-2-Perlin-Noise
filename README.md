# AVX-2-Perlin-Noise
An AVX-2 improved implementation of Ken Perlin's Noise.

## Test Coverage:
100% of my Code is Covered.

## Build Status:
![.NET Core](https://github.com/bartimaeusnek/AVX-2-Perlin-Noise/workflows/.NET%20Core/badge.svg)

## Benchmark Status:
``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.900 (1909/November2018Update/19H2)
Intel Xeon CPU E3-1246 v3 3.50GHz, 1 CPU, 8 logical and 4 physical cores
Frequency=3410078 Hz, Resolution=293.2484 ns, Timer=TSC
.NET Core SDK=3.1.301
  [Host]     : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT
  DefaultJob : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT

RunStrategy=Throughput  

```

|       Method |       Mean |    Error |   StdDev | Ratio |
|------------- |-----------:|---------:|---------:|------:|
| AVX2Parallel |   968.7 ns |  4.85 ns |  4.30 ns |  0.33 |
|         AVX2 | 1,852.9 ns | 32.22 ns | 49.20 ns |  0.62 |
|      Regular | 2,937.6 ns | 15.49 ns | 12.94 ns |  1.00 |

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19043
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.204
  [Host]     : .NET Core 3.1.16 (CoreCLR 4.700.21.26205, CoreFX 4.700.21.26205), X64 RyuJIT
  Job-APWNSB : .NET Core 3.1.16 (CoreCLR 4.700.21.26205, CoreFX 4.700.21.26205), X64 RyuJIT

RunStrategy=Throughput  

```
|                     Method |       Mean |    Error |   StdDev | Ratio |
|--------------------------- |-----------:|---------:|---------:|------:|
|                AVX2Dynamic | 1,045.3 ns |  7.18 ns |  6.71 ns |  0.50 |
|               AVX2Parallel |   636.0 ns |  4.29 ns |  4.01 ns |  0.30 |
| AVX2DynamicDoublePrecision | 1,862.5 ns | 21.95 ns | 20.53 ns |  0.89 |
|                    Regular | 2,103.0 ns | 12.23 ns | 11.44 ns |  1.00 |


Benchmark assumptions:

The program is expected to create 8 Values with 8 Octaves.

AVX2Parallel computes 8 Values at Once, and applies the Octaves in series.
AVX2 computes 8x 1 Value and applies the Octaves at once. (Obsolete)
AVX2Dynamic is AVX2 but it can do any amount of Octaves, AWESOME!
AVX2DynamicDoublePrecision uses longs and floats, therefore is about half as fast as AVX2Dynamic,
but still faster than regular!
Regular computes 8x 1 Value and applies the Octaves in series.

For Optimal Performance use:

``Perlin.OptimizedOctavePerlin``
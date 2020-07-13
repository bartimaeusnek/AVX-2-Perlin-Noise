# AVX-2-Perlin-Noise
An AVX-2 improved implementation of Ken Perlin's Noise.

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

IterationCount=100  RunStrategy=Throughput  

```

|  Method |     Mean |   Error |  StdDev |   Median | Ratio |
|-------- |---------:|--------:|--------:|---------:|------:|
|    AVX2 | 208.9 ns | 1.57 ns | 4.58 ns | 208.5 ns |  0.53 |
| Regular | 392.0 ns | 2.71 ns | 7.81 ns | 389.5 ns |  1.00 |
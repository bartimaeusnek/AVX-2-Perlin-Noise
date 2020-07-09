# AVX-2-Perlin-Noise
An AVX-2 improved implementation of Ken Perlin's Noise.

## Work Status:
Currently only the Float/Int implementation is working faster than the regular implementation. Octaves aren't supported yet.

## Build Status:
![.NET Core](https://github.com/bartimaeusnek/AVX-2-Perlin-Noise/workflows/.NET%20Core/badge.svg)

## Benchmark Status:
``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.17134.1550 (1803/April2018Update/Redstone4)
Intel Xeon CPU E3-1246 v3 3.50GHz, 1 CPU, 8 logical and 4 physical cores
Frequency=3410078 Hz, Resolution=293.2484 ns, Timer=TSC
.NET Core SDK=3.1.301
  [Host]     : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT
  DefaultJob : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT


```
|  Method |     Mean |   Error |  StdDev | Ratio |
|-------- |---------:|--------:|--------:|------:|
|    AVX2 | 141.2 ns | 2.21 ns | 1.72 ns |  0.38 |
| Regular | 373.3 ns | 2.43 ns | 2.03 ns |  1.00 |

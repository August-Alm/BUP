``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT


```
|         Method |            Mean |         Error |          StdDev |
|--------------- |----------------:|--------------:|----------------:|
|        Hoas50k | 26,238,826.5 ns | 840,436.58 ns | 2,478,046.23 ns |
| HoasNoQuote50k |        115.5 ns |       1.32 ns |         1.24 ns |
|     HoasTree15 | 10,199,712.1 ns | 191,344.47 ns |   178,983.73 ns |
|      HoasFact8 | 18,973,271.2 ns | 375,085.34 ns |   385,185.03 ns |

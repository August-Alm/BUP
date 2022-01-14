``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT


```
|         Method |            Mean |         Error |          StdDev |
|--------------- |----------------:|--------------:|----------------:|
|        Hoas50k | 25,910,995.3 ns | 546,116.54 ns | 1,558,101.20 ns |
| HoasNoQuote50k |        114.8 ns |       0.45 ns |         0.40 ns |
|     HoasTree15 | 10,219,327.1 ns | 198,720.80 ns |   291,282.37 ns |
|      HoasFact8 | 19,675,633.4 ns | 384,745.91 ns |   587,547.64 ns |

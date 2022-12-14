``` ini

BenchmarkDotNet=v0.13.1, OS=macOS 13.0.1 (22A400) [Darwin 22.1.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.400
  [Host]     : .NET 6.0.8 (6.0.822.36306), Arm64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.8 (6.0.822.36306), Arm64 RyuJIT


```
|         Method |            Mean |           Error |          StdDev |
|--------------- |----------------:|----------------:|----------------:|
|        Hoas50k | 40,586,461.5 ns | 1,330,842.40 ns | 3,861,015.12 ns |
| HoasNoQuote50k |        185.3 ns |         3.63 ns |         3.73 ns |
|     HoasTree15 | 15,563,498.4 ns |   241,764.43 ns |   248,274.27 ns |
|      HoasFact8 | 30,763,913.9 ns |   609,105.96 ns | 1,066,799.95 ns |

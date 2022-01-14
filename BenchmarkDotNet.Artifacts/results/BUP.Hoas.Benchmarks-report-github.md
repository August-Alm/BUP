``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT


```
|         Method |            Mean |         Error |          StdDev |
|--------------- |----------------:|--------------:|----------------:|
|        Hoas50k | 25,368,752.9 ns | 504,214.39 ns | 1,430,372.30 ns |
| HoasNoQuote50k |        114.9 ns |       0.30 ns |         0.28 ns |
|     HoasTree15 | 10,191,357.7 ns | 201,845.74 ns |   207,280.71 ns |
|      HoasFact8 | 19,093,890.8 ns | 375,968.83 ns |   501,907.59 ns |

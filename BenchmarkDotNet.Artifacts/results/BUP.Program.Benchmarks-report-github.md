``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  Job-XMAPTD : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |             Mean |         Error |        StdDev |             Median |
|--------------------------- |-----------------:|--------------:|--------------:|-------------------:|
|       ParseAndNormalise50k |  6,309,594.47 ns |  70,876.11 ns |  66,297.56 ns |  6,316,417.0000 ns |
|               Normalise50k |         36.71 ns |      15.94 ns |      46.50 ns |          0.0000 ns |
|    ParseAndNormaliseTree15 |  9,110,471.38 ns | 142,450.06 ns | 118,952.26 ns |  9,118,125.0000 ns |
|     ParseAndNormaliseFact8 |  3,807,845.64 ns |  38,137.31 ns |  33,807.73 ns |  3,793,250.5000 ns |
|        ParseAndNormalise1M | 40,150,961.27 ns | 188,436.61 ns | 176,263.72 ns | 40,091,417.0000 ns |
| ParseAndNormalisePearls100 |    106,421.07 ns |   2,126.20 ns |   3,116.56 ns |    105,125.0000 ns |

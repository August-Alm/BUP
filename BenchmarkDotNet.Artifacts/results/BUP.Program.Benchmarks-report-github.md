``` ini

BenchmarkDotNet=v0.13.1, OS=pop 21.10
AMD Ryzen 7 PRO 4750G with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT DEBUG
  Job-RGYAHU : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |            Mean |         Error |        StdDev |          Median |
|--------------------------- |----------------:|--------------:|--------------:|----------------:|
|       ParseAndNormalise50k |  3,610,278.3 ns |  53,111.07 ns | 106,068.54 ns |  3,578,963.0 ns |
|               Normalise50k |        106.6 ns |       9.00 ns |      25.09 ns |        101.0 ns |
|    ParseAndNormaliseTree15 | 16,130,032.4 ns | 309,742.74 ns | 274,578.87 ns | 16,215,045.5 ns |
|     ParseAndNormaliseFact8 |  2,775,256.7 ns |  55,349.08 ns |  87,789.50 ns |  2,756,611.0 ns |
|        ParseAndNormalise1M | 39,882,852.8 ns | 173,694.98 ns | 162,474.39 ns | 39,851,890.5 ns |
| ParseAndNormalisePearls100 |    143,539.4 ns |   4,154.16 ns |  11,511.15 ns |    139,245.0 ns |

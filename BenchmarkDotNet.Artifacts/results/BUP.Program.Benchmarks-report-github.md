``` ini

BenchmarkDotNet=v0.13.1, OS=pop 21.10
AMD Ryzen 7 PRO 4750G with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT DEBUG
  Job-UWRYID : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |            Mean |         Error |        StdDev |          Median |
|--------------------------- |----------------:|--------------:|--------------:|----------------:|
|       ParseAndNormalise50k |  9,318,633.7 ns | 169,259.56 ns | 158,325.50 ns |  9,307,377.0 ns |
|               Normalise50k |        120.6 ns |      21.50 ns |      56.26 ns |        116.0 ns |
|    ParseAndNormaliseTree15 | 16,570,589.5 ns | 320,772.32 ns | 300,050.63 ns | 16,608,327.0 ns |
|     ParseAndNormaliseFact8 |  6,110,336.6 ns |  24,691.59 ns |  20,618.59 ns |  6,113,390.0 ns |
|        ParseAndNormalise1M | 89,752,072.4 ns | 110,664.41 ns | 103,515.56 ns | 89,734,888.5 ns |
| ParseAndNormalisePearls100 |    167,809.7 ns |   9,607.30 ns |  26,781.31 ns |    159,881.0 ns |

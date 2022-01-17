``` ini

BenchmarkDotNet=v0.13.1, OS=pop 21.10
AMD Ryzen 7 PRO 4750G with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT DEBUG
  Job-BMGZJS : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |            Mean |         Error |       StdDev |          Median |
|--------------------------- |----------------:|--------------:|-------------:|----------------:|
|       ParseAndNormalise50k |  9,329,633.4 ns |  86,960.90 ns |  81,343.3 ns |  9,320,191.0 ns |
|               Normalise50k |        264.4 ns |      54.85 ns |     154.7 ns |        234.5 ns |
|    ParseAndNormaliseTree15 | 16,558,506.0 ns | 208,299.90 ns | 194,843.9 ns | 16,529,922.5 ns |
|     ParseAndNormaliseFact8 |  6,198,053.1 ns |  92,478.48 ns |  81,979.8 ns |  6,165,139.5 ns |
|        ParseAndNormalise1M | 92,305,414.7 ns | 231,664.64 ns | 205,364.7 ns | 92,380,346.5 ns |
| ParseAndNormalisePearls100 |    135,495.4 ns |   2,380.87 ns |   6,060.1 ns |    132,917.0 ns |

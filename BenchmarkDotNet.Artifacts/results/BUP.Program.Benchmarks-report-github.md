``` ini

BenchmarkDotNet=v0.13.1, OS=pop 21.10
AMD Ryzen 7 PRO 4750G with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT DEBUG
  Job-VODJIB : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |            Mean |        Error |       StdDev |          Median |
|--------------------------- |----------------:|-------------:|-------------:|----------------:|
|       ParseAndNormalise50k |  4,643,864.0 ns | 270,983.5 ns | 794,747.7 ns |  4,349,802.0 ns |
|               Normalise50k |        703.4 ns |     192.2 ns |     560.5 ns |        591.5 ns |
|    ParseAndNormaliseTree15 | 17,273,606.0 ns | 333,189.8 ns | 456,073.9 ns | 17,214,088.0 ns |
|     ParseAndNormaliseFact8 |  2,702,515.1 ns | 172,509.2 ns | 508,647.1 ns |  2,822,934.5 ns |
|        ParseAndNormalise1M | 43,664,121.8 ns |  51,855.3 ns |  45,968.4 ns | 43,662,689.0 ns |
| ParseAndNormalisePearls100 |    154,888.9 ns |   4,894.3 ns |  14,199.3 ns |    155,133.0 ns |

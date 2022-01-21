``` ini

BenchmarkDotNet=v0.13.1, OS=pop 21.10
AMD Ryzen 7 PRO 4750G with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT DEBUG
  Job-IXEILU : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |            Mean |         Error |       StdDev |          Median |
|--------------------------- |----------------:|--------------:|-------------:|----------------:|
|       ParseAndNormalise50k |  9,492,494.1 ns | 112,713.46 ns |  99,917.5 ns |  9,481,286.5 ns |
|               Normalise50k |        197.5 ns |      66.40 ns |     192.6 ns |        126.0 ns |
|    ParseAndNormaliseTree15 | 16,219,842.7 ns | 309,906.01 ns | 289,886.3 ns | 16,353,607.0 ns |
|     ParseAndNormaliseFact8 |  6,232,910.3 ns |  30,510.19 ns |  25,477.4 ns |  6,229,818.5 ns |
|        ParseAndNormalise1M | 91,526,342.1 ns | 144,082.02 ns | 134,774.4 ns | 91,541,197.0 ns |
| ParseAndNormalisePearls100 |    140,522.6 ns |   3,612.79 ns |  10,189.9 ns |    137,761.0 ns |

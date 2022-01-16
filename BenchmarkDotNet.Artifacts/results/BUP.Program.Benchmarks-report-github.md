``` ini

BenchmarkDotNet=v0.13.1, OS=pop 21.10
AMD Ryzen 7 PRO 4750G with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT DEBUG
  Job-FFWWGT : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |            Mean |         Error |        StdDev |
|--------------------------- |----------------:|--------------:|--------------:|
|       ParseAndNormalise50k |  5,521,152.5 ns |  58,442.41 ns |  54,667.07 ns |
|               Normalise50k |        183.8 ns |      22.48 ns |      60.39 ns |
|    ParseAndNormaliseTree15 | 15,565,138.4 ns | 299,757.43 ns | 294,401.88 ns |
|     ParseAndNormaliseFact8 |  2,625,039.2 ns |  50,853.08 ns |  47,568.01 ns |
|        ParseAndNormalise1M | 42,340,875.9 ns |  66,862.36 ns |  55,833.10 ns |
| ParseAndNormalisePearls100 |    131,464.8 ns |   1,827.40 ns |   2,376.14 ns |

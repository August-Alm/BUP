``` ini

BenchmarkDotNet=v0.13.1, OS=pop 21.10
AMD Ryzen 7 PRO 4750G with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT DEBUG
  Job-XVMFQM : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |             Mean |         Error |        StdDev |
|--------------------------- |-----------------:|--------------:|--------------:|
|       ParseAndNormalise50k |   9,556,429.0 ns |  21,279.88 ns |  39,443.62 ns |
|               Normalise50k |         219.4 ns |      32.03 ns |      86.05 ns |
|    ParseAndNormaliseTree15 |  18,425,357.7 ns | 358,323.20 ns | 465,921.38 ns |
|     ParseAndNormaliseFact8 |   8,781,467.8 ns |  42,829.05 ns |  40,062.32 ns |
|        ParseAndNormalise1M | 123,256,751.0 ns | 167,666.53 ns | 156,835.37 ns |
| ParseAndNormalisePearls100 |     163,791.1 ns |   7,291.60 ns |  20,446.44 ns |

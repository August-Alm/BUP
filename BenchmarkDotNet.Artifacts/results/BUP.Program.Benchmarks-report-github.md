``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  Job-BDWQSP : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |            Mean |         Error |       StdDev |           Median |
|--------------------------- |----------------:|--------------:|-------------:|-----------------:|
|       ParseAndNormalise50k |  5,974,367.7 ns |  62,310.40 ns |  58,285.2 ns |  5,973,770.50 ns |
|               Normalise50k |        114.3 ns |      42.58 ns |     118.0 ns |         83.00 ns |
|    ParseAndNormaliseTree15 |  9,037,425.6 ns | 132,967.59 ns | 117,872.3 ns |  9,068,521.00 ns |
|     ParseAndNormaliseFact8 |  3,931,658.6 ns |  72,838.75 ns |  71,537.4 ns |  3,920,249.50 ns |
|        ParseAndNormalise1M | 44,214,567.4 ns |  61,808.40 ns |  51,612.8 ns | 44,206,542.00 ns |
| ParseAndNormalisePearls100 |    104,028.1 ns |   1,313.92 ns |   1,025.8 ns |    104,292.00 ns |

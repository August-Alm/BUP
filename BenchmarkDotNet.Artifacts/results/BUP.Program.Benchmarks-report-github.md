``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  Job-HRVXUR : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |        Mean |     Error |    StdDev |
|--------------------------- |------------:|----------:|----------:|
|        ParseAndNormalise5k |    613.4 μs |  10.86 μs |  10.16 μs |
|    ParseAndNormaliseTree15 |  9,277.3 μs | 121.68 μs | 113.82 μs |
|     ParseAndNormaliseFact7 |    552.5 μs |   6.11 μs |   5.11 μs |
|        ParseAndNormalise1M | 47,423.9 μs | 129.24 μs | 107.92 μs |
| ParseAndNormalisePearls100 |    104.1 μs |   1.85 μs |   1.98 μs |

``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  Job-MXWJFH : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |        Mean |     Error |    StdDev |
|--------------------------- |------------:|----------:|----------:|
|       ParseAndNormalise50k |  6,007.7 μs |  67.82 μs |  60.12 μs |
|    ParseAndNormaliseTree15 |  9,130.9 μs | 138.61 μs | 129.65 μs |
|     ParseAndNormaliseFact8 |  3,855.6 μs |  34.21 μs |  32.00 μs |
|        ParseAndNormalise1M | 44,217.7 μs |  93.02 μs |  77.67 μs |
| ParseAndNormalisePearls100 |    105.1 μs |   2.09 μs |   3.13 μs |

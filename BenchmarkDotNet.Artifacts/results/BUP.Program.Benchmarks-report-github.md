``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  Job-XJGFBN : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |        Mean |     Error |    StdDev |
|--------------------------- |------------:|----------:|----------:|
|        ParseAndNormalise5k |    608.6 μs |   5.10 μs |   4.26 μs |
|    ParseAndNormaliseTree15 |  9,292.6 μs | 133.63 μs | 124.99 μs |
|             NormaliseFact7 |    553.3 μs |   2.52 μs |   1.97 μs |
|        NormaliseAndParse1M | 47,439.9 μs |  92.24 μs |  81.77 μs |
| NormaliseAndParsePearls100 |    102.2 μs |   1.03 μs |   0.80 μs |

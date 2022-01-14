``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  Job-ZAIEVU : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |             Mean |         Error |        StdDev |
|--------------------------- |-----------------:|--------------:|--------------:|
|       ParseAndNormalise50k |  6,226,877.36 ns |  88,805.75 ns |  78,723.99 ns |
|               Normalise50k |         70.10 ns |      27.73 ns |      77.76 ns |
|    ParseAndNormaliseTree15 |  9,133,899.00 ns | 101,598.52 ns | 124,772.11 ns |
|     ParseAndNormaliseFact8 |  3,838,727.67 ns |  59,190.68 ns |  55,367.00 ns |
|        ParseAndNormalise1M | 40,185,799.77 ns | 322,751.83 ns | 301,902.26 ns |
| ParseAndNormalisePearls100 |    104,002.62 ns |   1,317.57 ns |   1,100.23 ns |

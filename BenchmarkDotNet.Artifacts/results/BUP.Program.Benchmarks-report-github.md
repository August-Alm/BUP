``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  Job-NGZFER : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |             Mean |         Error |        StdDev |
|--------------------------- |-----------------:|--------------:|--------------:|
|       ParseAndNormalise50k |  7,085,041.73 ns |  78,461.64 ns |  73,393.06 ns |
|               Normalise50k |         76.39 ns |      18.46 ns |      53.84 ns |
|    ParseAndNormaliseTree15 |  9,698,911.53 ns | 166,546.56 ns | 155,787.75 ns |
|     ParseAndNormaliseFact8 |  4,808,723.29 ns |  59,581.37 ns |  52,817.33 ns |
|        ParseAndNormalise1M | 56,890,899.33 ns | 334,600.82 ns | 261,234.31 ns |
| ParseAndNormalisePearls100 |    104,708.57 ns |   1,643.87 ns |   1,457.25 ns |

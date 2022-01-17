``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  Job-FRHHCA : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |            Mean |         Error |        StdDev |
|--------------------------- |----------------:|--------------:|--------------:|
|       ParseAndNormalise50k |  5,747,639.6 ns |  53,624.96 ns |  86,594.31 ns |
|               Normalise50k |        121.7 ns |      30.53 ns |      85.11 ns |
|    ParseAndNormaliseTree15 |  9,934,238.1 ns | 151,998.87 ns | 142,179.84 ns |
|     ParseAndNormaliseFact8 |  4,122,365.5 ns |  62,182.95 ns | 140,357.29 ns |
|        ParseAndNormalise1M | 69,879,752.6 ns | 392,783.56 ns | 348,192.39 ns |
| ParseAndNormalisePearls100 |    106,565.9 ns |     618.35 ns |     482.77 ns |

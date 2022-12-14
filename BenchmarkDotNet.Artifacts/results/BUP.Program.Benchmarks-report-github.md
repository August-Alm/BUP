``` ini

BenchmarkDotNet=v0.13.1, OS=macOS 13.0.1 (22A400) [Darwin 22.1.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.400
  [Host]     : .NET 6.0.8 (6.0.822.36306), Arm64 RyuJIT DEBUG
  Job-FEQAST : .NET 6.0.8 (6.0.822.36306), Arm64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                     Method |            Mean |           Error |         StdDev |           Median |
|--------------------------- |----------------:|----------------:|---------------:|-----------------:|
|       ParseAndNormalise50k |  7,670,791.1 ns |     9,782.18 ns |     8,671.6 ns |  7,670,145.00 ns |
|               Normalise50k |        149.4 ns |        98.99 ns |       272.6 ns |         21.50 ns |
|    ParseAndNormaliseTree15 | 16,657,657.3 ns | 1,067,459.96 ns | 3,130,675.3 ns | 15,090,980.00 ns |
|     ParseAndNormaliseFact8 |  4,903,641.7 ns |   151,554.32 ns |   393,909.8 ns |  4,829,917.00 ns |
|        ParseAndNormalise1M | 89,485,991.0 ns | 1,773,036.96 ns | 3,965,651.2 ns | 89,304,688.00 ns |
| ParseAndNormalisePearls100 |    121,225.1 ns |     5,036.32 ns |    13,000.4 ns |    116,520.50 ns |

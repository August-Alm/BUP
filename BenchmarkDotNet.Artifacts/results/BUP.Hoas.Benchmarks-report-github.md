``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT


```
|     Method |     Mean |    Error |   StdDev |   Median |
|----------- |---------:|---------:|---------:|---------:|
|    Hoas50k | 25.81 ms | 0.509 ms | 1.170 ms | 26.22 ms |
| HoasTree15 | 10.12 ms | 0.189 ms | 0.203 ms | 10.12 ms |
|  HoasFact8 | 19.43 ms | 0.387 ms | 0.579 ms | 19.54 ms |

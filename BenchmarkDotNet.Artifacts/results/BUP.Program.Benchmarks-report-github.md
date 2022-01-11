``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT


```
|          Method |     Mean |     Error |    StdDev |
|---------------- |---------:|----------:|----------:|
|  NormaliseFact8 |       NA |        NA |        NA |
| NormaliseTree15 | 9.060 ms | 0.0726 ms | 0.0644 ms |

Benchmarks with issues:
  Benchmarks.NormaliseFact8: DefaultJob

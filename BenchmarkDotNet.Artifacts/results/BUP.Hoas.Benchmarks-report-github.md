``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT


```
|     Method |      Mean |     Error |    StdDev |
|----------- |----------:|----------:|----------:|
|     Hoas5k |  1.055 ms | 0.0015 ms | 0.0013 ms |
| HoasTree15 | 10.016 ms | 0.1598 ms | 0.1416 ms |
|  HoasFact7 |  1.316 ms | 0.0042 ms | 0.0038 ms |

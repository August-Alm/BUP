``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT


```
|     Method |      Mean |     Error |    StdDev |
|----------- |----------:|----------:|----------:|
|     Hoas5k |  1.057 ms | 0.0040 ms | 0.0033 ms |
| HoasTree15 | 10.086 ms | 0.1925 ms | 0.1977 ms |
|  HoasFact7 |  1.323 ms | 0.0046 ms | 0.0041 ms |

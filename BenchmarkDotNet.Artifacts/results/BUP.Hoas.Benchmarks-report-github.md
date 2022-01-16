``` ini

BenchmarkDotNet=v0.13.1, OS=pop 21.10
AMD Ryzen 7 PRO 4750G with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT


```
|         Method |           Mean |         Error |        StdDev |
|--------------- |---------------:|--------------:|--------------:|
|        Hoas50k | 8,081,650.2 ns |  23,665.61 ns |  19,761.85 ns |
| HoasNoQuote50k |       176.7 ns |       0.50 ns |       0.42 ns |
|     HoasTree15 | 6,648,604.4 ns | 130,866.88 ns | 248,987.84 ns |
|      HoasFact8 | 8,024,277.5 ns | 107,810.44 ns | 100,845.95 ns |

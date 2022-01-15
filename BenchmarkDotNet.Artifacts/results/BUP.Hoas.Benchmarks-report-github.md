``` ini

BenchmarkDotNet=v0.13.1, OS=pop 21.10
AMD Ryzen 7 PRO 4750G with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT


```
|         Method |           Mean |         Error |        StdDev |         Median |
|--------------- |---------------:|--------------:|--------------:|---------------:|
|        Hoas50k | 8,381,943.9 ns | 167,285.95 ns | 288,560.04 ns | 8,302,295.6 ns |
| HoasNoQuote50k |       178.4 ns |       3.54 ns |       5.08 ns |       180.2 ns |
|     HoasTree15 | 6,166,992.2 ns |  70,506.07 ns |  65,951.42 ns | 6,151,650.4 ns |
|      HoasFact8 | 8,331,092.0 ns | 174,990.24 ns | 490,691.40 ns | 8,137,810.6 ns |

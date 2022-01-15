``` ini

BenchmarkDotNet=v0.13.1, OS=pop 21.10
AMD Ryzen 7 PRO 4750G with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT DEBUG
  DefaultJob : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT


```
|         Method |           Mean |         Error |        StdDev |
|--------------- |---------------:|--------------:|--------------:|
|        Hoas50k | 8,373,439.9 ns | 152,106.75 ns | 142,280.75 ns |
| HoasNoQuote50k |       178.9 ns |       1.30 ns |       1.21 ns |
|     HoasTree15 | 6,330,610.4 ns |  65,518.45 ns |  58,080.40 ns |
|      HoasFact8 | 7,912,557.2 ns |  33,689.02 ns |  28,131.86 ns |

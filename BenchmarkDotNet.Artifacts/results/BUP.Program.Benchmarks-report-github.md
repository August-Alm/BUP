``` ini

<<<<<<< HEAD
BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT DEBUG
  Job-NGZFER : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT
=======
BenchmarkDotNet=v0.13.1, OS=pop 21.10
AMD Ryzen 7 PRO 4750G with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT DEBUG
  Job-IJUNSM : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
>>>>>>> c39a304c54ca00e5669e865d4234485b39fd7651

InvocationCount=1  UnrollFactor=1  

```
<<<<<<< HEAD
|                     Method |             Mean |         Error |        StdDev |
|--------------------------- |-----------------:|--------------:|--------------:|
|       ParseAndNormalise50k |  7,085,041.73 ns |  78,461.64 ns |  73,393.06 ns |
|               Normalise50k |         76.39 ns |      18.46 ns |      53.84 ns |
|    ParseAndNormaliseTree15 |  9,698,911.53 ns | 166,546.56 ns | 155,787.75 ns |
|     ParseAndNormaliseFact8 |  4,808,723.29 ns |  59,581.37 ns |  52,817.33 ns |
|        ParseAndNormalise1M | 56,890,899.33 ns | 334,600.82 ns | 261,234.31 ns |
| ParseAndNormalisePearls100 |    104,708.57 ns |   1,643.87 ns |   1,457.25 ns |
=======
|                     Method |            Mean |           Error |         StdDev |          Median |
|--------------------------- |----------------:|----------------:|---------------:|----------------:|
|       ParseAndNormalise50k |  9,188,787.0 ns |    94,879.26 ns |    79,228.5 ns |  9,181,757.0 ns |
|               Normalise50k |        202.1 ns |        47.05 ns |       131.9 ns |        161.0 ns |
|    ParseAndNormaliseTree15 | 16,334,512.4 ns |   289,240.27 ns |   270,555.5 ns | 16,323,355.5 ns |
|     ParseAndNormaliseFact8 |  6,141,230.6 ns |    58,571.17 ns |    54,787.5 ns |  6,130,286.0 ns |
|        ParseAndNormalise1M | 92,056,779.1 ns | 1,806,701.99 ns | 1,774,422.9 ns | 91,520,021.5 ns |
| ParseAndNormalisePearls100 |    142,985.0 ns |     3,755.71 ns |    10,531.4 ns |    138,802.0 ns |
>>>>>>> c39a304c54ca00e5669e865d4234485b39fd7651

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                     | Mean     | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated  |
|--------------------------- |---------:|---------:|---------:|---------:|---------:|---------:|-----------:|
| SequentialPrimeCalculation | 35.15 ms | 0.473 ms | 0.419 ms |        - |        - |        - |  306.81 KB |
| ParallelPrimeCalculation   | 12.81 ms | 0.255 ms | 0.323 ms | 375.0000 | 234.3750 | 187.5000 | 2056.57 KB |

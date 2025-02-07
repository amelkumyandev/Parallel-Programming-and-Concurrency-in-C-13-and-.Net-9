```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method               | Mean      | Error     | StdDev    |
|--------------------- |----------:|----------:|----------:|
| SequentialProcessing | 13.592 ms | 0.2713 ms | 0.4381 ms |
| ParallelProcessing   |  5.265 ms | 0.1022 ms | 0.1177 ms |

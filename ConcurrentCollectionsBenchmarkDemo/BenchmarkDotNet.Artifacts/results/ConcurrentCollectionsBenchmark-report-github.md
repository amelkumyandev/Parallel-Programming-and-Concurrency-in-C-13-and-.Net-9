```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                   | Mean      | Error     | StdDev    | Allocated |
|------------------------- |----------:|----------:|----------:|----------:|
| ConcurrentQueueTest      | 12.794 ms | 0.0327 ms | 0.0255 ms |   3.18 KB |
| ConcurrentBagTest        |  4.186 ms | 0.0374 ms | 0.0350 ms |   3.02 KB |
| ConcurrentDictionaryTest |  1.501 ms | 0.0290 ms | 0.0257 ms |   3.02 KB |

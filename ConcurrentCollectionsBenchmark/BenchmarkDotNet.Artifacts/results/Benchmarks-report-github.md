```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  ShortRun : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=1  

```
| Method                   | Mean       | Error       | StdDev    | Allocated |
|------------------------- |-----------:|------------:|----------:|----------:|
| ConcurrentDictionaryTest |   8.562 ms |   0.0991 ms | 0.0054 ms |   3.26 KB |
| ConcurrentQueueTest      |  77.189 ms |   1.1186 ms | 0.0613 ms |   3.38 KB |
| BlockingCollectionTest   | 263.251 ms |  59.6338 ms | 3.2687 ms |   3.81 KB |
| LockedDictionaryTest     | 110.840 ms | 111.5497 ms | 6.1144 ms |   3.42 KB |

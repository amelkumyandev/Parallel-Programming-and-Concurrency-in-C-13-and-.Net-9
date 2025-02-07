using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static AsyncLocal<string> userContext = new AsyncLocal<string>();

    static async Task Main()
    {
        Console.WriteLine("=== SynchronizationContext & ExecutionContext Demo ===\n");

        // 1) Default (no SyncContext)
        userContext.Value = "UserA";
        Console.WriteLine("[1] Default: No specialized SyncContext");
        await ShowContextFlowAsync("Default environment");

        // 2) Custom single-thread context (simulate a UI-like environment)
        using (var customCtx = new CustomSingleThreadContext())
        {
            SynchronizationContext.SetSynchronizationContext(customCtx);
            userContext.Value = "UserB";
            Console.WriteLine("\n[2] CustomSingleThreadContext:");
            await ShowContextFlowAsync("Inside custom context");
            SynchronizationContext.SetSynchronizationContext(null);
        }

        // 3) Demonstrate ConfigureAwait(false) with or without a SyncContext
        userContext.Value = "UserC";
        Console.WriteLine("\n[3] ConfigureAwait(false) demonstration in default context:");
        await ShowConfigureAwaitDemo();

        Console.WriteLine("\nAll demos completed. Press ENTER to exit.");
        Console.ReadLine();
    }

    private static async Task ShowContextFlowAsync(string label)
    {
        Console.WriteLine($"  - Before await: {label}");
        PrintStatus();

        await Task.Delay(100);

        if (SynchronizationContext.Current is CustomSingleThreadContext ctx)
        {
            SynchronizationContext.SetSynchronizationContext(ctx);
        }

        Console.WriteLine($"  - After await: {label}");
        PrintStatus();
    }

    private static async Task ShowConfigureAwaitDemo()
    {
        Console.WriteLine("  - Before await Task.Delay(100).ConfigureAwait(false)");
        PrintStatus();

        await Task.Delay(100).ConfigureAwait(false);

        Console.WriteLine("  - After await with ConfigureAwait(false)");
        PrintStatus();
    }

    private static void PrintStatus()
    {
        var syncCtx = SynchronizationContext.Current?.GetType().Name ?? "None";
        Console.WriteLine($"    Thread={Thread.CurrentThread.ManagedThreadId}, " +
                          $"SyncContext={syncCtx}, " +
                          $"AsyncLocalUserContext='{userContext.Value}'");
    }
}

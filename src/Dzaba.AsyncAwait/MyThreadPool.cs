using System.Collections.Concurrent;

namespace Dzaba.AsyncAwait;

public static class MyThreadPool
{
    private static readonly BlockingCollection<ActionWithContext> workItems = new BlockingCollection<ActionWithContext>();
    private static readonly Thread[] threads;

    static MyThreadPool()
    {
        threads = Enumerable.Range(0, Environment.ProcessorCount)
            .Select(_ =>
            {
                var thread = new Thread(ThreadMethod)
                {
                    IsBackground = true,
                };
                thread.Start();
                return thread;
            })
            .ToArray();
    }

    private static void ThreadMethod()
    {
        while (true)
        {
            var workItem = workItems.Take();
            workItem.Invoke();
        }
    }

    public static void QueueUserWorkItem(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        workItems.Add(ActionWithContext.Capture(action));
    }
}

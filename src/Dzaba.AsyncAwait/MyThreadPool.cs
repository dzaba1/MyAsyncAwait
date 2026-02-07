using System.Collections.Concurrent;

namespace Dzaba.AsyncAwait;

public static class MyThreadPool
{
    private static readonly BlockingCollection<QueuedWorkItem> workItems = new BlockingCollection<QueuedWorkItem>();
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
            if (workItem.Context == null)
            {
                workItem.Action();
            }
            else
            {
                ExecutionContext.Run(workItem.Context, s => ((QueuedWorkItem)s).Action.Invoke(), workItem);
            }
        }
    }

    public static void QueueUserWorkItem(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        workItems.Add(new QueuedWorkItem(action, ExecutionContext.Capture()));
    }

    internal class QueuedWorkItem(Action action, ExecutionContext context)
    {
        public Action Action { get; } = action;

        public ExecutionContext Context { get; } = context;
    }
}

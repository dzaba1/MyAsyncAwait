using System.Runtime.CompilerServices;

namespace Dzaba.AsyncAwait;

public class TaskAwaiter : INotifyCompletion
{
    public TaskAwaiter(IMyTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        Task = task;
    }

    public IMyTask Task { get; }

    public bool IsCompleted => Task.IsCompleted;

    public void GetResult() => Task.Wait();

    public void OnCompleted(Action continuation) => Task.ContinueWith(continuation);
}

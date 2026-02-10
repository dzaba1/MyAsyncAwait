using System.Runtime.ExceptionServices;

namespace Dzaba.AsyncAwait;

public interface ITask
{
    bool IsCompleted { get; }
    Exception Exception { get; }
    void Wait();
    ITask ContinueWith(Action action);
}

public class Task : ITask
{
    private bool isCompleted = false;
    private Exception exception;
    private ActionWithContext continuation;
    private readonly object syncLock = new object();

    public bool IsCompleted
    {
        get
        {
            lock (syncLock)
            {
                return isCompleted;
            }
        }
    }

    public Exception Exception
    {
        get
        {
            lock (syncLock)
            {
                return exception;
            }
        }
    }

    public ITask ContinueWith(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var task = new Task();
        var callback = () => ActionCallback(task, action);

        lock (syncLock)
        {
            if (isCompleted)
            {
                MyThreadPool.QueueUserWorkItem(callback);
            }
            else
            {
                continuation = ActionWithContext.Capture(callback);
            }
        }

        return task;
    }

    public void Wait()
    {
        ManualResetEventSlim signal = null;

        lock (syncLock)
        {
            if (!isCompleted)
            {
                signal = new ManualResetEventSlim(false);
                ContinueWith(signal.Set);
            }
        }

        signal?.Wait();

        if (exception != null)
        {
            //throw new AggregateException(exception);
            ExceptionDispatchInfo.Throw(exception);
        }
    }

    private void Complete(Exception ex)
    {
        lock (syncLock)
        {
            if (isCompleted)
            {
                throw new InvalidOperationException("Task is already completed.");
            }

            isCompleted = true;
            exception = ex;

            if (continuation != null)
            {
                MyThreadPool.QueueUserWorkItem(ContinationMethod);
            }
        }
    }

    private void ContinationMethod()
    {
        continuation.Invoke();
    }

    private static void ActionCallback(Task task, Action action)
    {
        try
        {
            action();
            task.Complete(null);
        }
        catch (Exception ex)
        {
            task.Complete(ex);
        }
    }

    public static Task Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var task = new Task();

        MyThreadPool.QueueUserWorkItem(() => ActionCallback(task, action));

        return task;
    }

    public static Task Delay(TimeSpan delay)
    {
        var task = new Task();

        var timer = new Timer(_ => task.Complete(null));
        timer.Change(delay, Timeout.InfiniteTimeSpan);

        return task;
    }
}
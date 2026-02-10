using System.Runtime.ExceptionServices;

namespace Dzaba.AsyncAwait;

public interface IMyTask
{
    bool IsCompleted { get; }
    Exception Exception { get; }
    void Wait();
    IMyTask ContinueWith(Action action);
    IMyTask ContinueWith(Func<IMyTask> action);
    TaskAwaiter GetAwaiter();
}

public class MyTask : IMyTask
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

    private void QueueContinueWith(Action callback)
    {
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
    }

    public IMyTask ContinueWith(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var task = new MyTask();
        var callback = () => ActionCallback(task, action);

        QueueContinueWith(callback);

        return task;
    }

    public IMyTask ContinueWith(Func<IMyTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var task = new MyTask();
        var callback = () =>
        {
            try
            {
                var next = action();
                next.ContinueWith(() =>
                {
                    if (next.Exception != null)
                    {
                        task.Complete(next.Exception);
                    }
                    else
                    {
                        task.Complete(null);
                    }
                });
            }
            catch (Exception ex)
            {
                task.Complete(ex);
            }
        };

        QueueContinueWith(callback);

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

    public TaskAwaiter GetAwaiter()
    {
        return new TaskAwaiter(this);
    }

    private void ContinationMethod()
    {
        continuation.Invoke();
    }

    private static void ActionCallback(MyTask task, Action action)
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

    public static MyTask Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var task = new MyTask();

        MyThreadPool.QueueUserWorkItem(() => ActionCallback(task, action));

        return task;
    }

    public static MyTask Delay(TimeSpan delay)
    {
        var task = new MyTask();

        var timer = new Timer(_ => task.Complete(null));
        timer.Change(delay, Timeout.InfiniteTimeSpan);

        return task;
    }

    private static void MoveNext(IEnumerator<IMyTask> enumerator, MyTask task)
    {
        try
        {
            if (enumerator.MoveNext())
            {
                var next = enumerator.Current;
                next.ContinueWith(() => MoveNext(enumerator, task));
            }
            else
            {
                task.Complete(null);
            }
        }
        catch (Exception ex)
        {
            task.Complete(ex);
        }
    }

    public static MyTask Iterate(IEnumerable<IMyTask> tasks)
    {
        var task = new MyTask();

        var enumerator = tasks.GetEnumerator();
        MoveNext(enumerator, task);

        return task;
    }
}
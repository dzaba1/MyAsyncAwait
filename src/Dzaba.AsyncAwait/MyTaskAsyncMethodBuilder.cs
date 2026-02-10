using System.Runtime.CompilerServices;

namespace Dzaba.AsyncAwait;

public class MyTaskAsyncMethodBuilder
{
    private MyTask task;

    public MyTask Task => task;

    public static MyTaskAsyncMethodBuilder Create()
    {
        return new MyTaskAsyncMethodBuilder
        {
            task = new MyTask()
        };
    }

    public void SetException(Exception exception)
    {
        task.Complete(exception);
    }

    public void SetResult()
    {
        task.Complete(null);
    }

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(stateMachine.MoveNext);
    }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
    }

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        
    }
}
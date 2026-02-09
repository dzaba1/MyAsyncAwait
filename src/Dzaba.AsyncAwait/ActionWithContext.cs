namespace Dzaba.AsyncAwait;

public class ActionWithContext
{
    public ActionWithContext(Action action, ExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(action);

        Action = action;
        Context = context;
    }

    public Action Action { get; }
    public ExecutionContext Context { get; }

    public static ActionWithContext Capture(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new ActionWithContext(action, ExecutionContext.Capture());
    }

    public void Invoke()
    {
        if (Context == null)
        {
            Action();
        }
        else
        {
            ExecutionContext.Run(Context, s => ((Action)s).Invoke(), Action);
        }
    }
}

using NUnit.Framework;

namespace Dzaba.AsyncAwait.Tests;

[TestFixture]
public class MyThreadPoolTests
{
    [Test]
    public void QueueUserWorkItem_WhenCalled_ThenActionIsExecuted()
    {
        var signal = new ManualResetEventSlim(false);

        MyThreadPool.QueueUserWorkItem(signal.Set);

        signal.Wait();
    }
}

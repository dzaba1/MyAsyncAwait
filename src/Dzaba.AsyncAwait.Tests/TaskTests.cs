using FluentAssertions;
using NUnit.Framework;

namespace Dzaba.AsyncAwait.Tests
{
    [TestFixture]
    public class TaskTests
    {
        [Test]
        public void Wait_WhenCalled_ThenItWaits()
        {
            var finished = false;
            var task = Task.Run(() => { finished = true; });
            task.Wait();

            finished.Should().BeTrue();
        }

        [Test]
        public void Wait_WhenError_ThenException()
        {
            var task = Task.Run(() => throw new Exception("Test"));

            this.Invoking(_ => task.Wait()).Should().Throw<Exception>();
        }
    }
}

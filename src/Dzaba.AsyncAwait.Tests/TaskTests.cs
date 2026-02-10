using FluentAssertions;
using NUnit.Framework;
using System.Diagnostics.Metrics;

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

        [Test]
        public void ContinueWith_WhenDelay_ThenICanWait()
        {
            var finished = false;
            Task.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith(() => finished = true)
                .Wait();

            finished.Should().BeTrue();
        }

        [Test]
        public void ContinueWith_WhenMultipleInvocations_ThenICanWait()
        {
            var counter = 0;
            Task.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith(() =>
                {
                    counter++;
                    return Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(() =>
                    {
                        counter++;
                        return Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(() =>
                        {
                            counter++;
                            return Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(() =>
                            {
                                counter++;
                            });
                        });
                    });
                })
                .Wait();

            counter.Should().Be(4);
        }

        private static IEnumerable<Task> DelayAndIncrement(int delayCount, RefInt value)
        {
            for (int i = 0; i < delayCount; i++)
            {
                yield return Task.Delay(TimeSpan.FromSeconds(1));
                value.Value++;
            }
        }

        [Test]
        public void Iterate_WhenEnumerableTasks_ThenWait()
        {
            var counter = new RefInt();
            var delays = DelayAndIncrement(4, counter);
            Task.Iterate(delays).Wait();

            counter.Value.Should().Be(4);
        }

        private class RefInt
        {
            public int Value { get; set; }
        }
    }
}

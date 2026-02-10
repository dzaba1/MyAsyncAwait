using FluentAssertions;
using NUnit.Framework;

namespace Dzaba.AsyncAwait.Tests
{
    [TestFixture]
    public class MyTaskTests
    {
        [Test]
        public void Wait_WhenCalled_ThenItWaits()
        {
            var finished = false;
            var task = MyTask.Run(() => { finished = true; });
            task.Wait();

            finished.Should().BeTrue();
        }

        [Test]
        public void Wait_WhenError_ThenException()
        {
            var task = MyTask.Run(() => throw new Exception("Test"));

            this.Invoking(_ => task.Wait()).Should().Throw<Exception>();
        }

        [Test]
        public void ContinueWith_WhenDelay_ThenICanWait()
        {
            var finished = false;
            MyTask.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith(() => finished = true)
                .Wait();

            finished.Should().BeTrue();
        }

        [Test]
        public void ContinueWith_WhenMultipleInvocations_ThenICanWait()
        {
            var counter = 0;
            MyTask.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith(() =>
                {
                    counter++;
                    return MyTask.Delay(TimeSpan.FromSeconds(1)).ContinueWith(() =>
                    {
                        counter++;
                        return MyTask.Delay(TimeSpan.FromSeconds(1)).ContinueWith(() =>
                        {
                            counter++;
                            return MyTask.Delay(TimeSpan.FromSeconds(1)).ContinueWith(() =>
                            {
                                counter++;
                            });
                        });
                    });
                })
                .Wait();

            counter.Should().Be(4);
        }

        private static IEnumerable<MyTask> DelayAndIncrement(int delayCount, RefInt value)
        {
            for (int i = 0; i < delayCount; i++)
            {
                yield return MyTask.Delay(TimeSpan.FromSeconds(1));
                value.Value++;
            }
        }

        [Test]
        public void Iterate_WhenEnumerableTasks_ThenWait()
        {
            var counter = new RefInt();
            var delays = DelayAndIncrement(4, counter);
            MyTask.Iterate(delays).Wait();

            counter.Value.Should().Be(4);
        }

        private static async Task DelayAndIncrementAsync(int delayCount, RefInt value)
        {
            for (int i = 0; i < delayCount; i++)
            {
                await MyTask.Delay(TimeSpan.FromSeconds(1));
                value.Value++;
            }
        }

        [Test]
        public void AsyncAwait_WhenTask_ThenWait()
        {
            var counter = new RefInt();
            DelayAndIncrementAsync(4, counter).Wait();

            counter.Value.Should().Be(4);
        }

        [Test]
        public async Task AsyncAwait_WhenNormalAsync_ThenAwait()
        {
            var counter = new RefInt();
            await DelayAndIncrementAsync(4, counter);

            counter.Value.Should().Be(4);
        }

        [Test]
        public async MyTask AsyncAwait_WhenAsync_ThenAwait()
        {
            var counter = new RefInt();
            await DelayAndIncrementAsync(4, counter);

            counter.Value.Should().Be(4);
        }

        private class RefInt
        {
            public int Value { get; set; }
        }
    }
}

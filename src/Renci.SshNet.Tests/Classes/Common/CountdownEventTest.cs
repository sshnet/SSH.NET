using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if !FEATURE_THREAD_COUNTDOWNEVENT
using CountdownEvent = Renci.SshNet.Common.CountdownEvent;
#endif

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class CountdownEventTest
    {
        private Random _random;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
        }

        [TestMethod]
        public void Ctor_InitialCountGreatherThanZero()
        {
            var initialCount = _random.Next(1, 500);

            var countdownEvent = CreateCountdownEvent(initialCount);
            Assert.AreEqual(initialCount, countdownEvent.CurrentCount);
            Assert.IsFalse(countdownEvent.IsSet);
            Assert.IsFalse(countdownEvent.WaitHandle.WaitOne(0));
            countdownEvent.Dispose();
        }

        [TestMethod]
        public void Ctor_InitialCountZero()
        {
            const int initialCount = 0;

            var countdownEvent = CreateCountdownEvent(0);
            Assert.AreEqual(initialCount, countdownEvent.CurrentCount);
            Assert.IsTrue(countdownEvent.IsSet);
            Assert.IsTrue(countdownEvent.WaitHandle.WaitOne(0));
            countdownEvent.Dispose();
        }

        [TestMethod]
        public void Signal_CurrentCountGreatherThanOne()
        {
            var initialCount = _random.Next(2, 1000);

            var countdownEvent = CreateCountdownEvent(initialCount);
            Assert.IsFalse(countdownEvent.Signal());
            Assert.AreEqual(--initialCount, countdownEvent.CurrentCount);
            Assert.IsFalse(countdownEvent.IsSet);
            Assert.IsFalse(countdownEvent.WaitHandle.WaitOne(0));
            countdownEvent.Dispose();
        }

        [TestMethod]
        public void Signal_CurrentCountOne()
        {
            var countdownEvent = CreateCountdownEvent(1);
            Assert.IsTrue(countdownEvent.Signal());
            Assert.AreEqual(0, countdownEvent.CurrentCount);
            Assert.IsTrue(countdownEvent.IsSet);
            Assert.IsTrue(countdownEvent.WaitHandle.WaitOne(0));
            countdownEvent.Dispose();
        }

        [TestMethod]
        public void Signal_CurrentCountZero()
        {
            var countdownEvent = CreateCountdownEvent(0);

            try
            {
                countdownEvent.Signal();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                // Invalid attempt made to decrement the event's count below zero
            }
            finally
            {
                countdownEvent.Dispose();
            }
        }

        [TestMethod]
        public void Wait_TimeoutInfinite_ShouldBlockUntilCountdownEventIsSet()
        {
            var sleep = TimeSpan.FromMilliseconds(100);
            var timeout = Session.InfiniteTimeSpan;

            var countdownEvent = CreateCountdownEvent(1);
            var signalCount = 0;
            var expectedSignalCount = _random.Next(5, 20);

            for (var i = 0; i < (expectedSignalCount - 1); i++)
                countdownEvent.AddCount();

            var threads = new Thread[expectedSignalCount];
            for (var i = 0; i < expectedSignalCount; i++)
            {
                threads[i] = new Thread(() =>
                    {
                        Thread.Sleep(sleep);
                        Interlocked.Increment(ref signalCount);
                        countdownEvent.Signal();
                    });
                threads[i].Start();
            }

            var start = DateTime.Now;
            var actual = countdownEvent.Wait(timeout);
            var elapsedTime = DateTime.Now - start;

            Assert.IsTrue(actual);
            Assert.AreEqual(expectedSignalCount, signalCount);
            Assert.IsTrue(countdownEvent.IsSet);
            Assert.IsTrue(countdownEvent.WaitHandle.WaitOne(0));
            Assert.IsTrue(elapsedTime >= sleep);
            Assert.IsTrue(elapsedTime <= sleep.Add(TimeSpan.FromMilliseconds(100)));

            countdownEvent.Dispose();
        }

        [TestMethod]
        public void Wait_ShouldReturnTrueWhenCountdownEventIsSetBeforeTimeoutExpires()
        {
            var sleep = TimeSpan.FromMilliseconds(100);
            var timeout = sleep.Add(TimeSpan.FromSeconds(2));

            var countdownEvent = CreateCountdownEvent(1);
            var signalCount = 0;
            var expectedSignalCount = _random.Next(5, 20);

            for (var i = 0; i < (expectedSignalCount - 1); i++)
                countdownEvent.AddCount();

            var threads = new Thread[expectedSignalCount];
            for (var i = 0; i < expectedSignalCount; i++)
            {
                threads[i] = new Thread(() =>
                {
                    Thread.Sleep(sleep);
                    Interlocked.Increment(ref signalCount);
                    countdownEvent.Signal();
                });
                threads[i].Start();
            }

            var start = DateTime.Now;
            var actual = countdownEvent.Wait(timeout);
            var elapsedTime = DateTime.Now - start;

            Assert.IsTrue(actual);
            Assert.AreEqual(expectedSignalCount, signalCount);
            Assert.IsTrue(countdownEvent.IsSet);
            Assert.IsTrue(countdownEvent.WaitHandle.WaitOne(0));
            Assert.IsTrue(elapsedTime >= sleep);
            Assert.IsTrue(elapsedTime <= timeout);

            countdownEvent.Dispose();
        }

        [TestMethod]
        public void Wait_ShouldReturnFalseWhenTimeoutExpiresBeforeCountdownEventIsSet()
        {
            var sleep = TimeSpan.FromMilliseconds(100);
            var timeout = TimeSpan.FromMilliseconds(30);

            var countdownEvent = CreateCountdownEvent(1);
            var signalCount = 0;
            var expectedSignalCount = _random.Next(5, 20);

            for (var i = 0; i < (expectedSignalCount - 1); i++)
                countdownEvent.AddCount();

            var threads = new Thread[expectedSignalCount];
            for (var i = 0; i < expectedSignalCount; i++)
            {
                threads[i] = new Thread(() =>
                {
                    Thread.Sleep(sleep);
                    countdownEvent.Signal();
                    Interlocked.Increment(ref signalCount);
                });
                threads[i].Start();
            }

            var start = DateTime.Now;
            var actual = countdownEvent.Wait(timeout);
            var elapsedTime = DateTime.Now - start;

            Assert.IsFalse(actual);
            Assert.IsFalse(countdownEvent.IsSet);
            Assert.IsFalse(countdownEvent.WaitHandle.WaitOne(0));
            Assert.IsTrue(elapsedTime >= timeout);

            countdownEvent.Wait(Session.InfiniteTimeSpan);
            countdownEvent.Dispose();
        }

        [TestMethod]
        public void WaitHandle_ShouldAlwaysReturnSameInstance()
        {
            var countdownEvent = CreateCountdownEvent(1);

            var waitHandleA = countdownEvent.WaitHandle;
            Assert.IsNotNull(waitHandleA);

            var waitHandleB = countdownEvent.WaitHandle;
            Assert.AreSame(waitHandleA, waitHandleB);
        }

        [TestMethod]
        public void WaitHandle_WaitOne_TimeoutInfinite_ShouldBlockUntilCountdownEventIsSet()
        {
            var sleep = TimeSpan.FromMilliseconds(100);
            var timeout = Session.InfiniteTimeSpan;

            var countdownEvent = CreateCountdownEvent(1);
            var signalCount = 0;
            var expectedSignalCount = _random.Next(5, 20);

            for (var i = 0; i < (expectedSignalCount - 1); i++)
                countdownEvent.AddCount();

            var threads = new Thread[expectedSignalCount];
            for (var i = 0; i < expectedSignalCount; i++)
            {
                threads[i] = new Thread(() =>
                {
                    Thread.Sleep(sleep);
                    Interlocked.Increment(ref signalCount);
                    countdownEvent.Signal();
                });
                threads[i].Start();
            }

            var start = DateTime.Now;
            var actual = countdownEvent.WaitHandle.WaitOne(timeout);
            var elapsedTime = DateTime.Now - start;

            Assert.IsTrue(actual);
            Assert.AreEqual(expectedSignalCount, signalCount);
            Assert.IsTrue(countdownEvent.IsSet);
            Assert.IsTrue(countdownEvent.WaitHandle.WaitOne(0));
            Assert.IsTrue(elapsedTime >= sleep);
            Assert.IsTrue(elapsedTime <= sleep.Add(TimeSpan.FromMilliseconds(100)));

            countdownEvent.Dispose();
        }

        [TestMethod]
        public void WaitHandle_WaitOne_ShouldReturnTrueWhenCountdownEventIsSetBeforeTimeoutExpires()
        {
            var sleep = TimeSpan.FromMilliseconds(100);
            var timeout = sleep.Add(TimeSpan.FromSeconds(2));

            var countdownEvent = CreateCountdownEvent(1);
            var signalCount = 0;
            var expectedSignalCount = _random.Next(5, 20);

            for (var i = 0; i < (expectedSignalCount - 1); i++)
                countdownEvent.AddCount();

            var threads = new Thread[expectedSignalCount];
            for (var i = 0; i < expectedSignalCount; i++)
            {
                threads[i] = new Thread(() =>
                {
                    Thread.Sleep(sleep);
                    Interlocked.Increment(ref signalCount);
                    countdownEvent.Signal();
                });
                threads[i].Start();
            }

            var start = DateTime.Now;
            var actual = countdownEvent.Wait(timeout);
            var elapsedTime = DateTime.Now - start;

            Assert.IsTrue(actual);
            Assert.AreEqual(expectedSignalCount, signalCount);
            Assert.IsTrue(countdownEvent.IsSet);
            Assert.IsTrue(countdownEvent.WaitHandle.WaitOne(0));
            Assert.IsTrue(elapsedTime >= sleep);
            Assert.IsTrue(elapsedTime <= timeout);

            countdownEvent.Dispose();
        }

        [TestMethod]
        public void WaitHandle_WaitOne_ShouldReturnFalseWhenTimeoutExpiresBeforeCountdownEventIsSet()
        {
            var sleep = TimeSpan.FromMilliseconds(100);
            var timeout = TimeSpan.FromMilliseconds(30);

            var countdownEvent = CreateCountdownEvent(1);
            var signalCount = 0;
            var expectedSignalCount = _random.Next(5, 20);

            for (var i = 0; i < (expectedSignalCount - 1); i++)
                countdownEvent.AddCount();

            var threads = new Thread[expectedSignalCount];
            for (var i = 0; i < expectedSignalCount; i++)
            {
                threads[i] = new Thread(() =>
                {
                    Thread.Sleep(sleep);
                    countdownEvent.Signal();
                    Interlocked.Increment(ref signalCount);
                });
                threads[i].Start();
            }

            var start = DateTime.Now;
            var actual = countdownEvent.WaitHandle.WaitOne(timeout);
            var elapsedTime = DateTime.Now - start;

            Assert.IsFalse(actual);
            Assert.IsFalse(countdownEvent.IsSet);
            Assert.IsFalse(countdownEvent.WaitHandle.WaitOne(0));
            Assert.IsTrue(elapsedTime >= timeout);

            countdownEvent.Wait(Session.InfiniteTimeSpan);
            countdownEvent.Dispose();
        }

        private static CountdownEvent CreateCountdownEvent(int initialCount)
        {
            return new CountdownEvent(initialCount);
        }
    }
}

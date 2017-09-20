using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class SemaphoreLightTest : TestBase
    {
        [TestMethod]
        public void SemaphoreLightConstructorTest()
        {
            var initialCount = new Random().Next(1, 10);
            var target = new SemaphoreLight(initialCount);
            Assert.AreEqual(initialCount, target.CurrentCount);
        }

        [TestMethod]
        public void Release()
        {
            var initialCount = new Random().Next(1, 10);
            var target = new SemaphoreLight(initialCount);

            Assert.AreEqual(initialCount, target.Release());
            Assert.AreEqual(initialCount + 1, target.CurrentCount);

            Assert.AreEqual(initialCount + 1, target.Release());
            Assert.AreEqual(initialCount + 2, target.CurrentCount);
        }

        /// <summary>
        ///A test for Release
        ///</summary>
        [TestMethod]
        public void Release_ReleaseCount()
        {
            var initialCount = new Random().Next(1, 10);
            var target = new SemaphoreLight(initialCount);

            var releaseCount1 = new Random().Next(1, 10);
            Assert.AreEqual(initialCount, target.Release(releaseCount1));
            Assert.AreEqual(initialCount + releaseCount1, target.CurrentCount);

            var releaseCount2 = new Random().Next(1, 10);
            Assert.AreEqual(initialCount + releaseCount1, target.Release(releaseCount2));
            Assert.AreEqual(initialCount + releaseCount1 + releaseCount2, target.CurrentCount);
        }

        /// <summary>
        ///A test for Wait
        ///</summary>
        [TestMethod]
        public void WaitTest()
        {
            const int sleepTime = 200; 
            const int initialCount = 2;
            var target = new SemaphoreLight(initialCount);

            var start = DateTime.Now;

            target.Wait();
            target.Wait();

            Assert.IsTrue((DateTime.Now - start).TotalMilliseconds < 50);

            var releaseThread = new Thread(
                () =>
                    {
                        Thread.Sleep(sleepTime);
                        target.Release();
                    });
            releaseThread.Start();

            target.Wait();

            var end = DateTime.Now;
            var elapsed = end - start;

            Assert.IsTrue(elapsed.TotalMilliseconds > 200);
            Assert.IsTrue(elapsed.TotalMilliseconds < 250);
        }

        [TestMethod]
        public void CurrentCountTest()
        {
            var initialCount = new Random().Next(1, 20);
            var target = new SemaphoreLight(initialCount);

            Assert.AreEqual(initialCount, target.CurrentCount);
        }
    }
}

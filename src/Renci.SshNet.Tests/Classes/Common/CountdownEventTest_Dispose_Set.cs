using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if !FEATURE_THREAD_COUNTDOWNEVENT
using CountdownEvent = Renci.SshNet.Common.CountdownEvent;
#else
using System.Threading;
#endif

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class CountdownEventTest_Dispose_Set
    {
        private CountdownEvent _countdownEvent;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        private void Arrange()
        {
            _countdownEvent = new CountdownEvent(0);
        }

        private void Act()
        {
            _countdownEvent.Dispose();
        }

        [TestMethod]
        public void AddCount_ShouldThrowObjectDisposedException()
        {
            try
            {
                _countdownEvent.AddCount();
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [TestMethod]
        public void CurrentCount_ShouldReturnZero()
        {
            var actual = _countdownEvent.CurrentCount;

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void Dispose_ShouldNotThrow()
        {
            _countdownEvent.Dispose();
        }

        [TestMethod]
        public void IsSet_ShouldReturnTrue()
        {
            var actual = _countdownEvent.IsSet;

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Signal_ShouldThrowObjectDisposedException()
        {
            try
            {
                var set = _countdownEvent.Signal();
                Assert.Fail("Should have thrown ObjectDisposedException, but returned: " + set);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [TestMethod]
        public void Wait_TimeSpan_ShouldThrowObjectDisposedException()
        {
            try
            {
                var set = _countdownEvent.Wait(TimeSpan.FromSeconds(5));
                Assert.Fail("Should have thrown ObjectDisposedException, but returned: " + set);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [TestMethod]
        public void WaitHandle_ShouldThrowObjectDisposedException()
        {
            try
            {
                var waitHandle = _countdownEvent.WaitHandle;
                Assert.Fail("Should have thrown ObjectDisposedException, but returned: " + waitHandle);
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}

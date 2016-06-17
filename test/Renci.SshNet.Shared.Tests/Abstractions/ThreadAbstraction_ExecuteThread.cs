using System;
using System.Threading;
#if SILVERLIGHT
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Tests.Abstractions
{
    [TestClass]
    public class ThreadAbstraction_ExecuteThread
    {
        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenActionIsNull()
        {
            const Action action = null;

            try
            {
                ThreadAbstraction.ExecuteThread(action);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("action", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldExecuteActionOnSeparateThread()
        {
            DateTime? executionTime = null;
            int executionCount = 0;
            EventWaitHandle waitHandle = new ManualResetEvent(false);

            Action action = () =>
            {
                ThreadAbstraction.Sleep(500);
                executionCount++;
                executionTime = DateTime.Now;
                waitHandle.Set();
            };

            DateTime start = DateTime.Now;

            ThreadAbstraction.ExecuteThread(action);

            Assert.AreEqual(0, executionCount);
            Assert.IsNull(executionTime);

            Assert.IsTrue(waitHandle.WaitOne(2000));

            Assert.AreEqual(1, executionCount);
            Assert.IsNotNull(executionTime);

            var elapsedTime = executionTime.Value - start;
            Assert.IsTrue(elapsedTime > TimeSpan.Zero);
            Assert.IsTrue(elapsedTime > TimeSpan.FromMilliseconds(500));
            Assert.IsTrue(elapsedTime < TimeSpan.FromMilliseconds(1000));
        }
    }
}

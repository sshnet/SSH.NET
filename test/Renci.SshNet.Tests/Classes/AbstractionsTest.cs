using Renci.SshNet.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class AbstractionsTest
    {
        [TestMethod]
        public void CryptoAbstraction_GenerateRandom_ShouldPerformNoOpWhenDataIsZeroLength()
        {
            Assert.AreEqual(0, CryptoAbstraction.GenerateRandom(0).Length);
        }

        [TestMethod]
        public void CryptoAbstraction_GenerateRandom_ShouldGenerateRandomSequenceOfValues()
        {
            var dataLength = new Random().Next(1, 100);

            var dataA = CryptoAbstraction.GenerateRandom(dataLength);
            var dataB = CryptoAbstraction.GenerateRandom(dataLength);

            Assert.AreEqual(dataLength, dataA.Length);
            Assert.AreEqual(dataLength, dataB.Length);

            CollectionAssert.AreNotEqual(dataA, dataB);
        }

        [TestMethod]
        public void ThreadAbstraction_ExecuteThread_ShouldThrowArgumentNullExceptionWhenActionIsNull()
        {
            var ex = Assert.ThrowsExactly<ArgumentNullException>(() => ThreadAbstraction.ExecuteThread(null));

            Assert.IsNull(ex.InnerException);
            Assert.AreEqual("action", ex.ParamName);
        }

        [TestMethod]
        public void ThreadAbstraction_ExecuteThread_ShouldExecuteActionOnSeparateThread()
        {
            int threadId = 0;
            using var waitHandle = new ManualResetEventSlim();

            ThreadAbstraction.ExecuteThread(() =>
            {
                threadId = Environment.CurrentManagedThreadId;
                waitHandle.Set();
            });

            Assert.IsTrue(waitHandle.Wait(1000));
            Assert.AreNotEqual(0, threadId);
            Assert.AreNotEqual(Environment.CurrentManagedThreadId, threadId);
        }
    }
}

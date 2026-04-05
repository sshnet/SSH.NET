using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Abstractions;

using System;
using System.Threading;
using System.Security.Cryptography;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class AbstractionsTest
    {
        [TestMethod]
        public void CryptoAbstraction_GenerateRandom_ShouldPerformNoOpWhenDataIsZeroLength()
        {
            Assert.IsEmpty(RandomNumberGenerator.GetBytes(0));
        }

        [TestMethod]
        public void CryptoAbstraction_GenerateRandom_ShouldGenerateRandomSequenceOfValues()
        {
            var dataLength = new Random().Next(1, 100);

            var dataA = RandomNumberGenerator.GetBytes(dataLength);
            var dataB = RandomNumberGenerator.GetBytes(dataLength);

            Assert.HasCount(dataLength, dataA);
            Assert.HasCount(dataLength, dataB);

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

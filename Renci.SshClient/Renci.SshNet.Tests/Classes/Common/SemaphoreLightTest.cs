using Renci.SshNet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for SemaphoreLightTest and is intended
    ///to contain all SemaphoreLightTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SemaphoreLightTest : TestBase
    {
        /// <summary>
        ///A test for SemaphoreLight Constructor
        ///</summary>
        [TestMethod()]
        public void SemaphoreLightConstructorTest()
        {
            int initialCount = 0; // TODO: Initialize to an appropriate value
            SemaphoreLight target = new SemaphoreLight(initialCount);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Release
        ///</summary>
        [TestMethod()]
        public void ReleaseTest()
        {
            int initialCount = 0; // TODO: Initialize to an appropriate value
            SemaphoreLight target = new SemaphoreLight(initialCount); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.Release();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Release
        ///</summary>
        [TestMethod()]
        public void ReleaseTest1()
        {
            int initialCount = 0; // TODO: Initialize to an appropriate value
            SemaphoreLight target = new SemaphoreLight(initialCount); // TODO: Initialize to an appropriate value
            int releaseCount = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.Release(releaseCount);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Wait
        ///</summary>
        [TestMethod()]
        public void WaitTest()
        {
            int initialCount = 0; // TODO: Initialize to an appropriate value
            SemaphoreLight target = new SemaphoreLight(initialCount); // TODO: Initialize to an appropriate value
            target.Wait();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for CurrentCount
        ///</summary>
        [TestMethod()]
        public void CurrentCountTest()
        {
            int initialCount = 0; // TODO: Initialize to an appropriate value
            SemaphoreLight target = new SemaphoreLight(initialCount); // TODO: Initialize to an appropriate value
            int actual;
            actual = target.CurrentCount;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}

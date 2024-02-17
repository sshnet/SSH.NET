using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class TimeSpanExtensionsTest
    {
        [TestMethod]
        public void AsTimeout_ValidTimeSpan_ReturnsExpectedMilliseconds()
        {
            var timeSpan = TimeSpan.FromSeconds(10);

            var timeout = timeSpan.AsTimeout("TestMethodName");

            Assert.AreEqual(10000, timeout);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AsTimeout_NegativeTimeSpan_ThrowsArgumentOutOfRangeException()
        {
            var timeSpan = TimeSpan.FromSeconds(-1);

            timeSpan.AsTimeout("TestMethodName");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AsTimeout_TimeSpanExceedingMaxValue_ThrowsArgumentOutOfRangeException()
        {
            var timeSpan = TimeSpan.FromMilliseconds((double)int.MaxValue + 1);

            timeSpan.AsTimeout("TestMethodName");
        }

        [TestMethod]
        public void EnsureValidTimeout_ValidTimeSpan_DoesNotThrow()
        {
            var timeSpan = TimeSpan.FromSeconds(5);

            timeSpan.EnsureValidTimeout("TestMethodName");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void EnsureValidTimeout_NegativeTimeSpan_ThrowsArgumentOutOfRangeException()
        {
            var timeSpan = TimeSpan.FromSeconds(-1);

            timeSpan.EnsureValidTimeout("TestMethodName");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void EnsureValidTimeout_TimeSpanExceedingMaxValue_ThrowsArgumentOutOfRangeException()
        {
            var timeSpan = TimeSpan.FromMilliseconds((double)int.MaxValue + 1);

            timeSpan.EnsureValidTimeout("TestMethodName");
        }
    }
}

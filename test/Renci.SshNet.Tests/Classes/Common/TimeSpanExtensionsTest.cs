using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

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
            var timeSpan = TimeSpan.FromMilliseconds((double) int.MaxValue + 1);

            timeSpan.AsTimeout("TestMethodName");
        }

        [TestMethod]
        public void AsTimeout_ArgumentOutOfRangeException_HasCorrectInformation()
        {

            try
            {
                var timeSpan = TimeSpan.FromMilliseconds((double) int.MaxValue + 1);

                timeSpan.AsTimeout("TestMethodName");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                ArgumentExceptionAssert.MessageEquals("The timeout must represent a value between -1 and Int32.MaxValue milliseconds, inclusive.", ex);
                Assert.AreEqual("TestMethodName", ex.ParamName);
            }
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
            var timeSpan = TimeSpan.FromMilliseconds((double) int.MaxValue + 1);

            timeSpan.EnsureValidTimeout("TestMethodName");
        }

        [TestMethod]
        public void EnsureValidTimeout_ArgumentOutOfRangeException_HasCorrectInformation()
        {

            try
            {
                var timeSpan = TimeSpan.FromMilliseconds((double) int.MaxValue + 1);

                timeSpan.EnsureValidTimeout("TestMethodName");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                ArgumentExceptionAssert.MessageEquals("The timeout must represent a value between -1 and Int32.MaxValue milliseconds, inclusive.", ex);
                Assert.AreEqual("TestMethodName", ex.ParamName);
            }
        }
    }
}

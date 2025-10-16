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

            var timeout = timeSpan.AsTimeout();

            Assert.AreEqual(10000, timeout);
        }

        [TestMethod]
        [DataRow(-2)]
        [DataRow((double)int.MaxValue + 1)]
        public void AsTimeout_InvalidTimeSpan_ThrowsArgumentOutOfRangeException(double milliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(milliseconds);

            var ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => timeSpan.AsTimeout());

            Assert.Contains("The timeout must represent a value between -1 and Int32.MaxValue milliseconds, inclusive.", ex.Message, StringComparison.Ordinal);

            Assert.AreEqual(nameof(timeSpan), ex.ParamName);
        }

        [TestMethod]
        public void EnsureValidTimeout_ValidTimeSpan_DoesNotThrow()
        {
            var timeSpan = TimeSpan.FromSeconds(5);

            timeSpan.EnsureValidTimeout();
        }

        [TestMethod]
        [DataRow(-2)]
        [DataRow((double)int.MaxValue + 1)]
        public void EnsureValidTimeout_InvalidTimeSpan_ThrowsArgumentOutOfRangeException(double milliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(milliseconds);

            var ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => timeSpan.EnsureValidTimeout());

            Assert.Contains("The timeout must represent a value between -1 and Int32.MaxValue milliseconds, inclusive.", ex.Message, StringComparison.Ordinal);

            Assert.AreEqual(nameof(timeSpan), ex.ParamName);
        }
    }
}

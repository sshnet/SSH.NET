using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class NetConfClientTest : TestBase
    {
        private Random _random;

        [TestInitialize]
        public void SetUp()
        {
            _random = new Random();
        }

        [TestMethod]
        public void OperationTimeout_Default()
        {
            using (var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd"))
            using (var target = new NetConfClient(connectionInfo))
            {
                var actual = target.OperationTimeout;

                Assert.AreEqual(TimeSpan.FromMilliseconds(-1), actual);
            }
        }

        [TestMethod]
        public void OperationTimeout_InsideLimits()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(_random.Next(0, int.MaxValue - 1));

            using (var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd"))
            using (var target = new NetConfClient(connectionInfo))
            {
                target.OperationTimeout = operationTimeout;

                var actual = target.OperationTimeout;

                Assert.AreEqual(operationTimeout, actual);
            }
        }

        [TestMethod]
        public void OperationTimeout_LowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(-1);

            using (var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd"))
            using (var target = new NetConfClient(connectionInfo))
            {
                target.OperationTimeout = operationTimeout;

                var actual = target.OperationTimeout;

                Assert.AreEqual(operationTimeout, actual);
            }
        }

        [TestMethod]
        public void OperationTimeout_UpperLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(int.MaxValue);

            using (var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd"))
            using (var target = new NetConfClient(connectionInfo))
            {
                target.OperationTimeout = operationTimeout;

                var actual = target.OperationTimeout;

                Assert.AreEqual(operationTimeout, actual);
            }
        }

        [TestMethod]
        public void OperationTimeout_LessThanLowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(-2);

            using (var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd"))
            using (var target = new NetConfClient(connectionInfo))
            {
                try
                {
                    target.OperationTimeout = operationTimeout;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    ArgumentExceptionAssert.MessageEquals("The timeout must represent a value between -1 and Int32.MaxValue, inclusive.", ex);
                    Assert.AreEqual("value", ex.ParamName);
                }
            }
        }

        [TestMethod]
        public void OperationTimeout_GreaterThanLowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(int.MaxValue).Add(TimeSpan.FromMilliseconds(1));

            using (var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd"))
            using (var target = new NetConfClient(connectionInfo))
            {
                try
                {
                    target.OperationTimeout = operationTimeout;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    ArgumentExceptionAssert.MessageEquals("The timeout must represent a value between -1 and Int32.MaxValue, inclusive.", ex);
                    Assert.AreEqual("value", ex.ParamName);
                }
            }
        }
    }
}

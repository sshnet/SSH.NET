using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    [TestClass]
    public partial class SftpClientTest : TestBase
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
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo);

            var actual = target.OperationTimeout;

            Assert.AreEqual(TimeSpan.FromMilliseconds(-1), actual);
        }

        [TestMethod]
        public void OperationTimeout_InsideLimits()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(_random.Next(0, int.MaxValue - 1));
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo)
                {
                    OperationTimeout = operationTimeout
                };

            var actual = target.OperationTimeout;

            Assert.AreEqual(operationTimeout, actual);
        }

        [TestMethod]
        public void OperationTimeout_LowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(-1);
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo)
                {
                    OperationTimeout = operationTimeout
                };

            var actual = target.OperationTimeout;

            Assert.AreEqual(operationTimeout, actual);
        }

        [TestMethod]
        public void OperationTimeout_UpperLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(int.MaxValue);
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo)
                {
                    OperationTimeout = operationTimeout
                };

            var actual = target.OperationTimeout;

            Assert.AreEqual(operationTimeout, actual);
        }

        [TestMethod]
        public void OperationTimeout_LessThanLowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(-2);
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo);

            try
            {
                target.OperationTimeout = operationTimeout;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                ArgumentExceptionAssert.MessageEquals("The timeout must represent a value between -1 and Int32.MaxValue milliseconds, inclusive.", ex);

                Assert.AreEqual("OperationTimeout", ex.ParamName);
            }
        }

        [TestMethod]
        public void OperationTimeout_GreaterThanLowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(int.MaxValue).Add(TimeSpan.FromMilliseconds(1));
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo);

            try
            {
                target.OperationTimeout = operationTimeout;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                ArgumentExceptionAssert.MessageEquals("The timeout must represent a value between -1 and Int32.MaxValue milliseconds, inclusive.", ex);

                Assert.AreEqual("OperationTimeout", ex.ParamName);
            }
        }

        [TestMethod]
        public void OperationTimeout_Disposed()
        {
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo);
            target.Dispose();

            // getter
            try
            {
                var actual = target.OperationTimeout;
                Assert.Fail("Should have failed, but returned: " + actual);
            }
            catch (ObjectDisposedException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(typeof(SftpClient).FullName, ex.ObjectName);
            }

            // setter
            try
            {
                target.OperationTimeout = TimeSpan.FromMilliseconds(5);
                Assert.Fail();
            }
            catch (ObjectDisposedException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(typeof(SftpClient).FullName, ex.ObjectName);
            }
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.Net;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides connection information when password authentication method is used
    /// </summary>
    [TestClass]
    public class PasswordConnectionInfoTest : TestBase
    {
        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        public void Test_ConnectionInfo_Host_Is_Null()
        {
            try
            {
                _ = new PasswordConnectionInfo(null, Resources.USERNAME, Resources.PASSWORD);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("host", ex.ParamName);
            }

        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Username_Is_Null()
        {
            _ = new PasswordConnectionInfo(Resources.HOST, null, Resources.PASSWORD);
        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_ConnectionInfo_Password_Is_Null()
        {
            _ = new PasswordConnectionInfo(Resources.HOST, Resources.USERNAME, (string)null);
        }

        [TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [Description("Test passing whitespace to username parameter.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Username_Is_Whitespace()
        {
            _ = new PasswordConnectionInfo(Resources.HOST, " ", Resources.PASSWORD);
        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_SmallPortNumber()
        {
            _ = new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MinPort - 1, Resources.USERNAME, Resources.PASSWORD);
        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_BigPortNumber()
        {
            _ = new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MaxPort + 1, Resources.USERNAME, Resources.PASSWORD);
        }
        
        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DisposeTest()
        {
            var host = string.Empty; // TODO: Initialize to an appropriate value
            var username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            var target = new PasswordConnectionInfo(host, username, password); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            string proxyPassword = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest1()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            string proxyPassword = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest2()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort, proxyUsername);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest3()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest4()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password, proxyType, proxyHost, proxyPort, proxyUsername);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest5()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password, proxyType, proxyHost, proxyPort);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest6()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest7()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest8()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            string proxyPassword = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest9()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort, proxyUsername);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest10()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest11()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password, proxyType, proxyHost, proxyPort, proxyUsername);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest12()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password, proxyType, proxyHost, proxyPort);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest13()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest14()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

    }
}

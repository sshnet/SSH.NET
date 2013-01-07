using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Represents remote connection information class.
    /// </summary>
    [TestClass]
    public class ConnectionInfoTest : TestBase
    {
        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass null as proxy host.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_ProxyHost_Null()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.Http, null, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too large proxy port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_ProxyPort_Large()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.Http, Resources.HOST, int.MaxValue, Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too small proxy port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_ProxyPort_Small()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.Http, Resources.HOST, int.MinValue, Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass a valid proxy port.")]
        [Owner("Kenneth_aa")]
        public void Test_ConnectionInfo_ProxyPort_Valid()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass null as host.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Host_Null()
        {
            new ConnectionInfo(null, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass a valid host.")]
        [Owner("Kenneth_aa")]
        public void Test_ConnectionInfo_Host_Valid()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too large port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_Port_Large()
        {
            new ConnectionInfo(Resources.HOST, int.MaxValue, Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too small port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_Port_Small()
        {
            new ConnectionInfo(Resources.HOST, int.MinValue, Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass a valid port.")]
        [Owner("Kenneth_aa")]
        public void Test_ConnectionInfo_Port_Valid()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass null as session.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_ConnectionInfo_Authenticate_Null()
        {
            var ret = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
            ret.Authenticate(null);
        }

        /// <summary>
        ///A test for Timeout
        ///</summary>
        [TestMethod()]
        public void TimeoutTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationMethod[] authenticationMethods = null; // TODO: Initialize to an appropriate value
            ConnectionInfo target = new ConnectionInfo(host, username, authenticationMethods); // TODO: Initialize to an appropriate value
            TimeSpan expected = new TimeSpan(); // TODO: Initialize to an appropriate value
            TimeSpan actual;
            target.Timeout = expected;
            actual = target.Timeout;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for RetryAttempts
        ///</summary>
        [TestMethod()]
        public void RetryAttemptsTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationMethod[] authenticationMethods = null; // TODO: Initialize to an appropriate value
            ConnectionInfo target = new ConnectionInfo(host, username, authenticationMethods); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.RetryAttempts = expected;
            actual = target.RetryAttempts;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for MaxSessions
        ///</summary>
        [TestMethod()]
        public void MaxSessionsTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationMethod[] authenticationMethods = null; // TODO: Initialize to an appropriate value
            ConnectionInfo target = new ConnectionInfo(host, username, authenticationMethods); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.MaxSessions = expected;
            actual = target.MaxSessions;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Authenticate
        ///</summary>
        [TestMethod()]
        public void AuthenticateTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationMethod[] authenticationMethods = null; // TODO: Initialize to an appropriate value
            ConnectionInfo target = new ConnectionInfo(host, username, authenticationMethods); // TODO: Initialize to an appropriate value
            Session session = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Authenticate(session);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ConnectionInfo Constructor
        ///</summary>
        [TestMethod()]
        public void ConnectionInfoConstructorTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            string proxyPassword = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationMethod[] authenticationMethods = null; // TODO: Initialize to an appropriate value
            ConnectionInfo target = new ConnectionInfo(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, authenticationMethods);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ConnectionInfo Constructor
        ///</summary>
        [TestMethod()]
        public void ConnectionInfoConstructorTest1()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationMethod[] authenticationMethods = null; // TODO: Initialize to an appropriate value
            ConnectionInfo target = new ConnectionInfo(host, port, username, authenticationMethods);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ConnectionInfo Constructor
        ///</summary>
        [TestMethod()]
        public void ConnectionInfoConstructorTest2()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationMethod[] authenticationMethods = null; // TODO: Initialize to an appropriate value
            ConnectionInfo target = new ConnectionInfo(host, username, authenticationMethods);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides connection information when private key authentication method is used
    /// </summary>
    [TestClass]
    public class PrivateKeyConnectionInfoTest : TestBase
    {        
        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyConnectionInfo target = new PrivateKeyConnectionInfo(host, username, keyFiles); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for PrivateKeyConnectionInfo Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyConnectionInfoConstructorTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            string proxyPassword = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyConnectionInfo target = new PrivateKeyConnectionInfo(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PrivateKeyConnectionInfo Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyConnectionInfoConstructorTest1()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            string proxyPassword = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyConnectionInfo target = new PrivateKeyConnectionInfo(host, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PrivateKeyConnectionInfo Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyConnectionInfoConstructorTest2()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyConnectionInfo target = new PrivateKeyConnectionInfo(host, username, proxyType, proxyHost, proxyPort, proxyUsername, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PrivateKeyConnectionInfo Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyConnectionInfoConstructorTest3()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyConnectionInfo target = new PrivateKeyConnectionInfo(host, username, proxyType, proxyHost, proxyPort, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PrivateKeyConnectionInfo Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyConnectionInfoConstructorTest4()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyConnectionInfo target = new PrivateKeyConnectionInfo(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PrivateKeyConnectionInfo Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyConnectionInfoConstructorTest5()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyConnectionInfo target = new PrivateKeyConnectionInfo(host, port, username, proxyType, proxyHost, proxyPort, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PrivateKeyConnectionInfo Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyConnectionInfoConstructorTest6()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyConnectionInfo target = new PrivateKeyConnectionInfo(host, port, username, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PrivateKeyConnectionInfo Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyConnectionInfoConstructorTest7()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyConnectionInfo target = new PrivateKeyConnectionInfo(host, username, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System.IO;
using System.Text;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides connection information when private key authentication method is used
    /// </summary>
    [TestClass]
    public class PrivateKeyConnectionInfoTest : TestBase
    {
        [TestMethod]
        [TestCategory("PrivateKeyConnectionInfo")]
        [TestCategory("integration")]
        public void Test_PrivateKeyConnectionInfo()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITHOUT_PASS));

            #region Example PrivateKeyConnectionInfo PrivateKeyFile
            var connectionInfo = new PrivateKeyConnectionInfo(host, username, new PrivateKeyFile(keyFileStream));
            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
            #endregion

            Assert.AreEqual(connectionInfo.Host, Resources.HOST);
            Assert.AreEqual(connectionInfo.Username, Resources.USERNAME);
        }

        [TestMethod]
        [TestCategory("PrivateKeyConnectionInfo")]
        [TestCategory("integration")]
        public void Test_PrivateKeyConnectionInfo_MultiplePrivateKey()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            MemoryStream keyFileStream1 = new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITHOUT_PASS));
            MemoryStream keyFileStream2 = new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITHOUT_PASS));

            #region Example PrivateKeyConnectionInfo PrivateKeyFile Multiple
            var connectionInfo = new PrivateKeyConnectionInfo(host, username, 
                new PrivateKeyFile(keyFileStream1), 
                new PrivateKeyFile(keyFileStream2));
            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
            #endregion

            Assert.AreEqual(connectionInfo.Host, Resources.HOST);
            Assert.AreEqual(connectionInfo.Username, Resources.USERNAME);
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
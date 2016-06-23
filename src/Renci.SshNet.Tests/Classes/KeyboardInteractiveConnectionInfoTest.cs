using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides connection information when keyboard interactive authentication method is used
    /// </summary>
    [TestClass]
    public class KeyboardInteractiveConnectionInfoTest : TestBase
    {
        [TestMethod]
        [TestCategory("KeyboardInteractiveConnectionInfo")]
        [TestCategory("integration")]
        public void Test_KeyboardInteractiveConnectionInfo()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;

            #region Example KeyboardInteractiveConnectionInfo AuthenticationPrompt
            var connectionInfo = new KeyboardInteractiveConnectionInfo(host, username);
            connectionInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
            {
                System.Console.WriteLine(e.Instruction);

                foreach (var prompt in e.Prompts)
                {
                    Console.WriteLine(prompt.Request);
                    prompt.Response = Console.ReadLine();
                }
            };

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                //  Do something here
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
            KeyboardInteractiveConnectionInfo target = new KeyboardInteractiveConnectionInfo(host, username); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for KeyboardInteractiveConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyboardInteractiveConnectionInfoConstructorTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            string proxyPassword = string.Empty; // TODO: Initialize to an appropriate value
            KeyboardInteractiveConnectionInfo target = new KeyboardInteractiveConnectionInfo(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for KeyboardInteractiveConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyboardInteractiveConnectionInfoConstructorTest1()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            string proxyPassword = string.Empty; // TODO: Initialize to an appropriate value
            KeyboardInteractiveConnectionInfo target = new KeyboardInteractiveConnectionInfo(host, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for KeyboardInteractiveConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyboardInteractiveConnectionInfoConstructorTest2()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            KeyboardInteractiveConnectionInfo target = new KeyboardInteractiveConnectionInfo(host, username, proxyType, proxyHost, proxyPort, proxyUsername);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for KeyboardInteractiveConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyboardInteractiveConnectionInfoConstructorTest3()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            KeyboardInteractiveConnectionInfo target = new KeyboardInteractiveConnectionInfo(host, username, proxyType, proxyHost, proxyPort);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for KeyboardInteractiveConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyboardInteractiveConnectionInfoConstructorTest4()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            KeyboardInteractiveConnectionInfo target = new KeyboardInteractiveConnectionInfo(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for KeyboardInteractiveConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyboardInteractiveConnectionInfoConstructorTest5()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            KeyboardInteractiveConnectionInfo target = new KeyboardInteractiveConnectionInfo(host, port, username, proxyType, proxyHost, proxyPort);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for KeyboardInteractiveConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyboardInteractiveConnectionInfoConstructorTest6()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            KeyboardInteractiveConnectionInfo target = new KeyboardInteractiveConnectionInfo(host, port, username);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for KeyboardInteractiveConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyboardInteractiveConnectionInfoConstructorTest7()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            KeyboardInteractiveConnectionInfo target = new KeyboardInteractiveConnectionInfo(host, username);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
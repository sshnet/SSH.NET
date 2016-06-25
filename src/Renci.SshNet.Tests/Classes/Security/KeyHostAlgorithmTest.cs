using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes.Security
{
    /// <summary>
    /// Implements key support for host algorithm.
    /// </summary>
    [TestClass]
    public class KeyHostAlgorithmTest : TestBase
    {
        [TestMethod]
        [TestCategory("integration")]
        public void Test_HostKey_SshRsa_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HostKeyAlgorithms.Clear();
            connectionInfo.HostKeyAlgorithms.Add("ssh-rsa", (data) => { return new KeyHostAlgorithm("ssh-rsa", new RsaKey(), data); });

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_HostKey_SshDss_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HostKeyAlgorithms.Clear();
            connectionInfo.HostKeyAlgorithms.Add("ssh-dss", (data) => { return new KeyHostAlgorithm("ssh-dss", new DsaKey(), data); });

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        /// <summary>
        ///A test for KeyHostAlgorithm Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyHostAlgorithmConstructorTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            Key key = null; // TODO: Initialize to an appropriate value
            KeyHostAlgorithm target = new KeyHostAlgorithm(name, key);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for KeyHostAlgorithm Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyHostAlgorithmConstructorTest1()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            Key key = null; // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            KeyHostAlgorithm target = new KeyHostAlgorithm(name, key, data);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Sign
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SignTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            Key key = null; // TODO: Initialize to an appropriate value
            KeyHostAlgorithm target = new KeyHostAlgorithm(name, key); // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Sign(data);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for VerifySignature
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void VerifySignatureTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            Key key = null; // TODO: Initialize to an appropriate value
            KeyHostAlgorithm target = new KeyHostAlgorithm(name, key); // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            byte[] signature = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.VerifySignature(data, signature);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Data
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DataTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            Key key = null; // TODO: Initialize to an appropriate value
            KeyHostAlgorithm target = new KeyHostAlgorithm(name, key); // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Data;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
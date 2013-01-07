using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography
{
    /// <summary>
    /// Provides HMAC algorithm implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [TestClass]
    public class HMacTest : TestBase
    {
        [TestMethod]
        public void Test_HMac_MD5_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-md5", (key) => { return new HMac<MD5Hash>(key.Take(16).ToArray()); });

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_HMac_Sha1_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-sha1", (key) => { return new HMac<SHA1Hash>(key.Take(20).ToArray()); });

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        /// <summary>
        ///A test for HMac`1 Constructor
        ///</summary>
        public void HMacConstructorTestHelper<T>()
            where T : HashAlgorithm, new()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            HMac<T> target = new HMac<T>(key);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        [TestMethod()]
        public void HMacConstructorTest()
        {
            Assert.Inconclusive("No appropriate type parameter is found to satisfies the type constraint(s) of T. " +
                    "Please call HMacConstructorTestHelper<T>() with appropriate type parameters.");
        }

        /// <summary>
        ///A test for Initialize
        ///</summary>
        public void InitializeTestHelper<T>()
            where T : HashAlgorithm, new()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            HMac<T> target = new HMac<T>(key); // TODO: Initialize to an appropriate value
            target.Initialize();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        [TestMethod()]
        public void InitializeTest()
        {
            Assert.Inconclusive("No appropriate type parameter is found to satisfies the type constraint(s) of T. " +
                    "Please call InitializeTestHelper<T>() with appropriate type parameters.");
        }

        /// <summary>
        ///A test for Key
        ///</summary>
        public void KeyTestHelper<T>()
            where T : HashAlgorithm, new()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            HMac<T> target = new HMac<T>(key); // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            target.Key = expected;
            actual = target.Key;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void KeyTest()
        {
            Assert.Inconclusive("No appropriate type parameter is found to satisfies the type constraint(s) of T. " +
                    "Please call KeyTestHelper<T>() with appropriate type parameters.");
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// old private key information/
    /// </summary>
    [TestClass]
    public class PrivateKeyFileTest : TestBase
    {
        [WorkItem(703), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_PrivateKeyFile_EmptyFileName()
        {
            string fileName = string.Empty;
            var keyFile = new PrivateKeyFile(fileName);
        }

        [WorkItem(703), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_PrivateKeyFile_StreamIsNull()
        {
            Stream stream = null;
            var keyFile = new PrivateKeyFile(stream);
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA()
        {
            new PrivateKeyFile(this.GetData("Key.RSA.txt"));
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_CBC()
        {
            new PrivateKeyFile(this.GetData("Key.RSA.Encrypted.Des.CBC.12345.txt"), "12345");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_EDE3_CBC()
        {
            new PrivateKeyFile(this.GetData("Key.RSA.Encrypted.Des.Ede3.CBC.12345.txt"), "12345");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_128_CBC()
        {
            new PrivateKeyFile(this.GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"), "12345");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_192_CBC()
        {
            new PrivateKeyFile(this.GetData("Key.RSA.Encrypted.Aes.192.CBC.12345.txt"), "12345");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_256_CBC()
        {
            new PrivateKeyFile(this.GetData("Key.RSA.Encrypted.Aes.256.CBC.12345.txt"), "12345");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_EDE3_CFB()
        {
            new PrivateKeyFile(this.GetData("Key.RSA.Encrypted.Des.Ede3.CFB.1234567890.txt"), "1234567890");
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            Stream privateKey = null; // TODO: Initialize to an appropriate value
            PrivateKeyFile target = new PrivateKeyFile(privateKey); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for PrivateKeyFile Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyFileConstructorTest()
        {
            Stream privateKey = null; // TODO: Initialize to an appropriate value
            string passPhrase = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile target = new PrivateKeyFile(privateKey, passPhrase);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PrivateKeyFile Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyFileConstructorTest1()
        {
            string fileName = string.Empty; // TODO: Initialize to an appropriate value
            string passPhrase = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile target = new PrivateKeyFile(fileName, passPhrase);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PrivateKeyFile Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyFileConstructorTest2()
        {
            string fileName = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile target = new PrivateKeyFile(fileName);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PrivateKeyFile Constructor
        ///</summary>
        [TestMethod()]
        public void PrivateKeyFileConstructorTest3()
        {
            Stream privateKey = null; // TODO: Initialize to an appropriate value
            PrivateKeyFile target = new PrivateKeyFile(privateKey);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
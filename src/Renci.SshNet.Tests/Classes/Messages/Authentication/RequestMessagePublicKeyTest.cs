using Renci.SshNet.Messages.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Messages;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Messages.Authentication
{
    /// <summary>
    ///This is a test class for RequestMessagePublicKeyTest and is intended
    ///to contain all RequestMessagePublicKeyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RequestMessagePublicKeyTest : TestBase
    {
        /// <summary>
        ///A test for RequestMessagePublicKey Constructor
        ///</summary>
        [TestMethod()]
        public void RequestMessagePublicKeyConstructorTest()
        {
            ServiceName serviceName = new ServiceName(); // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string keyAlgorithmName = string.Empty; // TODO: Initialize to an appropriate value
            byte[] keyData = null; // TODO: Initialize to an appropriate value
            RequestMessagePublicKey target = new RequestMessagePublicKey(serviceName, username, keyAlgorithmName, keyData);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for RequestMessagePublicKey Constructor
        ///</summary>
        [TestMethod()]
        public void RequestMessagePublicKeyConstructorTest1()
        {
            ServiceName serviceName = new ServiceName(); // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string keyAlgorithmName = string.Empty; // TODO: Initialize to an appropriate value
            byte[] keyData = null; // TODO: Initialize to an appropriate value
            byte[] signature = null; // TODO: Initialize to an appropriate value
            RequestMessagePublicKey target = new RequestMessagePublicKey(serviceName, username, keyAlgorithmName, keyData, signature);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for MethodName
        ///</summary>
        [TestMethod()]
        public void MethodNameTest()
        {
            ServiceName serviceName = new ServiceName(); // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string keyAlgorithmName = string.Empty; // TODO: Initialize to an appropriate value
            byte[] keyData = null; // TODO: Initialize to an appropriate value
            RequestMessagePublicKey target = new RequestMessagePublicKey(serviceName, username, keyAlgorithmName, keyData); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.MethodName;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Signature
        ///</summary>
        [TestMethod()]
        public void SignatureTest()
        {
            ServiceName serviceName = new ServiceName(); // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string keyAlgorithmName = string.Empty; // TODO: Initialize to an appropriate value
            byte[] keyData = null; // TODO: Initialize to an appropriate value
            RequestMessagePublicKey target = new RequestMessagePublicKey(serviceName, username, keyAlgorithmName, keyData); // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            target.Signature = expected;
            actual = target.Signature;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}

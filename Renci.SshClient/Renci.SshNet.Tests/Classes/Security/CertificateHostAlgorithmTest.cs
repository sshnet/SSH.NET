using Renci.SshNet.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for CertificateHostAlgorithmTest and is intended
    ///to contain all CertificateHostAlgorithmTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CertificateHostAlgorithmTest : TestBase
    {
        /// <summary>
        ///A test for CertificateHostAlgorithm Constructor
        ///</summary>
        [TestMethod()]
        public void CertificateHostAlgorithmConstructorTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            CertificateHostAlgorithm target = new CertificateHostAlgorithm(name);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Sign
        ///</summary>
        [TestMethod()]
        public void SignTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            CertificateHostAlgorithm target = new CertificateHostAlgorithm(name); // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void VerifySignatureTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            CertificateHostAlgorithm target = new CertificateHostAlgorithm(name); // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void DataTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            CertificateHostAlgorithm target = new CertificateHostAlgorithm(name); // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Data;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}

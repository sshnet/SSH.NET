using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;
using System;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Holds information about key size and cipher to use
    /// </summary>
    [TestClass]
    public class CipherInfoTest : TestBase
    {
        /// <summary>
        ///A test for CipherInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void CipherInfoConstructorTest()
        {
            int keySize = 0; // TODO: Initialize to an appropriate value
            Func<byte[], byte[], Cipher> cipher = null; // TODO: Initialize to an appropriate value
            CipherInfo target = new CipherInfo(keySize, cipher);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
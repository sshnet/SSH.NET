using System;
using System.Security.Cryptography;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Org.BouncyCastle.Crypto.Agreement;

using Renci.SshNet.Security;

namespace Renci.SshNet.Tests.Classes.Security
{
    [TestClass]
    public class KeyExchangeDiffieHellmanTest
    {
        [TestMethod]
        public void NameShouldBeCtorValue()
        {
            KeyExchangeDiffieHellman kex = new("diffie-hellman-group16-sha512", DHStandardGroups.rfc3526_4096, HashAlgorithmName.SHA512);

            Assert.AreEqual("diffie-hellman-group16-sha512", kex.Name);
        }

        [TestMethod]
        public void Ctor_ArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new KeyExchangeDiffieHellman(name: null, DHStandardGroups.rfc3526_4096, HashAlgorithmName.SHA512));
            Assert.AreEqual("name", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new KeyExchangeDiffieHellman("kex", parameters: null, HashAlgorithmName.SHA512));
            Assert.AreEqual("parameters", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new KeyExchangeDiffieHellman("kex", DHStandardGroups.rfc3526_4096, default));
            Assert.AreEqual("hashAlgorithm", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new KeyExchangeDiffieHellman("kex", DHStandardGroups.rfc3526_4096, new HashAlgorithmName(null)));
            Assert.AreEqual("hashAlgorithm", ex.ParamName);
        }

        [TestMethod]
        public void Ctor_InvalidHashAlgorithm_ThrowsArgumentException()
        {
            var ex = Assert.ThrowsExactly<ArgumentException>(() => new KeyExchangeDiffieHellman("kex", DHStandardGroups.rfc3526_4096, new HashAlgorithmName("bad")));
            Assert.AreEqual("hashAlgorithm", ex.ParamName);

            ex = Assert.ThrowsExactly<ArgumentException>(() => new KeyExchangeDiffieHellman("kex", DHStandardGroups.rfc3526_4096, new HashAlgorithmName("")));
            Assert.AreEqual("hashAlgorithm", ex.ParamName);
        }
    }
}

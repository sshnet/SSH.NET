using System;
using System.Security.Cryptography;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Security;

namespace Renci.SshNet.Tests.Classes.Security
{
    [TestClass]
    public class KeyExchangeDiffieHellmanGroupExchangeTest
    {
        [TestMethod]
        public void NameShouldBeCtorValue()
        {
            KeyExchangeDiffieHellmanGroupExchange kex = new("diffie-hellman-group-exchange-sha256", HashAlgorithmName.SHA512);

            Assert.AreEqual("diffie-hellman-group-exchange-sha256", kex.Name);
        }

        [TestMethod]
        public void Ctor_ArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new KeyExchangeDiffieHellmanGroupExchange(name: null, HashAlgorithmName.SHA512));
            Assert.AreEqual("name", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", default));
            Assert.AreEqual("hashAlgorithm", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", new HashAlgorithmName(null)));
            Assert.AreEqual("hashAlgorithm", ex.ParamName);
        }

        [TestMethod]
        public void Ctor_InvalidHashAlgorithm_ThrowsArgumentException()
        {
            var ex = Assert.ThrowsExactly<ArgumentException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", new HashAlgorithmName("bad")));
            Assert.AreEqual("hashAlgorithm", ex.ParamName);

            ex = Assert.ThrowsExactly<ArgumentException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", new HashAlgorithmName("")));
            Assert.AreEqual("hashAlgorithm", ex.ParamName);
        }

        [TestMethod]
        public void Ctor_InvalidGroupSizes_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", HashAlgorithmName.SHA512, 1024, 4096, 2048));
            Assert.AreEqual("preferredGroupSize", ex.ParamName);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => new KeyExchangeDiffieHellmanGroupExchange("kex", HashAlgorithmName.SHA512, 8192, 4096, 2048));
            Assert.AreEqual("preferredGroupSize", ex.ParamName);
        }
    }
}

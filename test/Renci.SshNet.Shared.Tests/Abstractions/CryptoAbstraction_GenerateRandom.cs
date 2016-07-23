using System;
using System.Linq;
using Renci.SshNet.Abstractions;
#if SILVERLIGHT
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Renci.SshNet.Tests.Abstractions
{
    [TestClass]
    public class CryptoAbstraction_GenerateRandom
    {
        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenDataIsNull()
        {
            const byte[] data = null;

            try
            {
                CryptoAbstraction.GenerateRandom(data); 
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("data", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldPerformNoOpWhenDataIsZeroLength()
        {
            var data = new byte[0];

            CryptoAbstraction.GenerateRandom(data);
        }

        [TestMethod]
        public void ShouldGenerateRandomSequenceOfValues()
        {
            var dataLength = new Random().Next(1, 100);
            var dataA = new byte[dataLength];
            var dataB = new byte[dataLength];

            CryptoAbstraction.GenerateRandom(dataA);
            CryptoAbstraction.GenerateRandom(dataB);

            Assert.IsFalse(dataA.SequenceEqual(dataB));
        }
    }
}

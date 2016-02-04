using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///
    /// </summary>
    [TestClass]
    public class ASCIIEncodingTest : TestBase
    {

        /// <summary>
        ///A test for GetByteCount
        ///</summary>
        [TestMethod()]
        public void GetByteCountTest()
        {
            ASCIIEncoding target = new ASCIIEncoding(); // TODO: Initialize to an appropriate value
            char[] chars = null; // TODO: Initialize to an appropriate value
            int index = 0; // TODO: Initialize to an appropriate value
            int count = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.GetByteCount(chars, index, count);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ASCIIEncoding Constructor
        ///</summary>
        [TestMethod()]
        public void ASCIIEncodingConstructorTest()
        {
            ASCIIEncoding target = new ASCIIEncoding();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for GetBytes
        ///</summary>
        [TestMethod()]
        public void GetBytesTest()
        {
            ASCIIEncoding target = new ASCIIEncoding(); // TODO: Initialize to an appropriate value
            char[] chars = null; // TODO: Initialize to an appropriate value
            int charIndex = 0; // TODO: Initialize to an appropriate value
            int charCount = 0; // TODO: Initialize to an appropriate value
            byte[] bytes = null; // TODO: Initialize to an appropriate value
            int byteIndex = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetCharCount
        ///</summary>
        [TestMethod()]
        public void GetCharCountTest()
        {
            ASCIIEncoding target = new ASCIIEncoding(); // TODO: Initialize to an appropriate value
            byte[] bytes = null; // TODO: Initialize to an appropriate value
            int index = 0; // TODO: Initialize to an appropriate value
            int count = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.GetCharCount(bytes, index, count);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetChars
        ///</summary>
        [TestMethod()]
        public void GetCharsTest()
        {
            ASCIIEncoding target = new ASCIIEncoding(); // TODO: Initialize to an appropriate value
            byte[] bytes = null; // TODO: Initialize to an appropriate value
            int byteIndex = 0; // TODO: Initialize to an appropriate value
            int byteCount = 0; // TODO: Initialize to an appropriate value
            char[] chars = null; // TODO: Initialize to an appropriate value
            int charIndex = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetMaxByteCount
        ///</summary>
        [TestMethod()]
        public void GetMaxByteCountTest()
        {
            ASCIIEncoding target = new ASCIIEncoding(); // TODO: Initialize to an appropriate value
            int charCount = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.GetMaxByteCount(charCount);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetMaxCharCount
        ///</summary>
        [TestMethod()]
        public void GetMaxCharCountTest()
        {
            ASCIIEncoding target = new ASCIIEncoding(); // TODO: Initialize to an appropriate value
            int byteCount = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.GetMaxCharCount(byteCount);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

    }
}
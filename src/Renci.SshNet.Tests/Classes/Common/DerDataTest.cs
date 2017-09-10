using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for DerDataTest and is intended
    ///to contain all DerDataTest Unit Tests
    ///</summary>
    [TestClass]
    [Ignore] // placeholder for actual test
    public class DerDataTest : TestBase
    {
        /// <summary>
        ///A test for DerData Constructor
        ///</summary>
        [TestMethod]
        public void DerDataConstructorTest()
        {
            DerData target = new DerData();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for DerData Constructor
        ///</summary>
        [TestMethod]
        public void DerDataConstructorTest1()
        {
            byte[] data = null; // TODO: Initialize to an appropriate value
            DerData target = new DerData(data);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Encode
        ///</summary>
        [TestMethod]
        public void EncodeTest()
        {
            DerData target = new DerData(); // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Encode();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadBigInteger
        ///</summary>
        [TestMethod]
        public void ReadBigIntegerTest()
        {
            DerData target = new DerData(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = target.ReadBigInteger();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadInteger
        ///</summary>
        [TestMethod]
        public void ReadIntegerTest()
        {
            DerData target = new DerData(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.ReadInteger();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod]
        public void WriteTest()
        {
            DerData target = new DerData(); // TODO: Initialize to an appropriate value
            bool data = false; // TODO: Initialize to an appropriate value
            target.Write(data);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod]
        public void WriteTest1()
        {
            DerData target = new DerData(); // TODO: Initialize to an appropriate value
            uint data = 0; // TODO: Initialize to an appropriate value
            target.Write(data);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod]
        public void WriteTest2()
        {
            DerData target = new DerData(); // TODO: Initialize to an appropriate value
            BigInteger data = new BigInteger(); // TODO: Initialize to an appropriate value
            target.Write(data);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod]
        public void WriteTest3()
        {
            DerData target = new DerData(); // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            target.Write(data);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod]
        public void WriteTest4()
        {
            DerData target = new DerData(); // TODO: Initialize to an appropriate value
            ObjectIdentifier identifier = new ObjectIdentifier(); // TODO: Initialize to an appropriate value
            target.Write(identifier);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod]
        public void WriteTest5()
        {
            DerData target = new DerData(); // TODO: Initialize to an appropriate value
            DerData data = null; // TODO: Initialize to an appropriate value
            target.Write(data);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WriteNull
        ///</summary>
        [TestMethod]
        public void WriteNullTest()
        {
            DerData target = new DerData(); // TODO: Initialize to an appropriate value
            target.WriteNull();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for IsEndOfData
        ///</summary>
        [TestMethod]
        public void IsEndOfDataTest()
        {
            DerData target = new DerData(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsEndOfData;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}

using Renci.SshNet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for SshDataTest and is intended
    ///to contain all SshDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SshDataTest : TestBase
    {
        internal virtual SshData CreateSshData()
        {
            // TODO: Instantiate an appropriate concrete class.
            SshData target = null;
            return target;
        }

        /// <summary>
        ///A test for GetBytes
        ///</summary>
        [TestMethod()]
        public void GetBytesTest()
        {
            SshData target = CreateSshData(); // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.GetBytes();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Load
        ///</summary>
        [TestMethod()]
        public void LoadTest()
        {
            SshData target = CreateSshData(); // TODO: Initialize to an appropriate value
            byte[] value = null; // TODO: Initialize to an appropriate value
            target.Load(value);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for IsEndOfData
        ///</summary>
        [TestMethod()]
        public void IsEndOfDataTest()
        {
            SshData target = CreateSshData(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsEndOfData;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}

﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Compression;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Compression
{
    /// <summary>
    ///This is a test class for ZlibOpenSshTest and is intended
    ///to contain all ZlibOpenSshTest Unit Tests
    ///</summary>
    [TestClass]
    [Ignore] // placeholders only
    public class ZlibOpenSshTest : TestBase
    {
        /// <summary>
        ///A test for ZlibOpenSsh Constructor
        ///</summary>
        [TestMethod()]
        public void ZlibOpenSshConstructorTest()
        {
            var target = new ZlibOpenSsh();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Init
        ///</summary>
        [TestMethod()]
        public void InitTest()
        {
            var target = new ZlibOpenSsh(); // TODO: Initialize to an appropriate value
            Session session = null; // TODO: Initialize to an appropriate value
            target.Init(session);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod()]
        public void NameTest()
        {
            var target = new ZlibOpenSsh(); // TODO: Initialize to an appropriate value
            var actual = target.Name;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}

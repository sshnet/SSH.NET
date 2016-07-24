using Renci.SshNet.Messages.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Transport
{
    /// <summary>
    ///This is a test class for NewKeysMessageTest and is intended
    ///to contain all NewKeysMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class NewKeysMessageTest : TestBase
    {
        /// <summary>
        ///A test for NewKeysMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void NewKeysMessageConstructorTest()
        {
            NewKeysMessage target = new NewKeysMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}

using Renci.SshNet.Messages.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Transport
{
    /// <summary>
    ///This is a test class for UnimplementedMessageTest and is intended
    ///to contain all UnimplementedMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class UnimplementedMessageTest : TestBase
    {
        /// <summary>
        ///A test for UnimplementedMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void UnimplementedMessageConstructorTest()
        {
            UnimplementedMessage target = new UnimplementedMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}

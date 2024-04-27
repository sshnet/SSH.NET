using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Text;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for GlobalRequestMessageTest and is intended
    ///to contain all GlobalRequestMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class GlobalRequestMessageTest : TestBase
    {
        [TestMethod]
        public void DefaultCtor()
        {
            new GlobalRequestMessage();
        }

        [TestMethod]
        public void Ctor_RequestNameAndWantReply()
        {
            var requestName = new Random().Next().ToString(CultureInfo.InvariantCulture);

            var target = new GlobalRequestMessage(Encoding.ASCII.GetBytes(requestName), true);
            Assert.AreEqual(requestName, target.RequestName);
            Assert.IsTrue(target.WantReply);

            target = new GlobalRequestMessage(Encoding.ASCII.GetBytes(requestName), false);
            Assert.AreEqual(requestName, target.RequestName);
            Assert.IsFalse(target.WantReply);
        }
    }
}

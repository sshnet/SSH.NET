using Renci.SshNet.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for KeyExchangeDiffieHellmanTest and is intended
    ///to contain all KeyExchangeDiffieHellmanTest Unit Tests
    ///</summary>
    [TestClass()]
    public class KeyExchangeDiffieHellmanTest : TestBase
    {
        internal virtual KeyExchangeDiffieHellman CreateKeyExchangeDiffieHellman()
        {
            // TODO: Instantiate an appropriate concrete class.
            KeyExchangeDiffieHellman target = null;
            return target;
        }

        /// <summary>
        ///A test for Start
        ///</summary>
        [TestMethod()]
        public void StartTest()
        {
            KeyExchangeDiffieHellman target = CreateKeyExchangeDiffieHellman(); // TODO: Initialize to an appropriate value
            Session session = null; // TODO: Initialize to an appropriate value
            KeyExchangeInitMessage message = null; // TODO: Initialize to an appropriate value
            target.Start(session, message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}

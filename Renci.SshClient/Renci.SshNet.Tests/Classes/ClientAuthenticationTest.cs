using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ClientAuthenticationTest
    {
        private ClientAuthentication _clientAuthentication;

        [TestInitialize]
        public void Init()
        {
            _clientAuthentication = new ClientAuthentication();
        }

        [TestMethod]
        public void AuthenticateShouldThrowArgumentNullExceptionWhenConnectionInfoIsNull()
        {
            IConnectionInfoInternal connectionInfo = null;
            var session = new Mock<ISession>(MockBehavior.Strict).Object;

            try
            {
                _clientAuthentication.Authenticate(connectionInfo, session);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("connectionInfo", ex.ParamName);
            }
        }

        [TestMethod]
        public void AuthenticateShouldThrowArgumentNullExceptionWhenSessionIsNull()
        {
            var connectionInfo = new Mock<IConnectionInfoInternal>(MockBehavior.Strict).Object;
            ISession session = null;

            try
            {
                _clientAuthentication.Authenticate(connectionInfo, session);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("session", ex.ParamName);
            }
        }
    }
}

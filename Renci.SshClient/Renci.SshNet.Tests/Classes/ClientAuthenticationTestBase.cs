using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public abstract class ClientAuthenticationTestBase : TestBase
    {
        internal Mock<IConnectionInfoInternal> ConnectionInfoMock { get; private set; }
        internal Mock<ISession> SessionMock { get; private set; }
        internal Mock<IAuthenticationMethod> NoneAuthenticationMethodMock { get; private set; }
        internal Mock<IAuthenticationMethod> PasswordAuthenticationMethodMock { get; private set; }
        internal Mock<IAuthenticationMethod> PublicKeyAuthenticationMethodMock { get; private set; }
        internal Mock<IAuthenticationMethod> KeyboardInteractiveAuthenticationMethodMock { get; private set; }
        internal ClientAuthentication ClientAuthentication { get; private set; }

        protected void CreateMocks()
        {
            ConnectionInfoMock = new Mock<IConnectionInfoInternal>(MockBehavior.Strict);
            SessionMock = new Mock<ISession>(MockBehavior.Strict);
            NoneAuthenticationMethodMock = new Mock<IAuthenticationMethod>(MockBehavior.Strict);
            PasswordAuthenticationMethodMock = new Mock<IAuthenticationMethod>(MockBehavior.Strict);
            PublicKeyAuthenticationMethodMock = new Mock<IAuthenticationMethod>(MockBehavior.Strict);
            KeyboardInteractiveAuthenticationMethodMock = new Mock<IAuthenticationMethod>(MockBehavior.Strict);
        }

        protected abstract void SetupMocks();

        protected abstract void Act();

        protected override void OnInit()
        {
            base.OnInit();

            // Arrange
            CreateMocks();
            SetupMocks();
            ClientAuthentication = new ClientAuthentication();

            // Act
            Act();
        }

        [TestMethod]
        public void RegisterMessageWithUserAuthFailureShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.RegisterMessage("SSH_MSG_USERAUTH_FAILURE"), Times.Once);
        }

        [TestMethod]
        public void RegisterMessageWithUserAuthSuccessShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.RegisterMessage("SSH_MSG_USERAUTH_SUCCESS"), Times.Once);
        }

        [TestMethod]
        public void RegisterMessageWithUserAuthBannerShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.RegisterMessage("SSH_MSG_USERAUTH_BANNER"), Times.Once);
        }

        [TestMethod]
        public void UnRegisterMessageWithUserAuthFailureShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_FAILURE"), Times.Once);
        }

        [TestMethod]
        public void UnRegisterMessageWithUserAuthSuccessShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_SUCCESS"), Times.Once);
        }

        [TestMethod]
        public void UnRegisterMessageWithUserAuthBannerShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_BANNER"), Times.Once);
        }
    }
}

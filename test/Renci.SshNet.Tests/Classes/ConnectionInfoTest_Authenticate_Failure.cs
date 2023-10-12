using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ConnectionInfoTest_Authenticate_Failure
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<IClientAuthentication> _clientAuthenticationMock;
        private Mock<ISession> _sessionMock;
        private ConnectionInfo _connectionInfo;
        private SshAuthenticationException _authenticationException;
        private SshAuthenticationException _actualException;

        [TestInitialize]
        public void Init()
        {
            Arrange();
            Act();
        }

        protected void Arrange()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _clientAuthenticationMock = new Mock<IClientAuthentication>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            _connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None,
                Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
            _authenticationException = new SshAuthenticationException();

            _serviceFactoryMock.Setup(p => p.CreateClientAuthentication()).Returns(_clientAuthenticationMock.Object);
            _clientAuthenticationMock.Setup(p => p.Authenticate(_connectionInfo, _sessionMock.Object))
                .Throws(_authenticationException);
        }

        protected void Act()
        {
            try
            {
                _connectionInfo.Authenticate(_sessionMock.Object, _serviceFactoryMock.Object);
            }
            catch (SshAuthenticationException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void AuthenticateShouldHaveThrownSshAuthenticationException()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreSame(_authenticationException, _actualException);
        }

        [TestMethod]
        public void IsAuthenticatedShouldReturnFalse()
        {
            Assert.IsFalse(_connectionInfo.IsAuthenticated);
        }

        [TestMethod]
        public void CreateClientAuthenticationOnServiceFactoryShouldBeInvokedOnce()
        {
            _serviceFactoryMock.Verify(p => p.CreateClientAuthentication(), Times.Once);
        }

        [TestMethod]
        public void AuthenticateOnClientAuthenticationShouldBeInvokedOnce()
        {
            _clientAuthenticationMock.Verify(p => p.Authenticate(_connectionInfo, _sessionMock.Object), Times.Once);
        }
    }
}

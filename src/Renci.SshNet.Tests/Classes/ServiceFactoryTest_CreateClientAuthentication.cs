using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ServiceFactoryTest_CreateClientAuthentication
    {
        private ServiceFactory _serviceFactory;
        private IClientAuthentication _actual;

        private void Arrange()
        {
            _serviceFactory = new ServiceFactory();
        }

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        private void Act()
        {
            _actual = _serviceFactory.CreateClientAuthentication();
        }

        [TestMethod]
        public void CreateClientAuthenticationShouldNotReturnNull()
        {
            Assert.IsNotNull(_actual);
        }

        [TestMethod]
        public void ClientAuthenticationShouldHavePartialSuccessLimitOf5()
        {
            var clientAuthentication = _actual as ClientAuthentication;
            Assert.IsNotNull(clientAuthentication);
            Assert.AreEqual(5, clientAuthentication.PartialSuccessLimit);
        }
    }
}

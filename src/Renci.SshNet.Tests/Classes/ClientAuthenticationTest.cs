using System;
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
            _clientAuthentication = new ClientAuthentication(1);
        }

        [TestMethod]
        public void Ctor_PartialSuccessLimit_Zero()
        {
            const int partialSuccessLimit = 0;

            try
            {
                new ClientAuthentication(partialSuccessLimit);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Cannot be less than one.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("partialSuccessLimit", ex.ParamName);
            }
        }

        [TestMethod]
        public void Ctor_PartialSuccessLimit_Negative()
        {
            var partialSuccessLimit = new Random().Next(int.MinValue, -1);

            try
            {
                new ClientAuthentication(partialSuccessLimit);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Cannot be less than one.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("partialSuccessLimit", ex.ParamName);
            }
        }

        [TestMethod]
        public void Ctor_PartialSuccessLimit_One()
        {
            const int partialSuccessLimit = 1;

            var clientAuthentication = new ClientAuthentication(partialSuccessLimit);
            Assert.AreEqual(partialSuccessLimit, clientAuthentication.PartialSuccessLimit);
        }

        [TestMethod]
        public void Ctor_PartialSuccessLimit_MaxValue()
        {
            const int partialSuccessLimit = int.MaxValue;

            var clientAuthentication = new ClientAuthentication(partialSuccessLimit);
            Assert.AreEqual(partialSuccessLimit, clientAuthentication.PartialSuccessLimit);
        }


        [TestMethod]
        public void AuthenticateShouldThrowArgumentNullExceptionWhenConnectionInfoIsNull()
        {
           const IConnectionInfoInternal connectionInfo = null;
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
            const ISession session = null;

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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// ConnectionInfo provides 'password' and 'publickey' authentication methods, and partial success limit is
    /// set to 3.
    ///
    /// Authentication proceeds as follows:
    /// 
    /// 1 x     * Client performs 'none' authentication attempt.
    ///         * Server responds with 'failure', and 'password' allowed authentication method.
    /// 
    /// 3 x     * Client performs 'password' authentication attempt.
    ///         * Server responds with 'partial success', and 'password' allowed authentication method
    /// 
    /// Since the server only ever allowed the 'password' authentication method, there are no
    /// authentication methods left to try after reaching the partial success limit for 'password'
    /// and as such authentication fails.
    /// </summary>
    [TestClass]
    public class ClientAuthenticationTest_Success_SingleList_SameAllowedAuthenticationAfterPartialSuccess_PartialSuccessLimitReached : ClientAuthenticationTestBase
    {
        private int _partialSuccessLimit;
        private ClientAuthentication _clientAuthentication;
        private SshAuthenticationException _actualException;

        protected override void SetupData()
        {
            _partialSuccessLimit = 3;
        }

        protected override void SetupMocks()
        {
            var seq = new MockSequence();

            SessionMock.InSequence(seq).Setup(p => p.RegisterMessage("SSH_MSG_USERAUTH_FAILURE"));
            SessionMock.InSequence(seq).Setup(p => p.RegisterMessage("SSH_MSG_USERAUTH_SUCCESS"));
            SessionMock.InSequence(seq).Setup(p => p.RegisterMessage("SSH_MSG_USERAUTH_BANNER"));

            ConnectionInfoMock.InSequence(seq).Setup(p => p.CreateNoneAuthenticationMethod())
                              .Returns(NoneAuthenticationMethodMock.Object);

            NoneAuthenticationMethodMock.InSequence(seq).Setup(p => p.Authenticate(SessionMock.Object))
                                        .Returns(AuthenticationResult.Failure);
            ConnectionInfoMock.InSequence(seq)
                              .Setup(p => p.AuthenticationMethods)
                              .Returns(new List<IAuthenticationMethod>
                                  {
                                      PublicKeyAuthenticationMethodMock.Object,
                                      PasswordAuthenticationMethodMock.Object
                                  });
            NoneAuthenticationMethodMock.InSequence(seq)
                                        .Setup(p => p.AllowedAuthentications)
                                        .Returns(new[] { "password" });

            for (var i = 0; i < _partialSuccessLimit; i++)
            {
                PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
                PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");

                PasswordAuthenticationMethodMock.InSequence(seq)
                                                .Setup(p => p.Authenticate(SessionMock.Object))
                                                .Returns(AuthenticationResult.PartialSuccess);
                PasswordAuthenticationMethodMock.InSequence(seq)
                                                .Setup(p => p.AllowedAuthentications)
                                                .Returns(new[] {"password"});
            }

            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");

            // used to construct exception message
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("x_password_x");

            SessionMock.InSequence(seq).Setup(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_FAILURE"));
            SessionMock.InSequence(seq).Setup(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_SUCCESS"));
            SessionMock.InSequence(seq).Setup(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_BANNER"));
        }

        protected override void Arrange()
        {
            base.Arrange();

            _clientAuthentication = new ClientAuthentication(_partialSuccessLimit);
        }

        protected override void Act()
        {
            try
            {
                _clientAuthentication.Authenticate(ConnectionInfoMock.Object, SessionMock.Object);
                Assert.Fail();
            }
            catch (SshAuthenticationException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void AuthenticateShouldThrowSshAuthenticationException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("Reached authentication attempt limit for method (x_password_x).",_actualException.Message);
        }
    }
}

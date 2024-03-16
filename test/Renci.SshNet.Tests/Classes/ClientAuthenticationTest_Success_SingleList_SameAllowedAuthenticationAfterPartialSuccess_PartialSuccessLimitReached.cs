using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// * ConnectionInfo provides the following authentication methods (in order):
    ///     o publickey
    ///     o password
    /// * Partial success limit is 3
    /// * Scenario:
    ///                           none
    ///                          (1=FAIL)
    ///                             |
    ///                         password
    ///                       (2=PARTIAL)
    ///                             |
    ///                         password
    ///                       (3=PARTIAL)
    ///                             |
    ///                         password
    ///                       (4=PARTIAL)
    ///                             |
    ///                         password
    ///                         (5=SKIP)
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

            /* 1 */
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

            /* Enumerate supported authentication methods */

            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");

            /* 2 */

            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.Authenticate(SessionMock.Object))
                                            .Returns(AuthenticationResult.PartialSuccess);
            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.AllowedAuthentications)
                                            .Returns(new[] {"password"});

            /* Enumerate supported authentication methods */

            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");

            /* 3 */

            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.Authenticate(SessionMock.Object))
                                            .Returns(AuthenticationResult.PartialSuccess);
            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.AllowedAuthentications)
                                            .Returns(new[] { "password" });

            /* Enumerate supported authentication methods */

            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");

            /* 4 */

            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.Authenticate(SessionMock.Object))
                                            .Returns(AuthenticationResult.PartialSuccess);
            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.AllowedAuthentications)
                                            .Returns(new[] { "password" });

            /* Enumerate supported authentication methods */

            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");

            /* 5: Record partial success limit reached exception, and skip password authentication method */

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

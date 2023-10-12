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
    /// * Partial success limit is 2
    /// 
    ///                               none
    ///                             (1=FAIL)
    ///                                 |
    ///        +------------------------+------------------------+
    ///        |                        |                        |
    ///    password      ◄--\       publickey            keyboard-interactive
    ///    (7=SKIP)         |         (2=PS)
    ///                     |           |
    ///                     |        password
    ///                     |         (3=PS)
    ///                     |           |
    ///                     |        password
    ///                     |         (4=PS)
    ///                     |           |
    ///                     |       publickey
    ///                     |         (5=PS)
    ///                     |           |
    ///                     \----   publickey
    ///                              (6=SKIP)
    /// </summary>
    [TestClass]
    public class ClientAuthenticationTest_Failure_MultiList_AllAllowedAuthenticationsHaveReachedPartialSuccessLimit : ClientAuthenticationTestBase
    {
        private int _partialSuccessLimit;
        private ClientAuthentication _clientAuthentication;
        private SshAuthenticationException _actualException;

        protected override void SetupData()
        {
            _partialSuccessLimit = 2;
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
                                        .Returns(new[] {"password", "publickey", "keyboard-interactive"});

            /* Enumerate supported authentication methods */

            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");

            /* 2 */

            PublicKeyAuthenticationMethodMock.InSequence(seq)
                                             .Setup(p => p.Authenticate(SessionMock.Object))
                                             .Returns(AuthenticationResult.PartialSuccess);
            PublicKeyAuthenticationMethodMock.InSequence(seq)
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
                                            .Returns(new[] {"password"});

            /* Enumerate supported authentication methods */

            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");

            /* 4 */

            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.Authenticate(SessionMock.Object))
                                            .Returns(AuthenticationResult.PartialSuccess);
            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.AllowedAuthentications)
                                            .Returns(new[] {"publickey"});

            /* Enumerate supported authentication methods */

            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");

            /* 5 */

            PublicKeyAuthenticationMethodMock.InSequence(seq)
                                             .Setup(p => p.Authenticate(SessionMock.Object))
                                             .Returns(AuthenticationResult.PartialSuccess);
            PublicKeyAuthenticationMethodMock.InSequence(seq)
                                             .Setup(p => p.AllowedAuthentications)
                                             .Returns(new[] {"publickey"});

            /* Enumerate supported authentication methods */

            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");

            /* 6: Record partial success limit reached exception, and skip password authentication method */

            PublicKeyAuthenticationMethodMock.InSequence(seq)
                                             .Setup(p => p.Name)
                                             .Returns("publickey-partial1");

            /* 7: Record partial success limit reached exception, and skip password authentication method */

            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.Name)
                                            .Returns("password-partial1");
            
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
        public void AuthenticateOnPasswordAuthenticationMethodShouldHaveBeenInvokedTwice()
        {
            PasswordAuthenticationMethodMock.Verify(p => p.Authenticate(SessionMock.Object), Times.Exactly(2));
        }

        [TestMethod]
        public void AuthenticateOnPublicKeyAuthenticationMethodShouldHaveBeenInvokedTwice()
        {
            PublicKeyAuthenticationMethodMock.Verify(p => p.Authenticate(SessionMock.Object), Times.Exactly(2));
        }

        [TestMethod]
        public void AuthenticateShouldThrowSshAuthenticationException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("Reached authentication attempt limit for method (password-partial1).", _actualException.Message);
        }
    }
}
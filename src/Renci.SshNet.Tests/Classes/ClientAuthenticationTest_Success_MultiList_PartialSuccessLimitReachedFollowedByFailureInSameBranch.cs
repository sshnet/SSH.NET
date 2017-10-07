using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// * ConnectionInfo provides the following authentication methods (in order):
    ///     o keyboard-interactive
    ///     o password
    ///     o publickey
    /// * Partial success limit is 2
    /// * Scenario:
    ///                           none
    ///                          (1=FAIL)
    ///                             |
    ///                         password
    ///                       (2=PARTIAL)
    ///                             |
    ///             +------------------------------+
    ///             |                              |
    ///         password                       publickey
    ///       (4=PARTIAL)                     (3=FAILURE)
    ///             |
    ///     keyboard-interactive
    ///        (5=FAILURE)
    /// </summary>
    [TestClass]
    public class ClientAuthenticationTest_Success_MultiList_PartialSuccessLimitReachedFollowedByFailureInSameBranch : ClientAuthenticationTestBase
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
                                      KeyboardInteractiveAuthenticationMethodMock.Object,
                                      PasswordAuthenticationMethodMock.Object,
                                      PublicKeyAuthenticationMethodMock.Object
                                  });
            NoneAuthenticationMethodMock.InSequence(seq)
                                        .Setup(p => p.AllowedAuthentications)
                                        .Returns(new[] {"password"});

            /* Enumerate supported authentication methods */

            KeyboardInteractiveAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("keyboard-interactive");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");
            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");

            /* 2 */

            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.Authenticate(SessionMock.Object))
                                            .Returns(AuthenticationResult.PartialSuccess);
            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.AllowedAuthentications)
                                            .Returns(new[] {"password", "publickey"});

            /* Enumerate supported authentication methods */

            KeyboardInteractiveAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("keyboard-interactive");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");
            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");

            /* 3 */

            PublicKeyAuthenticationMethodMock.InSequence(seq)
                                             .Setup(p => p.Authenticate(SessionMock.Object))
                                             .Returns(AuthenticationResult.Failure);
            PublicKeyAuthenticationMethodMock.InSequence(seq)
                                             .Setup(p => p.Name)
                                             .Returns("publickey-failure");

            /* 4 */

            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.Authenticate(SessionMock.Object))
                                            .Returns(AuthenticationResult.PartialSuccess);
            PasswordAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.AllowedAuthentications)
                                            .Returns(new[] {"keyboard-interactive"});

            /* Enumerate supported authentication methods */

            KeyboardInteractiveAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("keyboard-interactive");
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");
            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");

            /* 5 */

            KeyboardInteractiveAuthenticationMethodMock.InSequence(seq)
                                                       .Setup(p => p.Authenticate(SessionMock.Object))
                                                       .Returns(AuthenticationResult.Failure);
            KeyboardInteractiveAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("keyboard-interactive-failure");

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
        public void AuthenticateOnKeyboardInteractiveAuthenticationMethodShouldHaveBeenInvokedOnce()
        {
            KeyboardInteractiveAuthenticationMethodMock.Verify(p => p.Authenticate(SessionMock.Object), Times.Once);
        }

        [TestMethod]
        public void AuthenticateShouldThrowSshAuthenticationException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("Permission denied (keyboard-interactive-failure).", _actualException.Message);
        }
    }
}

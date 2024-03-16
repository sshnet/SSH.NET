using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// * ConnectionInfo provides the following authentication methods (in order):
    ///     o password
    ///     o publickey
    ///     o keyboard-interactive
    /// * Partial success limit is 1
    /// * Scenario:
    ///                           none
    ///                         (1=FAIL)
    ///                             |
    ///             +------------------------------+
    ///             |                              |
    ///         publickey                       password
    ///                                       (2=PARTIAL)
    ///                                *----------------------*
    ///                                |                      |
    ///                       keyboard-interactive        publickey
    ///                                                  (3=SUCCESS)
    /// </summary>
    [TestClass]
    public class ClientAuthenticationTest_Success_MultiList_DifferentAllowedAuthenticationsAfterPartialSuccess : ClientAuthenticationTestBase
    {
        private int _partialSuccessLimit;
        private ClientAuthentication _clientAuthentication;

        protected override void SetupData()
        {
            _partialSuccessLimit = 1;
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
            ConnectionInfoMock.InSequence(seq).Setup(p => p.AuthenticationMethods)
                            .Returns(new List<IAuthenticationMethod>
                {
                    PasswordAuthenticationMethodMock.Object,
                    PublicKeyAuthenticationMethodMock.Object,
                    KeyboardInteractiveAuthenticationMethodMock.Object,
                });
            NoneAuthenticationMethodMock.InSequence(seq)
                                        .Setup(p => p.AllowedAuthentications)
                                        .Returns(new[] {"publickey", "password"});

            /* Enumerate supported authentication methods */

            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");
            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
            KeyboardInteractiveAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("keyboard-interactive");

            /* 2 */

            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Authenticate(SessionMock.Object))
                .Returns(AuthenticationResult.PartialSuccess);
            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.AllowedAuthentications)
                                            .Returns(new[] {"keyboard-interactive", "publickey"});

            /* Enumerate supported authentication methods */

            PasswordAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("password");
            PublicKeyAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("publickey");
            KeyboardInteractiveAuthenticationMethodMock.InSequence(seq).Setup(p => p.Name).Returns("keyboard-interactive");

            /* 3 */

            PublicKeyAuthenticationMethodMock.InSequence(seq)
                                             .Setup(p => p.Authenticate(SessionMock.Object))
                                             .Returns(AuthenticationResult.Success);

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
            _clientAuthentication.Authenticate(ConnectionInfoMock.Object, SessionMock.Object);
        }
    }
}

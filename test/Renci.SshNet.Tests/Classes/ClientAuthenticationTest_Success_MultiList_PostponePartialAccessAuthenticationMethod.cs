using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    internal class ClientAuthenticationTest_Success_MultiList_PostponePartialAccessAuthenticationMethod : ClientAuthenticationTestBase
    {
        private int _partialSuccessLimit;
        private ClientAuthentication _clientAuthentication;

        protected override void SetupData()
        {
            _partialSuccessLimit = 3;
        }

        protected override void SetupMocks()
        {
            var seq = new MockSequence();

            _ = SessionMock.InSequence(seq)
                           .Setup(p => p.RegisterMessage("SSH_MSG_USERAUTH_FAILURE"));
            _ = SessionMock.InSequence(seq)
                           .Setup(p => p.RegisterMessage("SSH_MSG_USERAUTH_SUCCESS"));
            _ = SessionMock.InSequence(seq)
                .Setup(p => p.RegisterMessage("SSH_MSG_USERAUTH_BANNER"));
            _ = ConnectionInfoMock.InSequence(seq)
                                  .Setup(p => p.CreateNoneAuthenticationMethod())
                                  .Returns(NoneAuthenticationMethodMock.Object);
            _ = NoneAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.Authenticate(SessionMock.Object))
                                            .Returns(AuthenticationResult.Failure);
            _ = ConnectionInfoMock.InSequence(seq)
                                  .Setup(p => p.AuthenticationMethods)
                                  .Returns(new List<IAuthenticationMethod>
                                      {
                                          KeyboardInteractiveAuthenticationMethodMock.Object,
                                          PasswordAuthenticationMethodMock.Object,
                                          PublicKeyAuthenticationMethodMock.Object
                                      });
            _ = NoneAuthenticationMethodMock.InSequence(seq)
                                            .Setup(p => p.AllowedAuthentications)
                                            .Returns(new[] { "password" });
            _ = KeyboardInteractiveAuthenticationMethodMock.InSequence(seq)
                                                           .Setup(p => p.Name)
                                                           .Returns("keyboard-interactive");
            _ = PasswordAuthenticationMethodMock.InSequence(seq)
                                                .Setup(p => p.Name)
                                                .Returns("password");
            _ = PublicKeyAuthenticationMethodMock.InSequence(seq)
                                                 .Setup(p => p.Name)
                                                 .Returns("publickey");
            _ = PasswordAuthenticationMethodMock.InSequence(seq)
                                                .Setup(p => p.Authenticate(SessionMock.Object))
                                                .Returns(AuthenticationResult.PartialSuccess);
            _ = PasswordAuthenticationMethodMock.InSequence(seq)
                                                .Setup(p => p.AllowedAuthentications)
                                                .Returns(new[] {"password", "publickey"});
            _ = KeyboardInteractiveAuthenticationMethodMock.InSequence(seq)
                                                           .Setup(p => p.Name)
                                                           .Returns("keyboard-interactive");
            _ = PasswordAuthenticationMethodMock.InSequence(seq)
                                                .Setup(p => p.Name)
                                                .Returns("password");
            _ = PublicKeyAuthenticationMethodMock.InSequence(seq)
                                                 .Setup(p => p.Name)
                                                 .Returns("publickey");
            _ = PublicKeyAuthenticationMethodMock.InSequence(seq)
                                                 .Setup(p => p.Authenticate(SessionMock.Object))
                                                 .Returns(AuthenticationResult.Failure);
            _ = PublicKeyAuthenticationMethodMock.InSequence(seq)
                                                 .Setup(p => p.Name)
                                                 .Returns("publickey");
            _ = PasswordAuthenticationMethodMock.InSequence(seq)
                                                .Setup(p => p.Authenticate(SessionMock.Object))
                                                .Returns(AuthenticationResult.Success);
            _ = SessionMock.InSequence(seq)
                           .Setup(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_FAILURE"));
            _ = SessionMock.InSequence(seq)
                           .Setup(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_SUCCESS"));
            _ = SessionMock.InSequence(seq)
                           .Setup(p => p.UnRegisterMessage("SSH_MSG_USERAUTH_BANNER"));
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

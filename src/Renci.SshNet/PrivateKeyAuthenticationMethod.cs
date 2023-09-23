using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to perform private key authentication.
    /// </summary>
    public class PrivateKeyAuthenticationMethod : AuthenticationMethod, IDisposable
    {
        private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;
        private EventWaitHandle _authenticationCompleted = new ManualResetEvent(initialState: false);
        private bool _isSignatureRequired;
        private bool _isDisposed;

        /// <summary>
        /// Gets the name of the authentication method.
        /// </summary>
        public override string Name
        {
            get { return "publickey"; }
        }

        /// <summary>
        /// Gets the key files used for authentication.
        /// </summary>
        public ICollection<IPrivateKeySource> KeyFiles { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="keyFiles">The key files.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or <c>null</c>.</exception>
        public PrivateKeyAuthenticationMethod(string username, params IPrivateKeySource[] keyFiles)
            : base(username)
        {
            if (keyFiles is null)
            {
                throw new ArgumentNullException(nameof(keyFiles));
            }

            KeyFiles = new Collection<IPrivateKeySource>(keyFiles);
        }

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to authenticate.</param>
        /// <returns>
        /// Result of authentication  process.
        /// </returns>
        public override AuthenticationResult Authenticate(Session session)
        {
            session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;
            session.UserAuthenticationPublicKeyReceived += Session_UserAuthenticationPublicKeyReceived;

            session.RegisterMessage("SSH_MSG_USERAUTH_PK_OK");

            var hostAlgorithms = KeyFiles.SelectMany(x => x.HostKeyAlgorithms).ToList();

            try
            {
                foreach (var hostAlgorithm in hostAlgorithms)
                {
                    _ = _authenticationCompleted.Reset();
                    _isSignatureRequired = false;

                    var message = new RequestMessagePublicKey(ServiceName.Connection,
                                                              Username,
                                                              hostAlgorithm.Name,
                                                              hostAlgorithm.Data);

                    if (hostAlgorithms.Count == 1)
                    {
                        // If only one key file provided then send signature for very first request
                        var signatureData = new SignatureData(message, session.SessionId).GetBytes();

                        message.Signature = hostAlgorithm.Sign(signatureData);
                    }

                    // Send public key authentication request
                    session.SendMessage(message);

                    session.WaitOnHandle(_authenticationCompleted);

                    if (_isSignatureRequired)
                    {
                        _ = _authenticationCompleted.Reset();

                        var signatureMessage = new RequestMessagePublicKey(ServiceName.Connection,
                                                                           Username,
                                                                           hostAlgorithm.Name,
                                                                           hostAlgorithm.Data);

                        var signatureData = new SignatureData(message, session.SessionId).GetBytes();

                        signatureMessage.Signature = hostAlgorithm.Sign(signatureData);

                        // Send public key authentication request with signature
                        session.SendMessage(signatureMessage);
                    }

                    session.WaitOnHandle(_authenticationCompleted);

                    if (_authenticationResult is AuthenticationResult.Success or AuthenticationResult.PartialSuccess)
                    {
                        break;
                    }
                }

                return _authenticationResult;
            }
            finally
            {
                session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
                session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
                session.UserAuthenticationPublicKeyReceived -= Session_UserAuthenticationPublicKeyReceived;
                session.UnRegisterMessage("SSH_MSG_USERAUTH_PK_OK");
            }
        }

        private void Session_UserAuthenticationSuccessReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            _authenticationResult = AuthenticationResult.Success;

            _ = _authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            if (e.Message.PartialSuccess)
            {
                _authenticationResult = AuthenticationResult.PartialSuccess;
            }
            else
            {
                _authenticationResult = AuthenticationResult.Failure;
            }

            // Copy allowed authentication methods
            AllowedAuthentications = e.Message.AllowedAuthentications;

            _ = _authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationPublicKeyReceived(object sender, MessageEventArgs<PublicKeyMessage> e)
        {
            _isSignatureRequired = true;
            _ = _authenticationCompleted.Set();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                var authenticationCompleted = _authenticationCompleted;
                if (authenticationCompleted != null)
                {
                    _authenticationCompleted = null;
                    authenticationCompleted.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PrivateKeyAuthenticationMethod"/> class.
        /// </summary>
        ~PrivateKeyAuthenticationMethod()
        {
            Dispose(disposing: false);
        }

        private sealed class SignatureData : SshData
        {
            private readonly RequestMessagePublicKey _message;

            private readonly byte[] _sessionId;
            private readonly byte[] _serviceName;
            private readonly byte[] _authenticationMethod;

            protected override int BufferCapacity
            {
                get
                {
                    var capacity = base.BufferCapacity;
                    capacity += 4; // SessionId length
                    capacity += _sessionId.Length; // SessionId
                    capacity += 1; // Authentication Message Code
                    capacity += 4; // UserName length
                    capacity += _message.Username.Length; // UserName
                    capacity += 4; // ServiceName length
                    capacity += _serviceName.Length; // ServiceName
                    capacity += 4; // AuthenticationMethod length
                    capacity += _authenticationMethod.Length; // AuthenticationMethod
                    capacity += 1; // TRUE
                    capacity += 4; // PublicKeyAlgorithmName length
                    capacity += _message.PublicKeyAlgorithmName.Length; // PublicKeyAlgorithmName
                    capacity += 4; // PublicKeyData length
                    capacity += _message.PublicKeyData.Length; // PublicKeyData
                    return capacity;
                }
            }

            public SignatureData(RequestMessagePublicKey message, byte[] sessionId)
            {
                _message = message;
                _sessionId = sessionId;
                _serviceName = ServiceName.Connection.ToArray();
                _authenticationMethod = Ascii.GetBytes("publickey");
            }

            protected override void LoadData()
            {
                throw new NotImplementedException();
            }

            protected override void SaveData()
            {
                WriteBinaryString(_sessionId);
                Write((byte) RequestMessage.AuthenticationMessageCode);
                WriteBinaryString(_message.Username);
                WriteBinaryString(_serviceName);
                WriteBinaryString(_authenticationMethod);
                Write((byte)1); // TRUE
                WriteBinaryString(_message.PublicKeyAlgorithmName);
                WriteBinaryString(_message.PublicKeyData);
            }
        }
    }
}

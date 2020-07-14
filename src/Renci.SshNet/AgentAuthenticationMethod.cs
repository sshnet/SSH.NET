#if NETFRAMEWORK && !NET20 && !NET35
using System;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;

namespace Renci.SshNet {
    /// <summary>
    /// Provides functionality to perform private key authentication.
    /// </summary>
    public class AgentAuthenticationMethod : AuthenticationMethod, IDisposable {
        private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;
        private EventWaitHandle _authenticationCompleted = new ManualResetEvent (false);
        private bool _isSignatureRequired;

        /// <summary>
        /// Gets authentication method name
        /// </summary>
        public override string Name {
            get { return "publickey"; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IAgentProtocol Protocol { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="protocol">The key files.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or null.</exception>
        public AgentAuthenticationMethod (string username, IAgentProtocol protocol) : base (username) {
            Protocol = protocol;
        }

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to authenticate.</param>
        /// <returns></returns>
        public override AuthenticationResult Authenticate (Session session) {
            if (Protocol == null)
                return AuthenticationResult.Failure;

            session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;
            session.UserAuthenticationPublicKeyReceived += Session_UserAuthenticationPublicKeyReceived;

            session.RegisterMessage ("SSH_MSG_USERAUTH_PK_OK");

            try {
                foreach (var identity in Protocol.GetIdentities ()) {
                    _authenticationCompleted.Reset ();
                    _isSignatureRequired = false;

                    var message = new RequestMessagePublicKey (ServiceName.Connection,
                        Username,
                        identity.Type,
                        identity.Blob);

                    //  Send public key authentication request
                    session.SendMessage (message);

                    session.WaitOnHandle (_authenticationCompleted);

                    if (_isSignatureRequired) {
                        _authenticationCompleted.Reset ();

                        var signatureMessage = new RequestMessagePublicKey (ServiceName.Connection,
                            Username,
                            identity.Type,
                            identity.Blob);

                        var signatureData = new SignatureData (message, session.SessionId).GetBytes ();

                        signatureMessage.Signature = this.Protocol.SignData (identity, signatureData);

                        //  Send public key authentication request with signature
                        session.SendMessage (signatureMessage);
                    }

                    session.WaitOnHandle (_authenticationCompleted);

                    if (_authenticationResult == AuthenticationResult.Success) {
                        break;
                    }
                }
                return _authenticationResult;
            } finally {
                session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
                session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
                session.UserAuthenticationPublicKeyReceived -= Session_UserAuthenticationPublicKeyReceived;
                session.UnRegisterMessage ("SSH_MSG_USERAUTH_PK_OK");
            }
        }

        private void Session_UserAuthenticationSuccessReceived (object sender, MessageEventArgs<SuccessMessage> e) {
            this._authenticationResult = AuthenticationResult.Success;

            this._authenticationCompleted.Set ();
        }

        private void Session_UserAuthenticationFailureReceived (object sender, MessageEventArgs<FailureMessage> e) {
            if (e.Message.PartialSuccess)
                _authenticationResult = AuthenticationResult.PartialSuccess;
            else
                _authenticationResult = AuthenticationResult.Failure;

            //  Copy allowed authentication methods
            AllowedAuthentications = e.Message.AllowedAuthentications;

            _authenticationCompleted.Set ();
        }

        private void Session_UserAuthenticationPublicKeyReceived (object sender, MessageEventArgs<PublicKeyMessage> e) {
            this._isSignatureRequired = true;
            this._authenticationCompleted.Set ();
        }

        #region IDisposable Members

        private bool _isDisposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose () {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose (bool disposing) {
            // Check to see if Dispose has already been called.
            if (!_isDisposed)
                return;

            if (disposing) {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing) {
                    var authenticationCompleted = _authenticationCompleted;
                    // Dispose managed resources.
                    if (this._authenticationCompleted != null) {
                        _authenticationCompleted = null;
                        authenticationCompleted.Dispose ();
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PasswordConnectionInfo"/> is reclaimed by garbage collection.
        /// </summary>
        ~AgentAuthenticationMethod () {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose (false);
        }

        #endregion

        private class SignatureData : SshData {
            private readonly RequestMessagePublicKey _message;

            private byte[] _sessionId;
            private readonly byte[] _serviceName;
            private readonly byte[] _authenticationMethod;

            protected override int BufferCapacity {
                get {
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

            public SignatureData (RequestMessagePublicKey message, byte[] sessionId) {
                _message = message;
                _sessionId = sessionId;
                _serviceName = Ascii.GetBytes ("ssh-connection");
                _authenticationMethod = Ascii.GetBytes ("publickey");
            }

            protected override void LoadData () {
                throw new System.NotImplementedException ();
            }

            protected override void SaveData () {
                WriteBinaryString (_sessionId);
                Write ((byte) RequestMessage.AuthenticationMessageCode);
                WriteBinaryString (_message.Username);
                WriteBinaryString (_serviceName);
                WriteBinaryString (_authenticationMethod);
                Write ((byte) 1); // TRUE
                WriteBinaryString (_message.PublicKeyAlgorithmName);
                WriteBinaryString (_message.PublicKeyData);
            }
        }
    }
}
#endif
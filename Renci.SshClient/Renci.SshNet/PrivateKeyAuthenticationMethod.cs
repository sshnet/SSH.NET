using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages;
using Renci.SshNet.Common;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to perform private key authentication.
    /// </summary>
    public class PrivateKeyAuthenticationMethod : AuthenticationMethod, IDisposable
    {
        private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;

        private EventWaitHandle _authenticationCompleted = new ManualResetEvent(false);

        private bool _isSignatureRequired;

        /// <summary>
        /// Gets authentication method name
        /// </summary>
        public override string Name
        {
            get { return "publickey"; }
        }

        /// <summary>
        /// Gets the key files used for authentication.
        /// </summary>
        public ICollection<PrivateKeyFile> KeyFiles { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="keyFiles">The key files.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or null.</exception>
        public PrivateKeyAuthenticationMethod(string username, params PrivateKeyFile[] keyFiles)
            : base(username)
        {
            if (keyFiles == null)
                throw new ArgumentNullException("keyFiles");

            this.KeyFiles = new Collection<PrivateKeyFile>(keyFiles);
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
            session.MessageReceived += Session_MessageReceived;

            session.RegisterMessage("SSH_MSG_USERAUTH_PK_OK");

            foreach (var keyFile in this.KeyFiles)
            {
                this._authenticationCompleted.Reset();
                this._isSignatureRequired = false;

                var message = new RequestMessagePublicKey(ServiceName.Connection, this.Username, keyFile.HostKey.Name, keyFile.HostKey.Data);

                if (this.KeyFiles.Count < 2)
                {
                    //  If only one key file provided then send signature for very first request
                    var signatureData = new SignatureData(message, session.SessionId).GetBytes();

                    message.Signature = keyFile.HostKey.Sign(signatureData);
                }

                //  Send public key authentication request
                session.SendMessage(message);

                session.WaitOnHandle(this._authenticationCompleted);

                if (this._isSignatureRequired)
                {
                    this._authenticationCompleted.Reset();

                    var signatureMessage = new RequestMessagePublicKey(ServiceName.Connection, this.Username, keyFile.HostKey.Name, keyFile.HostKey.Data);

                    var signatureData = new SignatureData(message, session.SessionId).GetBytes();

                    signatureMessage.Signature = keyFile.HostKey.Sign(signatureData);

                    //  Send public key authentication request with signature
                    session.SendMessage(signatureMessage);
                }

                session.WaitOnHandle(this._authenticationCompleted);

                if (this._authenticationResult == AuthenticationResult.Success)
                {
                    break;
                }
            }
            
            session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
            session.MessageReceived -= Session_MessageReceived;

            session.UnRegisterMessage("SSH_MSG_USERAUTH_PK_OK");

            return this._authenticationResult;
        }

        private void Session_UserAuthenticationSuccessReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            this._authenticationResult = AuthenticationResult.Success;

            this._authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            if (e.Message.PartialSuccess)
                this._authenticationResult = AuthenticationResult.PartialSuccess;
            else
                this._authenticationResult = AuthenticationResult.Failure;

            //  Copy allowed authentication methods
            this.AllowedAuthentications = e.Message.AllowedAuthentications.ToList();

            this._authenticationCompleted.Set();
        }

        private void Session_MessageReceived(object sender, MessageEventArgs<Message> e)
        {
            var publicKeyMessage = e.Message as PublicKeyMessage;
            if (publicKeyMessage != null)
            {
                this._isSignatureRequired = true;
                this._authenticationCompleted.Set();
            }
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._authenticationCompleted != null)
                    {
                        this._authenticationCompleted.Dispose();
                        this._authenticationCompleted = null;
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
        ~PrivateKeyAuthenticationMethod()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

        private class SignatureData : SshData
        {
            private readonly RequestMessagePublicKey _message;

            private readonly byte[] _sessionId;

            public SignatureData(RequestMessagePublicKey message, byte[] sessionId)
            {
                this._message = message;
                this._sessionId = sessionId;
            }

            protected override void LoadData()
            {
                throw new NotImplementedException();
            }

            protected override void SaveData()
            {
                this.WriteBinaryString(this._sessionId);
                this.Write((byte)50);
                this.Write(this._message.Username);
                this.WriteAscii("ssh-connection");
                this.WriteAscii("publickey");
                this.Write((byte)1);
                this.WriteAscii(this._message.PublicKeyAlgorithmName);
                this.WriteBinaryString(this._message.PublicKeyData);
            }
        }

    }
}

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using System;
using System.Threading;


namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to perform private key authentication using a 
    /// signed public key component (a certificate).
    /// </summary>
    public class PrivateKeyCertAuthenticationMethod : AuthenticationMethod, IDisposable
    {
        private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;
        private EventWaitHandle _authenticationCompleted = new ManualResetEvent(false);

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
        public PrivateKeyFile KeyFile { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public PublicKeyCertFile CertificateFile { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="keyFile">The key files.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or <c>null</c>.</exception>
        public PrivateKeyCertAuthenticationMethod(string username, PrivateKeyFile keyFile)
            : base(username)
        {
            if (keyFile == null)
                throw new ArgumentNullException("keyFiles");

            KeyFile = keyFile;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="keyFile">The key files.</param>
        /// <param name="certFile"></param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or <c>null</c>.</exception>
        public PrivateKeyCertAuthenticationMethod(string username, PrivateKeyFile keyFile, PublicKeyCertFile certFile)
            : base(username)
        {
            if (keyFile == null)
                throw new ArgumentNullException("keyFile");

            if (certFile == null)
                throw new ArgumentNullException("certFile");

            KeyFile = keyFile;
            CertificateFile = certFile;
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

            try
            {
                _authenticationCompleted.Reset();

                var message = new RequestMessagePublicKey(ServiceName.Connection,
                                                            Username,
                                                            CertificateFile.HostCertificate.Name,
                                                            CertificateFile.HostCertificate.Data);

                //  Send signature for very first request
                var signatureData = new SshSignatureData(message, session.SessionId).GetBytes();
                message.Signature = KeyFile.HostKey.Sign(signatureData);

                //  Send public key authentication request
                session.SendMessage(message);

                session.WaitOnHandle(_authenticationCompleted);

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

            _authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            if (e.Message.PartialSuccess)
                _authenticationResult = AuthenticationResult.PartialSuccess;
            else
                _authenticationResult = AuthenticationResult.Failure;

            //  Copy allowed authentication methods
            AllowedAuthentications = e.Message.AllowedAuthentications;

            _authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationPublicKeyReceived(object sender, MessageEventArgs<PublicKeyMessage> e)
        {
            _authenticationCompleted.Set();
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
            if (_isDisposed)
                return;

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
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PasswordConnectionInfo"/> is reclaimed by garbage collection.
        /// </summary>
        ~PrivateKeyCertAuthenticationMethod()
        {
            Dispose(false);
        }

        #endregion
    }
}

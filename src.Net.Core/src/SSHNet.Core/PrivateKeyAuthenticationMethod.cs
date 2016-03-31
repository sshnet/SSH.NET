﻿using System;
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

            KeyFiles = new Collection<PrivateKeyFile>(keyFiles);
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

            foreach (var keyFile in KeyFiles)
            {
                _authenticationCompleted.Reset();
                _isSignatureRequired = false;

                var message = new RequestMessagePublicKey(ServiceName.Connection, Username, keyFile.HostKey.Name, keyFile.HostKey.Data);

                if (KeyFiles.Count < 2)
                {
                    //  If only one key file provided then send signature for very first request
                    var signatureData = new SignatureData(message, session.SessionId).GetBytes();

                    message.Signature = keyFile.HostKey.Sign(signatureData);
                }

                //  Send public key authentication request
                session.SendMessage(message);

                session.WaitOnHandle(_authenticationCompleted);

                if (_isSignatureRequired)
                {
                    _authenticationCompleted.Reset();

                    var signatureMessage = new RequestMessagePublicKey(ServiceName.Connection, Username, keyFile.HostKey.Name, keyFile.HostKey.Data);

                    var signatureData = new SignatureData(message, session.SessionId).GetBytes();

                    signatureMessage.Signature = keyFile.HostKey.Sign(signatureData);

                    //  Send public key authentication request with signature
                    session.SendMessage(signatureMessage);
                }

                session.WaitOnHandle(_authenticationCompleted);

                if (_authenticationResult == AuthenticationResult.Success)
                {
                    break;
                }
            }
            
            session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
            session.MessageReceived -= Session_MessageReceived;

            session.UnRegisterMessage("SSH_MSG_USERAUTH_PK_OK");

            return _authenticationResult;
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
            AllowedAuthentications = e.Message.AllowedAuthentications.ToList();

            _authenticationCompleted.Set();
        }

        private void Session_MessageReceived(object sender, MessageEventArgs<Message> e)
        {
            var publicKeyMessage = e.Message as PublicKeyMessage;
            if (publicKeyMessage != null)
            {
                _isSignatureRequired = true;
                _authenticationCompleted.Set();
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
            if (!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_authenticationCompleted != null)
                    {
                        _authenticationCompleted.Dispose();
                        _authenticationCompleted = null;
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
            private readonly byte[] _serviceName;
            private readonly byte[] _authenticationMethod;

#if true //old TUNING
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
#endif

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides connection information when private key authentication method is used
    /// </summary>
    public class PrivateKeyConnectionInfo : ConnectionInfo, IDisposable
    {
        private EventWaitHandle _publicKeyRequestMessageResponseWaitHandle = new ManualResetEvent(false);

        private bool _isSignatureRequired;
        
        /// <summary>
        /// Gets connection name
        /// </summary>
        public override string Name
        {
            get
            {
                return "publickey";
            }
        }

        /// <summary>
        /// Gets the key files used for authentication.
        /// </summary>
        public ICollection<PrivateKeyFile> KeyFiles { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="keyFiles">Connection key files.</param>
        public PrivateKeyConnectionInfo(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, 22, username, keyFiles)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="keyFiles">Connection key files.</param>
        public PrivateKeyConnectionInfo(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : base(host, port, username)
        {
            this.KeyFiles = new Collection<PrivateKeyFile>(keyFiles);
        }

        /// <summary>
        /// Called when connection needs to be authenticated.
        /// </summary>
        protected override void OnAuthenticate()
        {
            if (this.KeyFiles == null)
                return;

            this.Session.RegisterMessage("SSH_MSG_USERAUTH_PK_OK");

            foreach (var keyFile in this.KeyFiles)
            {
                this._publicKeyRequestMessageResponseWaitHandle.Reset();
                this._isSignatureRequired = false;

                var message = new RequestMessagePublicKey(ServiceName.Connection, this.Username, keyFile.HostKey.Name, keyFile.HostKey.Data);

                if (this.KeyFiles.Count < 2)
                {
                    //  If only one key file provided then send signature for very first request
                    var signatureData = new SignatureData(message, this.Session.SessionId).GetBytes();

                    message.Signature = keyFile.HostKey.Sign(signatureData);
                }

                //  Send public key authentication request
                this.SendMessage(message);

                this.WaitHandle(this._publicKeyRequestMessageResponseWaitHandle);

                if (this._isSignatureRequired)
                {
                    this._publicKeyRequestMessageResponseWaitHandle.Reset();

                    var signatureMessage = new RequestMessagePublicKey(ServiceName.Connection, this.Username, keyFile.HostKey.Name, keyFile.HostKey.Data);

                    var signatureData = new SignatureData(message, this.Session.SessionId).GetBytes();

                    signatureMessage.Signature = keyFile.HostKey.Sign(signatureData);

                    //  Send public key authentication request with signature
                    this.SendMessage(signatureMessage);
                }

                this.WaitHandle(this._publicKeyRequestMessageResponseWaitHandle);

                if (this.IsAuthenticated)
                {
                    break;
                }
            }

            this.Session.UnRegisterMessage("SSH_MSG_USERAUTH_PK_OK");
        }

        /// <summary>
        /// Handles the UserAuthenticationSuccessMessageReceived event of the session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        protected override void Session_UserAuthenticationSuccessMessageReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            base.Session_UserAuthenticationSuccessMessageReceived(sender, e);

            this._publicKeyRequestMessageResponseWaitHandle.Set();
        }

        /// <summary>
        /// Handles the UserAuthenticationFailureReceived event of the session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        protected override void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            base.Session_UserAuthenticationFailureReceived(sender, e);
            this._publicKeyRequestMessageResponseWaitHandle.Set();
        }

        /// <summary>
        /// Handles the MessageReceived event of the session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        protected override void Session_MessageReceived(object sender, MessageEventArgs<Message> e)
        {
            base.Session_MessageReceived(sender, e);

            var publicKeyMessage = e.Message as PublicKeyMessage;
            if (publicKeyMessage != null)
            {
                this._isSignatureRequired = true;
                this._publicKeyRequestMessageResponseWaitHandle.Set();
            }
        }

        private class SignatureData : SshData
        {
            private RequestMessagePublicKey _message;

            private byte[] _sessionId;

            public SignatureData(RequestMessagePublicKey message, byte[] sessionId)
            {
                this._message = message;
                this._sessionId = sessionId;
            }

            protected override void LoadData()
            {
                throw new System.NotImplementedException();
            }

            protected override void SaveData()
            {
                this.WriteBinaryString(this._sessionId);
                this.Write((byte)50);
                this.Write(this._message.Username);
                this.Write("ssh-connection");
                this.Write("publickey");
                this.Write((byte)1);
                this.Write(this._message.PublicKeyAlgorithmName);
                this.WriteBinaryString(this._message.PublicKeyData);
            }
        }

        #region IDisposable Members

        private bool _isDisposed = false;

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
                    if (this._publicKeyRequestMessageResponseWaitHandle != null)
                    {
                        this._publicKeyRequestMessageResponseWaitHandle.Dispose();
                        this._publicKeyRequestMessageResponseWaitHandle = null;
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PrivateKeyConnectionInfo"/> is reclaimed by garbage collection.
        /// </summary>
        ~PrivateKeyConnectionInfo()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

    }
}

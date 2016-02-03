using System;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;

namespace Renci.SshClient.Security
{
    internal class UserAuthenticationPublicKey : UserAuthentication, IDisposable
    {
        private EventWaitHandle _publicKeyRequestMessageResponseWaitHandle = new ManualResetEvent(false);

        private bool _isSignatureRequired;

        public override string Name
        {
            get
            {
                return "publickey";
            }
        }

        protected override void OnAuthenticate()
        {
            var privateKeyConnectionInfo = this.Session.ConnectionInfo as PrivateKeyConnectionInfo;

            if (privateKeyConnectionInfo == null)
                return;

            if (privateKeyConnectionInfo.KeyFiles == null)
                return;

            this.Session.RegisterMessageType<PublicKeyMessage>(MessageTypes.UserAuthenticationPublicKey);

            foreach (var keyFile in privateKeyConnectionInfo.KeyFiles)
            {
                this._publicKeyRequestMessageResponseWaitHandle.Reset();
                this._isSignatureRequired = false;

                var message = new RequestMessagePublicKey(ServiceNames.Connection,this.Username, keyFile.AlgorithmName, keyFile.PublicKey);

                if (privateKeyConnectionInfo.KeyFiles.Count < 2)
                {
                    //  If only one key file provided then send signature for very first request
                    var signatureData = new SignatureData(message, this.Session.SessionId.GetSshString()).GetBytes();

                    message.Signature = keyFile.GetSignature(signatureData);
                }

                //  Send public key authentication request
                this.SendMessage(message);

                this.WaitHandle(this._publicKeyRequestMessageResponseWaitHandle);

                if (this._isSignatureRequired)
                {
                    this._publicKeyRequestMessageResponseWaitHandle.Reset();

                    var signatureMessage = new RequestMessagePublicKey(ServiceNames.Connection, this.Session.ConnectionInfo.Username, keyFile.AlgorithmName, keyFile.PublicKey);

                    var signatureData = new SignatureData(message, this.Session.SessionId.GetSshString()).GetBytes();

                    signatureMessage.Signature = keyFile.GetSignature(signatureData);
                    
                    //  Send public key authentication request with signature
                    this.SendMessage(signatureMessage); 
                }

                this.WaitHandle(this._publicKeyRequestMessageResponseWaitHandle);

                if (this.IsAuthenticated)
                {
                    break;
                }            
            }

            this.Session.UnRegisterMessageType(MessageTypes.UserAuthenticationPublicKey);
        }

        protected override void Session_UserAuthenticationSuccessMessageReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            base.Session_UserAuthenticationSuccessMessageReceived(sender, e);

            this._publicKeyRequestMessageResponseWaitHandle.Set();
        }

        protected override void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            base.Session_UserAuthenticationFailureReceived(sender, e);
            this._publicKeyRequestMessageResponseWaitHandle.Set();
        }

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

            private string _sessionId;

            public SignatureData(RequestMessagePublicKey message, string sessionId)
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
                this.Write(this._sessionId);
                this.Write((byte)this._message.MessageType);
                this.Write(this._message.Username);
                this.Write("ssh-connection");
                this.Write("publickey");
                this.Write((byte)1);
                this.Write(this._message.PublicKeyAlgorithmName);
                this.Write(this._message.PublicKeyData.GetSshString());
            }
        }

        #region IDisposable Members

        private bool _isDisposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

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
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        ~UserAuthenticationPublicKey()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}

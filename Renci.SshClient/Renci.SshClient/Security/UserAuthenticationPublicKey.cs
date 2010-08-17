using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;

namespace Renci.SshClient.Security
{
    internal class UserAuthenticationPublicKey : UserAuthentication
    {
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        public override string Name
        {
            get
            {
                return "publickey";
            }
        }

        public UserAuthenticationPublicKey(Session session)
            : base(session)
        {

        }

        protected override bool Run()
        {
            if (this.Session.ConnectionInfo.KeyFile == null)
                return false;

            Message.RegisterMessageType<InformationRequestMessage>(MessageTypes.UserAuthenticationInformationRequest);


            //  TODO:   Complete full public key implemention which includes other messages
            var message = new PublicKeyRequestMessage
            {
                ServiceName = ServiceNames.Connection,
                Username = this.Session.ConnectionInfo.Username,
                PublicKeyAlgorithmName = this.Session.ConnectionInfo.KeyFile.AlgorithmName,
                PublicKeyData = this.Session.ConnectionInfo.KeyFile.PublicKey,
                Signature = new byte[] { },
                //Signature = this.Session.ConnectionInfo.KeyFile.GetSignature(this.Session.SessionId),
            };

            var signatureData = new SignatureData(message, this.Session.SessionId.GetSshString()).GetBytes();

            var signature = this.Session.ConnectionInfo.KeyFile.GetSignature(signatureData);

            message.Signature = signature;

            this.Session.SendMessage(message);

            this.Session.WaitHandle(this._authenticationCompleted);

            Message.UnRegisterMessageType(MessageTypes.UserAuthenticationInformationRequest);


            return true;
        }

        protected override void HandleMessage<T>(T message)
        {
            throw new System.NotImplementedException();
        }

        protected override void HandleMessage(SuccessMessage message)
        {
            base.HandleMessage(message);
            this._authenticationCompleted.Set();
        }

        protected override void HandleMessage(FailureMessage message)
        {
            base.HandleMessage(message);
            this._authenticationCompleted.Set();
        }

        private class SignatureData : SshData
        {

            private PublicKeyRequestMessage _message;

            private string _sessionId;

            public SignatureData(PublicKeyRequestMessage message, string sessionId)
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
    }
}

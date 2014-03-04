using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal abstract class SftpRequest : SftpMessage
    {
        private readonly Action<SftpStatusResponse> _statusAction;
        private Action<SftpAttrsResponse> _attrsAction;
        private Action<SftpDataResponse> _dataAction;
        private Action<SftpExtendedReplyResponse> _extendedReplyAction;
        private Action<SftpHandleResponse> _handleAction;
        private Action<SftpNameResponse> _nameAction;

        public uint RequestId { get; private set; }
        
        public uint ProtocolVersion { get; private set; }

        public SftpRequest(uint protocolVersion, uint requestId, Action<SftpStatusResponse> statusAction)
        {
            this.RequestId = requestId;
            this.ProtocolVersion = protocolVersion;
            this._statusAction = statusAction;
        }

        public void Complete(SftpResponse response)
        {
            if (response is SftpStatusResponse)
            {
                this._statusAction(response as SftpStatusResponse);
            }
            else if (response is SftpAttrsResponse)
            {
                this._attrsAction(response as SftpAttrsResponse);
            }
            else if (response is SftpDataResponse)
            {
                this._dataAction(response as SftpDataResponse);
            }
            else if (response is SftpExtendedReplyResponse)
            {
                this._extendedReplyAction(response as SftpExtendedReplyResponse);
            }
            else if (response is SftpHandleResponse)
            {
                this._handleAction(response as SftpHandleResponse);
            }
            else if (response is SftpNameResponse)
            {
                this._nameAction(response as SftpNameResponse);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Response of type '{0}' is not expected.", response.GetType().Name));
            }
        }

        protected void SetAction(Action<SftpAttrsResponse> action)
        {
            this._attrsAction = action;
        }

        protected void SetAction(Action<SftpDataResponse> action)
        {
            this._dataAction = action;
        }

        protected void SetAction(Action<SftpExtendedReplyResponse> action)
        {
            this._extendedReplyAction = action;
        }

        protected void SetAction(Action<SftpHandleResponse> action)
        {
            this._handleAction = action;
        }

        protected void SetAction(Action<SftpNameResponse> action)
        {
            this._nameAction = action;
        }

        protected override void LoadData()
        {
            throw new InvalidOperationException("Request cannot be saved.");
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.RequestId);
        }
    }
}

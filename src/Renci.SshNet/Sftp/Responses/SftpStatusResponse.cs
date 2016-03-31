using System.Text;
using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpStatusResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Status; }
        }

        public SftpStatusResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        public StatusCodes StatusCode { get; private set; }

        public string ErrorMessage { get; private set; }

        public string Language { get; private set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.StatusCode = (StatusCodes)this.ReadUInt32();

            if (this.ProtocolVersion < 3)
            {
                return;
            }

            if (!this.IsEndOfData)
            {
                // the SSH File Transfer Protocol specification states that the error message is UTF-8
                this.ErrorMessage = this.ReadString(Utf8);

                // the language of the error message; RFC 1766 states that the language code may be
                // expressed as US-ASCII
                this.Language = this.ReadString(Ascii);
            }
        }
    }
}

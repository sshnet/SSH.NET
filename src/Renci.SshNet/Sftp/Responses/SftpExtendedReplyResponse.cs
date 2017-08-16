namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpExtendedReplyResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.ExtendedReply; }
        }

        public SftpExtendedReplyResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        public T GetReply<T>() where T : ExtendedReplyInfo, new()
        {
            var result = new T();
            result.LoadData(DataStream);
            return result;
        }
    }
}

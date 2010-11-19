namespace Renci.SshClient.Messages.Connection
{
    internal class SessionChannelOpenInfo : ChannelOpenInfo
    {
        public const string NAME = "session";

        public override string ChannelType
        {
            get { return SessionChannelOpenInfo.NAME; }
        }
    }
}

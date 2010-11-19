namespace Renci.SshClient.Messages.Connection
{
    internal class ShellRequestInfo : RequestInfo
    {
        public const string NAME = "shell";

        public override string RequestName
        {
            get { return ShellRequestInfo.NAME; }
        }
    }
}

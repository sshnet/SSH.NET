
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestShellMessage : ChannelRequestMessage
    {
        public const string REQUEST_NAME = "shell";

        protected override void LoadData()
        {
            base.LoadData();
        }

        protected override void SaveData()
        {
            this.RequestName = REQUEST_NAME;
            base.SaveData();
        }
    }
}

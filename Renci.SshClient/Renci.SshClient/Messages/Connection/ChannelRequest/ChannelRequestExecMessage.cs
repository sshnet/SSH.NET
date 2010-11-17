
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestExecMessage : ChannelRequestMessage
    {
        public const string REQUEST_NAME = "exec";

        public string Command { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.Command = this.ReadString();
        }

        protected override void SaveData()
        {
            this.RequestName = REQUEST_NAME;

            base.SaveData();

            this.Write(this.Command);
        }
    }
}

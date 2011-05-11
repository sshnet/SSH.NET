
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestSignalMessage : ChannelRequestMessage
    {
        public const string REQUEST_NAME = "signal";

        public string SignalName { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.SignalName = this.ReadString();
        }

        protected override void SaveData()
        {
            this.RequestName = REQUEST_NAME;

            base.SaveData();

            this.Write(this.SignalName);
        }
    }
}

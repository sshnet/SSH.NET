
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestExitStatusMessage : ChannelRequestMessage
    {
        public const string REQUEST_NAME = "exit-status";

        public uint ExitStatus { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.ExitStatus = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            this.RequestName = REQUEST_NAME;
            base.SaveData();
            this.Write(this.ExitStatus);
        }
    }
}


namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestSubsystemMessage : ChannelRequestMessage
    {
        public const string REQUEST_NAME = "subsystem";

        public string SubsystemName { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.SubsystemName = this.ReadString();
        }

        protected override void SaveData()
        {
            this.RequestName = REQUEST_NAME;

            base.SaveData();

            this.Write(this.SubsystemName);
        }
    }
}

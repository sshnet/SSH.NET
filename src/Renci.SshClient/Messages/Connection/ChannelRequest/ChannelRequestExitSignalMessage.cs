
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestExitSignalMessage : ChannelRequestMessage
    {
        public const string REQUEST_NAME = "exit-signal";

        public string SignalName { get; set; }

        public bool CoreDumped { get; set; }

        public string ErrorMessage { get; set; }

        public string Language { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.SignalName = this.ReadString();
            this.CoreDumped = this.ReadBoolean();
            this.ErrorMessage = this.ReadString();
            this.Language = this.ReadString();
        }

        protected override void SaveData()
        {
            this.RequestName = REQUEST_NAME;

            base.SaveData();

            this.Write(this.SignalName);
            this.Write(this.CoreDumped);
            this.Write(this.ErrorMessage);
            this.Write(this.Language);
        }
    }
}


namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestXonXoffMessage : ChannelRequestMessage
    {
        public const string REQUEST_NAME = "xon-xoff";

        public bool ClientCanDo { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.ClientCanDo = this.ReadBoolean();
        }

        protected override void SaveData()
        {
            this.RequestName = REQUEST_NAME;

            base.SaveData();

            this.Write(this.ClientCanDo);
        }
    }
}

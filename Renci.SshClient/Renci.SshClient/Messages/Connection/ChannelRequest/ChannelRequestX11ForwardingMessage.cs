
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestX11ForwardingMessage : ChannelRequestMessage
    {
        public const string REQUEST_NAME = "x11-req";

        public bool IsSingleConnection { get; set; }

        public string AuthenticationProtocol { get; set; }

        public string AuthenticationCookie { get; set; }

        public uint ScreenNumber { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.IsSingleConnection = this.ReadBoolean();
            this.AuthenticationProtocol = this.ReadString();
            this.AuthenticationCookie = this.ReadString();
            this.ScreenNumber = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            this.RequestName = REQUEST_NAME;

            base.SaveData();

            this.Write(this.IsSingleConnection);
            this.Write(this.AuthenticationProtocol);
            this.Write(this.AuthenticationCookie);
            this.Write(this.ScreenNumber);
        }
    }
}

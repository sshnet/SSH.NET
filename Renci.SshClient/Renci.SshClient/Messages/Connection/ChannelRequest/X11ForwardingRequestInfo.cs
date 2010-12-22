namespace Renci.SshClient.Messages.Connection
{
    internal class X11ForwardingRequestInfo : RequestInfo
    {
        public const string NAME = "x11-req";

        public override string RequestName
        {
            get { return X11ForwardingRequestInfo.NAME; }
        }

        public bool IsSingleConnection { get; set; }

        public string AuthenticationProtocol { get; set; }

        public string AuthenticationCookie { get; set; }

        public uint ScreenNumber { get; set; }

        public X11ForwardingRequestInfo()
        {
            this.WantReply = true;
        }

        public X11ForwardingRequestInfo(bool isSignleConnection, string protocol, string cookie, uint screenNumber)
            : this()
        {
            this.IsSingleConnection = isSignleConnection;
            this.AuthenticationProtocol = protocol;
            this.AuthenticationCookie = cookie;
            this.ScreenNumber = screenNumber;
        }

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
            base.SaveData();

            this.Write(this.IsSingleConnection);
            this.Write(this.AuthenticationProtocol);
            this.Write(this.AuthenticationCookie);
            this.Write(this.ScreenNumber);
        }
    }
}

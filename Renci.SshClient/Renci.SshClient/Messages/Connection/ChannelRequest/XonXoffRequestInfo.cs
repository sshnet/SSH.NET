namespace Renci.SshClient.Messages.Connection
{
    internal class XonXoffRequestInfo : RequestInfo
    {
        public const string NAME = "xon-xoff";

        public override string RequestName
        {
            get { return XonXoffRequestInfo.NAME; }
        }

        public bool ClientCanDo { get; set; }

        public XonXoffRequestInfo()
        {
            this.WantReply = false;
        }

        public XonXoffRequestInfo(bool clientCanDo)
            : this()
        {
            this.ClientCanDo = clientCanDo;
        }

        protected override void LoadData()
        {
            base.LoadData();

            this.ClientCanDo = this.ReadBoolean();
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.ClientCanDo);
        }
    }
}

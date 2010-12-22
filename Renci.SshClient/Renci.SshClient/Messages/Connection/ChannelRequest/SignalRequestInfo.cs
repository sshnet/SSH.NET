namespace Renci.SshClient.Messages.Connection
{
    internal class SignalRequestInfo : RequestInfo
    {
        public const string NAME = "signal";

        public override string RequestName
        {
            get { return SignalRequestInfo.NAME; }
        }

        public string SignalName { get; private set; }

        public SignalRequestInfo()
        {
            this.WantReply = false;
        }

        public SignalRequestInfo(string signalName)
            : this()
        {
            this.SignalName = signalName;
        }

        protected override void LoadData()
        {
            base.LoadData();

            this.SignalName = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.SignalName);
        }
    }
}

namespace Renci.SshClient.Messages.Connection
{
    internal class SignalRequestInfo : RequestInfo
    {
        public const string NAME = "signal";

        public override string RequestName
        {
            get { return SignalRequestInfo.NAME; }
        }

        public string SignalName { get; set; }

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

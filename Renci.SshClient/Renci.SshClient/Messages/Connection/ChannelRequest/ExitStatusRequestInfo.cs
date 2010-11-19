namespace Renci.SshClient.Messages.Connection
{
    internal class ExitStatusRequestInfo : RequestInfo
    {
        public const string NAME = "exit-status";

        public override string RequestName
        {
            get { return ExitStatusRequestInfo.NAME; }
        }

        public uint ExitStatus { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.ExitStatus = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.ExitStatus);
        }
    }
}

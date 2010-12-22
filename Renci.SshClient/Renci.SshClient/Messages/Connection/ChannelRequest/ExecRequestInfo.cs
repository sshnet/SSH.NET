namespace Renci.SshClient.Messages.Connection
{
    internal class ExecRequestInfo : RequestInfo
    {
        public const string NAME = "exec";

        public override string RequestName
        {
            get { return ExecRequestInfo.NAME; }
        }

        public string Command { get; private set; }

        public ExecRequestInfo()
        {

        }

        public ExecRequestInfo(string command)
        {
            this.WantReply = true;
            this.Command = command;
        }

        protected override void LoadData()
        {
            base.LoadData();

            this.Command = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.Command);
        }
    }
}

namespace Renci.SshClient.Messages.Connection
{
    internal class ExitSignalRequestInfo : RequestInfo
    {
        public const string NAME = "exit-signal";

        public override string RequestName
        {
            get { return ExitSignalRequestInfo.NAME; }
        }

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
            base.SaveData();

            this.Write(this.SignalName);
            this.Write(this.CoreDumped);
            this.Write(this.ErrorMessage);
            this.Write(this.Language);
        }

    }
}

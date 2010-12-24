using System.Text;
namespace Renci.SshClient.Messages.Connection
{
    internal class ExitSignalRequestInfo : RequestInfo
    {
        public const string NAME = "exit-signal";

        public override string RequestName
        {
            get { return ExitSignalRequestInfo.NAME; }
        }

        public string SignalName { get; private set; }

        public bool CoreDumped { get; private set; }

        public string ErrorMessage { get; private set; }

        public string Language { get; private set; }

        public ExitSignalRequestInfo()
        {
            this.WantReply = false;
        }

        public ExitSignalRequestInfo(string signalName, bool coreDumped, string errorMessage, string language)
            : this()
        {
            this.SignalName = signalName;
            this.CoreDumped = coreDumped;
            this.ErrorMessage = errorMessage;
            this.Language = language;
        }

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
            this.Write(this.Language, Encoding.UTF8);
        }

    }
}

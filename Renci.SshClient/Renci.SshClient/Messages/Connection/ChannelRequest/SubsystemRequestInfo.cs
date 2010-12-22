namespace Renci.SshClient.Messages.Connection
{
    internal class SubsystemRequestInfo : RequestInfo
    {
        public const string NAME = "subsystem";

        public override string RequestName
        {
            get { return SubsystemRequestInfo.NAME; }
        }

        public string SubsystemName { get; private set; }

        public SubsystemRequestInfo()
        {
            this.WantReply = true;
        }

        public SubsystemRequestInfo(string subsystem)
            : this()
        {
            this.SubsystemName = subsystem;
        }

        protected override void LoadData()
        {
            base.LoadData();

            this.SubsystemName = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.SubsystemName);
        }
    }
}

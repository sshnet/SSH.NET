namespace Renci.SshClient.Messages.Connection
{
    internal class SubsystemRequestInfo : RequestInfo
    {
        public const string NAME = "subsystem";

        public override string RequestName
        {
            get { return SubsystemRequestInfo.NAME; }
        }

        public string SubsystemName { get; set; }

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

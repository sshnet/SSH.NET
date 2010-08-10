namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelOpenFailureMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelOpenFailure; }
        }

        public uint ReasconCode { get; set; }

        public string Description { get; set; }

        public string Language { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.ReasconCode = this.ReadUInt32();
            this.Description = this.ReadString();
            this.Language = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.ReasconCode);
            this.Write(this.Description);
            this.Write(this.Language);
        }
    }
}

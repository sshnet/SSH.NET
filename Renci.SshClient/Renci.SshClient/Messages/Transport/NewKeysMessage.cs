namespace Renci.SshClient.Messages.Transport
{
    internal class NewKeysMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.NewKeys; }
        }

        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
        }
    }
}

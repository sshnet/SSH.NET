namespace Renci.SshClient.Messages.Transport
{
    [Message("SSH_MSG_NEWKEYS", 21)]
    public class NewKeysMessage : Message
    {
        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
        }
    }
}

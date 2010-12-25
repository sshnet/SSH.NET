
namespace Renci.SshClient.Messages.Transport
{
    [Message("SSH_MSG_UNIMPLEMENTED", 3)]
    public class UnimplementedMessage : Message
    {
        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
        }
    }
}


namespace Renci.SshClient.Messages.Connection
{
    [Message("SSH_MSG_REQUEST_FAILURE", 82)]
    public class RequestFailureMessage : Message
    {
        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
        }
    }
}

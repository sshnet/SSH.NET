using Renci.SshClient.Messages;
namespace Renci.SshClient.Services
{
    internal abstract class UserAuthentication
    {
        public abstract string Name { get; }

        protected SessionInfo SessionInfo { get; private set; }

        public UserAuthentication(SessionInfo sessionInfo)
        {
            this.SessionInfo = sessionInfo;
        }

        public abstract bool Start();

        protected void SendMessage(Message message)
        {
            this.SessionInfo.SendMessage(message);
        }

    }
}

using Renci.SshClient.Messages;

namespace Renci.SshClient.Services
{
    internal abstract class Service
    {
        public abstract ServiceNames ServiceName { get; }

        protected SessionInfo SessionInfo { get; private set; }

        public Service(SessionInfo sessionInfo)
        {
            this.SessionInfo = sessionInfo;
        }

        protected void SendMessage(Message message)
        {
            this.SessionInfo.SendMessage(message);
        }

    }
}

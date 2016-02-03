using Renci.SshClient.Messages;

namespace Renci.SshClient.Services
{
    internal abstract class Service
    {
        public abstract ServiceNames ServiceName { get; }

        protected Session Session { get; private set; }

        public Service(Session session)
        {
            this.Session = session;
        }

        protected void SendMessage(Message message)
        {
            this.Session.SendMessage(message);
        }

    }
}

using Renci.SshClient.Messages;
namespace Renci.SshClient.Services
{
    internal abstract class UserAuthentication
    {
        public abstract string Name { get; }

        protected Session Session { get; private set; }

        public UserAuthentication(Session session)
        {
            this.Session = session;
        }

        public abstract bool Start();

        protected void SendMessage(Message message)
        {
            this.Session.SendMessage(message);
        }

    }
}

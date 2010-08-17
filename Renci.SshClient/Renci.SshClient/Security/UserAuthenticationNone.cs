using System.Threading;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;

namespace Renci.SshClient.Security
{
    internal class UserAuthenticationNone : UserAuthentication
    {
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        public override string Name
        {
            get { return "none"; }
        }

        public UserAuthenticationNone(Session session)
            : base(session)
        {

        }

        protected override bool Run()
        {
            this.Session.SendMessage(new RequestMessage
            {
                ServiceName = ServiceNames.Connection,
                Username = this.Session.ConnectionInfo.Username,
            });

            this.Session.WaitHandle(this._authenticationCompleted);

            return true;
        }

        protected override void HandleMessage<T>(T message)
        {
        }

        protected override void HandleMessage(SuccessMessage message)
        {
            base.HandleMessage(message);
            this._authenticationCompleted.Set();
        }

        protected override void HandleMessage(FailureMessage message)
        {
            base.HandleMessage(message);
            this._authenticationCompleted.Set();
        }
    }
}

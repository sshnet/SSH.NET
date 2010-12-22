using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;
using System;
using Renci.SshClient.Common;
using System.Threading;

namespace Renci.SshClient.Security
{
    internal abstract class UserAuthentication
    {
        public abstract string Name { get; }

        public bool IsAuthenticated { get; private set; }

        public string ErrorMessage { get; private set; }

        public event EventHandler<AuthenticationEventArgs> Authenticating;

        protected Session Session { get; private set; }

        protected string Username { get; private set; }

        public bool Authenticate(string username, Session session)
        {
            this.Username = username;
            this.Session = session;

            this.Session.RegisterMessageType<FailureMessage>(MessageTypes.UserAuthenticationFailure);
            this.Session.RegisterMessageType<SuccessMessage>(MessageTypes.UserAuthenticationSuccess);
            this.Session.RegisterMessageType<BannerMessage>(MessageTypes.UserAuthenticationBanner);

            this.Session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;
            this.Session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessMessageReceived;
            this.Session.UserAuthenticationBannerReceived += Session_UserAuthenticationBannerMessageReceived;
            this.Session.MessageReceived += Session_MessageReceived;

            this.OnAuthenticate();

            this.Session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
            this.Session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessMessageReceived;
            this.Session.UserAuthenticationBannerReceived -= Session_UserAuthenticationBannerMessageReceived;
            this.Session.MessageReceived -= Session_MessageReceived;

            this.Session.UnRegisterMessageType(MessageTypes.UserAuthenticationFailure);
            this.Session.UnRegisterMessageType(MessageTypes.UserAuthenticationSuccess);
            this.Session.UnRegisterMessageType(MessageTypes.UserAuthenticationBanner);

            return this.IsAuthenticated;
        }

        protected abstract void OnAuthenticate();

        protected virtual void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            this.ErrorMessage = e.Message.Message;
            this.IsAuthenticated = false;
        }

        protected virtual void Session_UserAuthenticationSuccessMessageReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            this.IsAuthenticated = true;
        }

        protected virtual void Session_UserAuthenticationBannerMessageReceived(object sender, MessageEventArgs<BannerMessage> e)
        {
            RaiseAuthenticating(new AuthenticationEventArgs(e.Message.Message, e.Message.Language));
        }

        protected void RaiseAuthenticating(AuthenticationEventArgs args)
        {
            if (this.Authenticating != null)
            {
                this.Authenticating(this, args);
            }
        }

        protected void SendMessage(Message message)
        {
            this.Session.SendMessage(message);
        }

        protected void WaitHandle(WaitHandle eventWaitHandle)
        {
            this.Session.WaitHandle(eventWaitHandle);
        }



        protected virtual void Session_MessageReceived(object sender, MessageEventArgs<Message> e)
        {
        }
        
    }
}

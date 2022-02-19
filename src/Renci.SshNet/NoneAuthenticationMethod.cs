﻿using System;
using System.Threading;
#if !FEATURE_WAITHANDLE_DISPOSE
using Renci.SshNet.Common;
#endif // !FEATURE_WAITHANDLE_DISPOSE
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for "none" authentication method
    /// </summary>
    public class NoneAuthenticationMethod : AuthenticationMethod
    {
	    /// <summary>
        /// Gets connection name
        /// </summary>
        public override string Name
        {
            get { return "none"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoneAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or <c>null</c>.</exception>
        public NoneAuthenticationMethod(string username)
            : base(username)
        {
	        _authenticationCompleted = new AutoResetEvent(false);
        }

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>
        /// Result of authentication  process.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="session" /> is <c>null</c>.</exception>
        public override AuthenticationResult Authenticate(Session session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;

            try
            {
                session.SendMessage(new RequestMessageNone(ServiceName.Connection, Username));
                session.WaitOnHandle(_authenticationCompleted);
            }
            finally
            {
                session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
                session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
            }

            return _authenticationResult;
        }

        private void Session_UserAuthenticationSuccessReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            _authenticationResult = AuthenticationResult.Success;

            _authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            if (e.Message.PartialSuccess)
                _authenticationResult = AuthenticationResult.PartialSuccess;
            else
                _authenticationResult = AuthenticationResult.Failure;

            // Copy allowed authentication methods
            AllowedAuthentications = e.Message.AllowedAuthentications;

            _authenticationCompleted.Set();
        }
    }
}

using System;
using System.Collections.Generic;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    internal class ClientAuthentication : IClientAuthentication
    {
        private readonly int _partialSuccessLimit;

        /// <summary>
        /// Initializes a new <see cref="ClientAuthentication"/> instance.
        /// </summary>
        /// <param name="partialSuccessLimit">The number of times an authentication attempt with any given <see cref="IAuthenticationMethod"/> can result in <see cref="AuthenticationResult.PartialSuccess"/> before it is disregarded.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partialSuccessLimit"/> is less than one.</exception>
        public ClientAuthentication(int partialSuccessLimit)
        {
            if (partialSuccessLimit < 1)
                throw new ArgumentOutOfRangeException("partialSuccessLimit", "Cannot be less than one.");

            _partialSuccessLimit = partialSuccessLimit;
        }

        /// <summary>
        /// Gets the number of times an authentication attempt with any given <see cref="IAuthenticationMethod"/> can
        /// result in <see cref="AuthenticationResult.PartialSuccess"/> before it is disregarded.
        /// </summary>
        /// <value>
        /// The number of times an authentication attempt with any given <see cref="IAuthenticationMethod"/> can result
        /// in <see cref="AuthenticationResult.PartialSuccess"/> before it is disregarded.
        /// </value>
        internal int PartialSuccessLimit
        {
            get { return _partialSuccessLimit; }
        }

        public void Authenticate(IConnectionInfoInternal connectionInfo, ISession session)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException("connectionInfo");
            if (session == null)
                throw new ArgumentNullException("session");

            session.RegisterMessage("SSH_MSG_USERAUTH_FAILURE");
            session.RegisterMessage("SSH_MSG_USERAUTH_SUCCESS");
            session.RegisterMessage("SSH_MSG_USERAUTH_BANNER");
            session.UserAuthenticationBannerReceived += connectionInfo.UserAuthenticationBannerReceived;

            try
            {
                // the exception to report an authentication failure with
                SshAuthenticationException authenticationException = null;

                // try to authenticate against none
                var noneAuthenticationMethod = connectionInfo.CreateNoneAuthenticationMethod();

                var authenticated = noneAuthenticationMethod.Authenticate(session);
                if (authenticated != AuthenticationResult.Success)
                {
                    if (!TryAuthenticate(session, new AuthenticationState(connectionInfo.AuthenticationMethods), noneAuthenticationMethod.AllowedAuthentications, ref authenticationException))
                    {
                        throw authenticationException;
                    }
                }
            }
            finally
            {
                session.UserAuthenticationBannerReceived -= connectionInfo.UserAuthenticationBannerReceived;
                session.UnRegisterMessage("SSH_MSG_USERAUTH_FAILURE");
                session.UnRegisterMessage("SSH_MSG_USERAUTH_SUCCESS");
                session.UnRegisterMessage("SSH_MSG_USERAUTH_BANNER");
            }
        }

        private static bool TryAuthenticate(ISession session,
                                            AuthenticationState authenticationState,
                                            string[] allowedAuthenticationMethods,
                                            ref SshAuthenticationException authenticationException)
        {
            if (allowedAuthenticationMethods.Length == 0)
            {
                authenticationException = new SshAuthenticationException("No authentication methods defined on SSH server.");
                return false;
            }

            // we want to try authentication methods in the order in which they were
            // passed in the ctor, not the order in which the SSH server returns
            // the allowed authentication methods
            var matchingAuthenticationMethods = GetAllowedAuthenticationMethodsThatAreSupported(authenticationState, allowedAuthenticationMethods);
            if (matchingAuthenticationMethods.Count == 0)
            {
                authenticationException = new SshAuthenticationException(string.Format("No suitable authentication method found to complete authentication ({0}).", string.Join(",", allowedAuthenticationMethods)));
                return false;
            }

            foreach (var authenticationMethod in GetOrderedAuthenticationMethods(authenticationState, matchingAuthenticationMethods))
            {
                if (authenticationState.FailedAuthenticationMethods.Contains(authenticationMethod))
                    continue;

                // when the authentication method was previously executed, then skip the authentication
                // method as long as there's another authentication method to try; this is done to avoid
                // a stack overflow for servers that do not update the list of allowed authentication
                // methods after a partial success

                if (!authenticationState.ExecutedAuthenticationMethods.Contains(authenticationMethod))
                {
                    // update state to reflect previosuly executed authentication methods
                    authenticationState.ExecutedAuthenticationMethods.Add(authenticationMethod);
                }

                var authenticationResult = authenticationMethod.Authenticate(session);
                switch (authenticationResult)
                {
                    case AuthenticationResult.PartialSuccess:
                        if (TryAuthenticate(session, authenticationState, authenticationMethod.AllowedAuthentications, ref authenticationException))
                        {
                            authenticationResult = AuthenticationResult.Success;
                        }
                        break;
                    case AuthenticationResult.Failure:
                        authenticationState.FailedAuthenticationMethods.Add(authenticationMethod);
                        authenticationException = new SshAuthenticationException(string.Format("Permission denied ({0}).", authenticationMethod.Name));
                        break;
                    case AuthenticationResult.Success:
                        authenticationException = null;
                        break;
                }

                if (authenticationResult == AuthenticationResult.Success)
                    return true;
            }

            return false;
        }

        private static List<IAuthenticationMethod> GetAllowedAuthenticationMethodsThatAreSupported(AuthenticationState authenticationState,
                                                                                                   string[] allowedAuthenticationMethods)
        {
            var result = new List<IAuthenticationMethod>();

            foreach (var supportedAuthenticationMethod in authenticationState.SupportedAuthenticationMethods)
            {
                var nameOfSupportedAuthenticationMethod = supportedAuthenticationMethod.Name;

                for (var i = 0; i < allowedAuthenticationMethods.Length; i++)
                {
                    if (allowedAuthenticationMethods[i] == nameOfSupportedAuthenticationMethod)
                    {
                        result.Add(supportedAuthenticationMethod);
                        break;
                    }
                }
            }

            return result;
        }

        private static IEnumerable<IAuthenticationMethod> GetOrderedAuthenticationMethods(AuthenticationState authenticationState, List<IAuthenticationMethod> matchingAuthenticationMethods)
        {
            var skippedAuthenticationMethods = new List<IAuthenticationMethod>();

            for (var i = 0; i < matchingAuthenticationMethods.Count; i++)
            {
                var authenticationMethod = matchingAuthenticationMethods[i];

                if (authenticationState.ExecutedAuthenticationMethods.Contains(authenticationMethod))
                {
                    skippedAuthenticationMethods.Add(authenticationMethod);
                    continue;
                }

                yield return authenticationMethod;
            }

            foreach (var authenticationMethod in skippedAuthenticationMethods)
                yield return authenticationMethod;
        }

        private class AuthenticationState
        {
            private readonly IList<IAuthenticationMethod> _supportedAuthenticationMethods;

            public AuthenticationState(IList<IAuthenticationMethod> supportedAuthenticationMethods)
            {
                _supportedAuthenticationMethods = supportedAuthenticationMethods;
                ExecutedAuthenticationMethods = new List<IAuthenticationMethod>();
                FailedAuthenticationMethods = new List<IAuthenticationMethod>();
            }

            /// <summary>
            /// Gets the list of authentication methods that were previously executed.
            /// </summary>
            /// <value>
            /// The list of authentication methods that were previously executed.
            /// </value>
            public IList<IAuthenticationMethod> ExecutedAuthenticationMethods { get; private set; }

            /// <summary>
            /// Gets the list of authentications methods that failed.
            /// </summary>
            /// <value>
            /// The list of authentications methods that failed.
            /// </value>
            public IList<IAuthenticationMethod> FailedAuthenticationMethods { get; private set; }

            /// <summary>
            /// Gets the list of supported authentication methods.
            /// </summary>
            /// <value>
            /// The list of supported authentication methods.
            /// </value>
            public IList<IAuthenticationMethod> SupportedAuthenticationMethods
            {
                get { return _supportedAuthenticationMethods; }
            }
        }
    }
}

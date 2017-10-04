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

        /// <summary>
        /// Attempts to authentication for a given <see cref="ISession"/> using the <see cref="IConnectionInfoInternal.AuthenticationMethods"/>
        /// of the specified <see cref="IConnectionInfoInternal"/>.
        /// </summary>
        /// <param name="connectionInfo">A <see cref="IConnectionInfoInternal"/> to use for authenticating.</param>
        /// <param name="session">The <see cref="ISession"/> for which to perform authentication.</param>
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

        private bool TryAuthenticate(ISession session,
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
            var matchingAuthenticationMethods = authenticationState.GetSupportedAuthenticationMethods(allowedAuthenticationMethods);
            if (matchingAuthenticationMethods.Count == 0)
            {
                authenticationException = new SshAuthenticationException(string.Format("No suitable authentication method found to complete authentication ({0}).",
                                                                                       string.Join(",", allowedAuthenticationMethods)));
                return false;
            }

            foreach (var authenticationMethod in authenticationState.GetActiveAuthenticationMethods(matchingAuthenticationMethods))
            {
                // guard against a stack overlow for servers that do not update the list of allowed authentication
                // methods after a partial success
                if (authenticationState.GetPartialSuccessCount(authenticationMethod) >= _partialSuccessLimit)
                {
                    // TODO Get list of all authentication methods that have reached the partial success limit?

                    authenticationException = new SshAuthenticationException(string.Format("Reached authentication attempt limit for method ({0}).",
                                                                                           authenticationMethod.Name));
                    continue;
                }

                var authenticationResult = authenticationMethod.Authenticate(session);
                switch (authenticationResult)
                {
                    case AuthenticationResult.PartialSuccess:
                        authenticationState.RecordPartialSuccess(authenticationMethod);
                        if (TryAuthenticate(session, authenticationState, authenticationMethod.AllowedAuthentications, ref authenticationException))
                        {
                            authenticationResult = AuthenticationResult.Success;
                        }
                        break;
                    case AuthenticationResult.Failure:
                        authenticationState.RecordFailure(authenticationMethod);
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

        private class AuthenticationState
        {
            private readonly IList<IAuthenticationMethod> _supportedAuthenticationMethods;

            /// <summary>
            /// Records if a given <see cref="IAuthenticationMethod"/> has been tried, and how many times this resulted
            /// in <see cref="AuthenticationResult.PartialSuccess"/>.
            /// </summary>
            /// <remarks>
            /// When there's no entry for a given <see cref="IAuthenticationMethod"/>, then it was never tried.
            /// </remarks>
            private readonly Dictionary<IAuthenticationMethod, int> _authenticationMethodPartialSuccessRegister;

            /// <summary>
            /// Holds the list of authentications methods that failed.
            /// </summary>
            private readonly List<IAuthenticationMethod> _failedAuthenticationMethods;

            public AuthenticationState(IList<IAuthenticationMethod> supportedAuthenticationMethods)
            {
                _supportedAuthenticationMethods = supportedAuthenticationMethods;
                _failedAuthenticationMethods = new List<IAuthenticationMethod>();
                _authenticationMethodPartialSuccessRegister = new Dictionary<IAuthenticationMethod, int>();
            }

            /// <summary>
            /// Records a <see cref="AuthenticationResult.Failure"/> authentication attempt for the specified
            /// <see cref="IAuthenticationMethod"/> .
            /// </summary>
            /// <param name="authenticationMethod">An <see cref="IAuthenticationMethod"/> for which to record the result of an authentication attempt.</param>
            public void RecordFailure(IAuthenticationMethod authenticationMethod)
            {
                _failedAuthenticationMethods.Add(authenticationMethod);
            }

            /// <summary>
            /// Records a <see cref="AuthenticationResult.PartialSuccess"/> authentication attempt for the specified
            /// <see cref="IAuthenticationMethod"/> .
            /// </summary>
            /// <param name="authenticationMethod">An <see cref="IAuthenticationMethod"/> for which to record the result of an authentication attempt.</param>
            public void RecordPartialSuccess(IAuthenticationMethod authenticationMethod)
            {
                int partialSuccessCount;
                if (_authenticationMethodPartialSuccessRegister.TryGetValue(authenticationMethod, out partialSuccessCount))
                {
                    _authenticationMethodPartialSuccessRegister[authenticationMethod] = ++partialSuccessCount;
                }
                else
                {
                    _authenticationMethodPartialSuccessRegister.Add(authenticationMethod, 1);
                }
            }

            /// <summary>
            /// Returns the number of times an authentication attempt with the specified <see cref="IAuthenticationMethod"/>
            /// has resulted in <see cref="AuthenticationResult.PartialSuccess"/>.
            /// </summary>
            /// <param name="authenticationMethod">An <see cref="IAuthenticationMethod"/>.</param>
            /// <returns>
            /// The number of times an authentication attempt with the specified <see cref="IAuthenticationMethod"/>
            /// has resulted in <see cref="AuthenticationResult.PartialSuccess"/>.
            /// </returns>
            public int GetPartialSuccessCount(IAuthenticationMethod authenticationMethod)
            {
                int partialSuccessCount;
                if (_authenticationMethodPartialSuccessRegister.TryGetValue(authenticationMethod, out partialSuccessCount))
                {
                    return partialSuccessCount;
                }
                return 0;
            }

            /// <summary>
            /// Returns a list of supported authentication methods that match one of the specified allowed authentication
            /// methods.
            /// </summary>
            /// <param name="allowedAuthenticationMethods">A list of allowed authentication methods.</param>
            /// <returns>
            /// A list of supported authentication methods that match one of the specified allowed authentication methods.
            /// </returns>
            /// <remarks>
            /// The authentication methods are returned in the order in which they were specified in the list that was
            /// used to initialize the current <see cref="AuthenticationState"/> instance.
            /// </remarks>
            public List<IAuthenticationMethod> GetSupportedAuthenticationMethods(string[] allowedAuthenticationMethods)
            {
                var result = new List<IAuthenticationMethod>();

                foreach (var supportedAuthenticationMethod in _supportedAuthenticationMethods)
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

            /// <summary>
            /// Returns the authentication methods from the specified list that have not yet failed.
            /// </summary>
            /// <param name="matchingAuthenticationMethods">A list of authentication methods.</param>
            /// <returns>
            /// The authentication methods from <paramref name="matchingAuthenticationMethods"/> that have not yet failed.
            /// </returns>
            /// <remarks>
            /// <para>
            /// This method first returns the authentication methods that have not yet been executed, and only then
            /// returns those for which an authentication attempt resulted in a <see cref="AuthenticationResult.PartialSuccess"/>.
            /// </para>
            /// <para>
            /// Any <see cref="IAuthenticationMethod"/> that has failed is skipped.
            /// </para>
            /// </remarks>
            public IEnumerable<IAuthenticationMethod> GetActiveAuthenticationMethods(List<IAuthenticationMethod> matchingAuthenticationMethods)
            {
                var skippedAuthenticationMethods = new List<IAuthenticationMethod>();

                for (var i = 0; i < matchingAuthenticationMethods.Count; i++)
                {
                    var authenticationMethod = matchingAuthenticationMethods[i];

                    // skip authentication methods that have already failed
                    if (_failedAuthenticationMethods.Contains(authenticationMethod))
                        continue;

                    // delay use of authentication methods that had a PartialSuccess result
                    if (_authenticationMethodPartialSuccessRegister.ContainsKey(authenticationMethod))
                    {
                        skippedAuthenticationMethods.Add(authenticationMethod);
                        continue;
                    }

                    yield return authenticationMethod;
                }

                foreach (var authenticationMethod in skippedAuthenticationMethods)
                    yield return authenticationMethod;
            }
        }
    }
}

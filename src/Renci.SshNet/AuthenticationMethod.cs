<<<<<<< HEAD
﻿using Renci.SshNet.Common;
using System;
using System.Threading;
using Renci.SshNet.Security.Org.BouncyCastle.Crypto.Parameters;
=======
﻿using System;

using Renci.SshNet.Common;
>>>>>>> develop

namespace Renci.SshNet
{
    /// <summary>
    /// Base class for all supported authentication methods.
    /// </summary>
    public abstract class AuthenticationMethod : IAuthenticationMethod, IDisposable
    {
        /// <summary>
        /// Tracks result of current authentication process
        /// </summary>
	    protected AuthenticationResult _authenticationResult = AuthenticationResult.Failure;

        /// <summary>
        /// Tracks completion of current authentication process
        /// </summary>
	    protected EventWaitHandle _authenticationCompleted = null;

        /// <summary>
        /// Gets the name of the authentication method.
        /// </summary>
        /// <value>
        /// The name of the authentication method.
        /// </value>
#pragma warning disable CA2119 // Seal methods that satisfy private interfaces
        public abstract string Name { get; }
#pragma warning restore CA2119 // Seal methods that satisfy private interfaces

        /// <summary>
        /// Gets connection username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets or sets the list of allowed authentications.
        /// </summary>
        public string[] AllowedAuthentications { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or <see langword="null"/>.</exception>
        protected AuthenticationMethod(string username)
        {
            ThrowHelper.ThrowIfNullOrWhiteSpace(username);

            Username = username;
        }

        #region IDisposable Members

        private bool _isDisposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
	        Dispose(true);
	        GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
	        if (_isDisposed)
		        return;

	        if (disposing)
	        {
		        var authenticationCompleted = _authenticationCompleted;
		        if (authenticationCompleted != null)
		        {
			        authenticationCompleted.Dispose();
			        _authenticationCompleted = null;
		        }

		        // Only if called with Dispose(true) otherwise we treat it is as not Disposed properly
		        _isDisposed = true;
	        }
        }

        #endregion

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to authenticate.</param>
        /// <returns>
        /// The result of the authentication process.
        /// </returns>
        public abstract AuthenticationResult Authenticate(Session session);

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to authenticate.</param>
        /// <returns>
        /// The result of the authentication process.
        /// </returns>
        AuthenticationResult IAuthenticationMethod.Authenticate(ISession session)
        {
            return Authenticate((Session)session);
        }
    }
}

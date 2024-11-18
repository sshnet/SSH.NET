namespace Renci.SshNet
{
    /// <summary>
    /// Base interface for authentication of a session using a given method.
    /// </summary>
    internal interface IAuthenticationMethod
    {
        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to authenticate.</param>
        /// <returns>
        /// The result of the authentication process.
        /// </returns>
        AuthenticationResult Authenticate(ISession session);

        /// <summary>
        /// Gets the list of allowed authentications.
        /// </summary>
        /// <value>
        /// The list of allowed authentications.
        /// </value>
        string[] AllowedAuthentications { get; }

        /// <summary>
        /// Gets the name of the authentication method.
        /// </summary>
        /// <value>
        /// The name of the authentication method.
        /// </value>
        string Name { get; }
    }
}

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides prompt information when <see cref="Renci.SshNet.KeyboardInteractiveConnectionInfo.AuthenticationPrompt"/> is raised
    /// </summary>
    public class AuthenticationPrompt
    {
        /// <summary>
        /// Gets the prompt sequence id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user input should be echoed as characters are typed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user input should be echoed as characters are typed; otherwise, <c>false</c>.
        /// </value>
        public bool IsEchoed { get; private set; }

        /// <summary>
        /// Gets server information request.
        /// </summary>
        public string Request { get; private set; }

        /// <summary>
        /// Gets or sets server information response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public string Response { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationPrompt"/> class.
        /// </summary>
        /// <param name="id">The sequence id.</param>
        /// <param name="isEchoed">if set to <c>true</c> the user input should be echoed.</param>
        /// <param name="request">The request.</param>
        public AuthenticationPrompt(int id, bool isEchoed, string request)
        {
            Id = id;
            IsEchoed = isEchoed;
            Request = request;
        }
    }
}

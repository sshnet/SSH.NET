﻿namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides prompt information when <see cref="KeyboardInteractiveConnectionInfo.AuthenticationPrompt"/> is raised.
    /// </summary>
    public class AuthenticationPrompt
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationPrompt"/> class.
        /// </summary>
        /// <param name="id">The sequence id.</param>
        /// <param name="isEchoed">if set to <see langword="true"/> the user input should be echoed.</param>
        /// <param name="request">The request.</param>
        public AuthenticationPrompt(int id, bool isEchoed, string request)
        {
            Id = id;
            IsEchoed = isEchoed;
            Request = request;
        }

        /// <summary>
        /// Gets the prompt sequence id.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets a value indicating whether the user input should be echoed as characters are typed.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the user input should be echoed as characters are typed; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsEchoed { get; }

        /// <summary>
        /// Gets server information request.
        /// </summary>
        public string Request { get; }

        /// <summary>
        /// Gets or sets server information response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public string Response { get; set; }
    }
}

using System;

using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for <see cref="Channels.Channel.RequestReceived"/> event.
    /// </summary>
    internal sealed class ChannelRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelRequestEventArgs"/> class.
        /// </summary>
        /// <param name="info">Request information.</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
        public ChannelRequestEventArgs(RequestInfo info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            Info = info;
        }

        /// <summary>
        /// Gets the request information.
        /// </summary>
        /// <value>
        /// The request information.
        /// </value>
        public RequestInfo Info { get; }
    }
}

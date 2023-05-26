using System;

using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for <see cref="Channels.Channel.RequestReceived"/> event.
    /// </summary>
    internal class ChannelRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelRequestEventArgs"/> class.
        /// </summary>
        /// <param name="info">Request information.</param>
        public ChannelRequestEventArgs(RequestInfo info)
        {
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

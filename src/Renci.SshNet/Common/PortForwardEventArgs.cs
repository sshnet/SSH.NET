using System;
using System.Net;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for <see cref="ForwardedPort.RequestReceived"/> event.
    /// </summary>
    public sealed class PortForwardEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortForwardEventArgs"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is not within <see cref="IPEndPoint.MinPort" /> and <see cref="IPEndPoint.MaxPort" />.</exception>
        internal PortForwardEventArgs(string host, uint port)
        {
            ThrowHelper.ThrowIfNull(host);
            port.ValidatePort();

            OriginatorHost = host;
            OriginatorPort = port;
        }

        /// <summary>
        /// Gets request originator host.
        /// </summary>
        public string OriginatorHost { get; }

        /// <summary>
        /// Gets request originator port.
        /// </summary>
        public uint OriginatorPort { get; }
    }
}

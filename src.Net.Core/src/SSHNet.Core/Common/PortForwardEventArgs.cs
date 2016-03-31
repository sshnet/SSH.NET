using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for <see cref="Renci.SshNet.ForwardedPort.RequestReceived"/> event.
    /// </summary>
    public class PortForwardEventArgs : EventArgs
    {
        /// <summary>
        /// Gets request originator host.
        /// </summary>
        public string OriginatorHost { get; private set; }

        /// <summary>
        /// Gets request originator port.
        /// </summary>
        public uint OriginatorPort { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortForwardEventArgs"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is not within <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        internal PortForwardEventArgs(string host, uint port)
        {
            if (host == null)
                throw new ArgumentNullException("host");
            port.ValidatePort("port");

            OriginatorHost = host;
            OriginatorPort = port;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
{
    /// <summary>
    /// Provides data for <see cref="Renci.SshClient.ForwardedPort.RequestReceived"/> event.
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
        public PortForwardEventArgs(string host, uint port)
        {
            this.OriginatorHost = host;
            this.OriginatorPort = port;
        }
    }
}

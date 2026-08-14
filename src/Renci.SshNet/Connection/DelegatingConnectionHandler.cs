using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// A base type for a <see cref="ConnectionHandler"/> that wraps and delegates to another
    /// <see cref="ConnectionHandler"/>.
    /// </summary>
    /// <remarks>
    /// Override only the members you want to customize; the rest forward to
    /// <see cref="InnerHandler"/> by default. This mirrors <c>DelegatingHandler</c> for <c>HttpClient</c>.
    /// </remarks>
    public abstract class DelegatingConnectionHandler : ConnectionHandler
    {
        /// <summary>
        /// Gets the inner handler which this instance delegates to by default.
        /// </summary>
        protected ConnectionHandler InnerHandler { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingConnectionHandler"/> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler which this instance delegates to by default.</param>
        /// <exception cref="ArgumentNullException"><paramref name="innerHandler"/> is <see langword="null"/>.</exception>
        protected DelegatingConnectionHandler(ConnectionHandler innerHandler)
        {
            ArgumentNullException.ThrowIfNull(innerHandler);

            InnerHandler = innerHandler;
        }

        /// <inheritdoc/>
        public override Socket Connect(ConnectionInfo connectionInfo)
        {
            return InnerHandler.Connect(connectionInfo);
        }

        /// <inheritdoc/>
        public override Task<Socket> ConnectAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            return InnerHandler.ConnectAsync(connectionInfo, cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                InnerHandler.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

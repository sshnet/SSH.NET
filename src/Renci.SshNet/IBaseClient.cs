namespace Renci.SshNet
{
    /// <summary>
    /// Serves as base class for client implementations, provides common client functionality.
    /// </summary>
    public interface IBaseClient
    {
        /// <summary>
        /// Connects client to the server.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The client is already connected.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <exception cref="T:System.Net.Sockets.SocketException">Socket connection to the SSH server or proxy server could not be established, or an error occurred while resolving the hostname.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshConnectionException">SSH session could not be established.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshAuthenticationException">Authentication of SSH session failed.</exception>
        /// <exception cref="T:Renci.SshNet.Common.ProxyException">Failed to establish proxy connection.</exception>
        void Connect();

        /// <summary>
        /// Disconnects client from the server.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">The method was called after the client was disposed.</exception>
        void Disconnect();
    }
}

using System.Text;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Represents a factory for creating SFTP response messages.
    /// </summary>
    internal interface ISftpResponseFactory
    {
        /// <summary>
        /// Creates a SFTP response message for the specified protocol version and message type, and
        /// with the specified <see cref="Encoding"/>.
        /// </summary>
        /// <param name="protocolVersion">The protocol version.</param>
        /// <param name="messageType">The message type.</param>
        /// <param name="encoding">The <see cref="Encoding"/>.</param>
        /// <returns>
        /// A <see cref="SftpMessage"/>.
        /// </returns>
        SftpMessage Create(uint protocolVersion, byte messageType, Encoding encoding);
    }
}

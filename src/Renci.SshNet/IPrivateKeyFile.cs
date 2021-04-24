using Renci.SshNet.Security;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents private key file interface.
    /// </summary>
    public interface IPrivateKeyFile
    {
        /// <summary>
        /// Gets the host key.
        /// </summary>
        HostAlgorithm HostKey { get; }
    }
}
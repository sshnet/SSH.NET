using Renci.SshNet.Security;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents private key source interface.
    /// </summary>
    public interface IPrivateKeySource
    {
        /// <summary>
        /// Gets the host key.
        /// </summary>
        HostAlgorithm HostKey { get; }
    }
}
using System.ComponentModel;

using Renci.SshNet.Security;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents private key source interface.
    /// </summary>
    /// <remarks>
    /// This interface has been replaced by <see cref="IHostAlgorithmsProvider"/>
    /// and is currently not used in the library.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IPrivateKeySource : IHostAlgorithmsProvider
    {
        /// <summary>
        /// Gets the host key.
        /// </summary>
        HostAlgorithm HostKey { get; }
    }
}

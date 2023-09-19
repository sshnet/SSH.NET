using System;
using System.ComponentModel;

using Renci.SshNet.Security;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents private key source interface.
    /// </summary>
    /// <remarks>
    /// This interface has been replaced by <see cref="IHostAlgorithmsProvider"/>
    /// and is obsolete.
    /// </remarks>
    [Obsolete($"Use {nameof(IHostAlgorithmsProvider)} instead. " +
        $"{nameof(IPrivateKeySource)} may be removed in a future release. " +
        $"See https://github.com/sshnet/SSH.NET/issues/1174 for details.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IPrivateKeySource : IHostAlgorithmsProvider
    {
        /// <summary>
        /// Gets the host key.
        /// </summary>
        HostAlgorithm HostKey { get; }
    }
}

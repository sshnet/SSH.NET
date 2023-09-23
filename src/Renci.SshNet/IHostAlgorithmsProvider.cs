using System.Collections.Generic;

using Renci.SshNet.Security;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents a collection of host algorithms.
    /// </summary>
    public interface IHostAlgorithmsProvider
    {
        /// <summary>
        /// The host algorithms provided by this <see cref="IHostAlgorithmsProvider"/>.
        /// </summary>
        /// <remarks>
        /// In situations where there is a preferred order of usage of the host algorithms,
        /// the collection should be ordered from most preferred to least.
        /// </remarks>
        IReadOnlyCollection<HostAlgorithm> HostAlgorithms { get; }
    }
}

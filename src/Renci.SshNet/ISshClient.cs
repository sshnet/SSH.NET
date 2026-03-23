using System;
using System.Text;

namespace Renci.SshNet
{
    /// <inheritdoc cref="V2.ISshClient" />
    [Obsolete($"Use {nameof(V2.ISshClient)} instead.")]
    public interface ISshClient : V2.ISshClient
    {
        /// <inheritdoc cref="V2.ISshClient.CreateCommand(string)" />
        public new SshCommand CreateCommand(string commandText);

        /// <inheritdoc cref="V2.ISshClient.CreateCommand(string, Encoding)" />
        public new SshCommand CreateCommand(string commandText, Encoding encoding);

        /// <inheritdoc cref="V2.ISshClient.RunCommand(string)" />
        public new SshCommand RunCommand(string commandText);
    }
}

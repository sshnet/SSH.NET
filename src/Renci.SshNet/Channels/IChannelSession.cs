using System.Collections.Generic;
#if NET6_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;
#endif

using Renci.SshNet.Common;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Session SSH channel.
    /// </summary>
    internal interface IChannelSession : IChannel
    {
        /// <summary>
        /// Opens the channel.
        /// </summary>
        void Open();

#if NET6_0_OR_GREATER
        /// <summary>
        /// Opens the channel.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous connect operation.</returns>
        Task OpenAsync(CancellationToken token);
#endif

        /// <summary>
        /// Sends the pseudo terminal request.
        /// </summary>
        /// <param name="environmentVariable">The environment variable.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="terminalModeValues">The terminal mode values.</param>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendPseudoTerminalRequest(string environmentVariable,
                                       uint columns,
                                       uint rows,
                                       uint width,
                                       uint height,
                                       IDictionary<TerminalModes, uint> terminalModeValues);

        /// <summary>
        /// Sends the X11 forwarding request.
        /// </summary>
        /// <param name="isSingleConnection">if set to <see langword="true"/> the it is single connection.</param>
        /// <param name="protocol">The protocol.</param>
        /// <param name="cookie">The cookie.</param>
        /// <param name="screenNumber">The screen number.</param>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendX11ForwardingRequest(bool isSingleConnection, string protocol, byte[] cookie, uint screenNumber);

        /// <summary>
        /// Sends the environment variable request.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="variableValue">The variable value.</param>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendEnvironmentVariableRequest(string variableName, string variableValue);

        /// <summary>
        /// Sends the shell request.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendShellRequest();

        /// <summary>
        /// Sends the exec request.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendExecRequest(string command);

#if NET6_0_OR_GREATER
        /// <summary>
        /// Sends the exec request.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous connect operation with
        /// value <see langword="true"/> if request was successful; otherwise <see langword="false"/>.</returns>
        Task<bool> SendExecRequestAsync(string command, CancellationToken token);
#endif

        /// <summary>
        /// Sends the exec request.
        /// </summary>
        /// <param name="breakLength">Length of the break.</param>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendBreakRequest(uint breakLength);

        /// <summary>
        /// Sends the subsystem request.
        /// </summary>
        /// <param name="subsystem">The subsystem.</param>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendSubsystemRequest(string subsystem);

        /// <summary>
        /// Sends the window change request.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendWindowChangeRequest(uint columns, uint rows, uint width, uint height);

        /// <summary>
        /// Sends the local flow request.
        /// </summary>
        /// <param name="clientCanDo">if set to <see langword="true"/> [client can do].</param>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendLocalFlowRequest(bool clientCanDo);

        /// <summary>
        /// Sends the signal request.
        /// </summary>
        /// <param name="signalName">Name of the signal.</param>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendSignalRequest(string signalName);

        /// <summary>
        /// Sends the exit status request.
        /// </summary>
        /// <param name="exitStatus">The exit status.</param>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendExitStatusRequest(uint exitStatus);

        /// <summary>
        /// Sends the exit signal request.
        /// </summary>
        /// <param name="signalName">Name of the signal.</param>
        /// <param name="coreDumped">if set to <see langword="true"/> [core dumped].</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="language">The language.</param>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendExitSignalRequest(string signalName, bool coreDumped, string errorMessage, string language);

        /// <summary>
        /// Sends eow@openssh.com request.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendEndOfWriteRequest();

        /// <summary>
        /// Sends keepalive@openssh.com request.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if request was successful; otherwise <see langword="false"/>.
        /// </returns>
        bool SendKeepAliveRequest();
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Connection;

namespace Renci.SshNet.Tests.Common
{
    /// <summary>
    /// Builds the byte stream an SCP server sends to the client and replays it into a real
    /// <see cref="ScpClient"/> through mocked session and channel objects.
    /// </summary>
    public sealed class ScpServerTranscript
    {
        private readonly List<byte> _bytes = new List<byte>();

        /// <summary>
        /// Appends a control line, terminated by a line feed.
        /// </summary>
        public ScpServerTranscript Line(string line)
        {
            return Raw(line + "\n");
        }

        /// <summary>
        /// Appends <paramref name="data"/> encoded as UTF-8, which is the default
        /// <see cref="ConnectionInfo.Encoding"/>.
        /// </summary>
        public ScpServerTranscript Raw(string data)
        {
            return Raw(Encoding.UTF8.GetBytes(data));
        }

        public ScpServerTranscript Raw(byte[] data)
        {
            _bytes.AddRange(data);
            return this;
        }

        /// <summary>
        /// Appends a C (file) record, the file content, and the status byte that follows it.
        /// </summary>
        public ScpServerTranscript File(string name, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var length = bytes.Length.ToString(CultureInfo.InvariantCulture);

            return Line($"C0644 {length} {name}").Raw(bytes).Raw([0]);
        }

        /// <summary>
        /// Replays the transcript into a connected <see cref="ScpClient"/> and runs
        /// <paramref name="operation"/> on it.
        /// </summary>
        /// <param name="operation">The client operation to run.</param>
        /// <param name="closeChannel">
        /// <see langword="true"/> to signal end of stream after the transcript, as when the
        /// server closes the channel; otherwise a read past the end blocks.
        /// </param>
        /// <param name="timeout">How long to wait for <paramref name="operation"/> to finish.</param>
        public ScpTranscriptResult Run(Action<ScpClient> operation, bool closeChannel = false, TimeSpan? timeout = null)
        {
            var connectionInfo = new ConnectionInfo("host", 22, "user", new PasswordAuthenticationMethod("user", "pwd"));
            var serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            var socketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
            var sessionMock = new Mock<ISession>(MockBehavior.Strict);
            var channelMock = new Mock<IChannelSession>(MockBehavior.Strict);
            var remotePathTransformationMock = new Mock<IRemotePathTransformation>(MockBehavior.Strict);
            var sentData = new List<byte[]>();
            var execRequests = new List<string>();

            var pipeStream = new PipeStream();
            pipeStream.Write(_bytes.ToArray(), 0, _bytes.Count);
            if (closeChannel)
            {
                pipeStream.Dispose();
            }

            _ = sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
            _ = serviceFactoryMock.Setup(p => p.CreateSocketFactory()).Returns(socketFactoryMock.Object);
            _ = serviceFactoryMock.Setup(p => p.CreateSession(connectionInfo, socketFactoryMock.Object)).Returns(sessionMock.Object);
            _ = sessionMock.Setup(p => p.Connect());
            _ = serviceFactoryMock.Setup(p => p.CreatePipeStream()).Returns(pipeStream);
            _ = sessionMock.Setup(p => p.CreateChannelSession()).Returns(channelMock.Object);
            _ = channelMock.Setup(p => p.Open());
            _ = remotePathTransformationMock.Setup(p => p.Transform(It.IsAny<string>())).Returns<string>(path => path);
            _ = channelMock.Setup(p => p.SendExecRequest(It.IsAny<string>()))
                           .Callback<string>(execRequests.Add)
                           .Returns(true);
            _ = channelMock.Setup(p => p.SendData(It.IsAny<byte[]>())).Callback<byte[]>(sentData.Add);
            _ = channelMock.Setup(p => p.SendData(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                           .Callback<byte[], int, int>((data, offset, count) => sentData.Add(data.Skip(offset).Take(count).ToArray()));
            _ = channelMock.Setup(p => p.Dispose());

            var scpClient = new ScpClient(connectionInfo, false, serviceFactoryMock.Object, remotePathTransformationMock.Object);
            scpClient.Connect();

            Exception exception = null;
            var task = Task.Run(() =>
            {
                try
                {
                    operation(scpClient);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            var completed = task.Wait(timeout ?? TimeSpan.FromSeconds(10));

            pipeStream.Dispose();

            _ = task.Wait(TimeSpan.FromSeconds(10));

            return new ScpTranscriptResult(completed, exception, sentData, execRequests);
        }
    }
}

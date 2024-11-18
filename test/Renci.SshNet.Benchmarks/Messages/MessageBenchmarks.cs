using BenchmarkDotNet.Attributes;

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Benchmarks.Messages
{
    [MemoryDiagnoser]
    public class MessageBenchmarks
    {
        [Benchmark]
        public Message WriteBytes()
        {
            using var sshDataStream = new SshDataStream(SshData.DefaultCapacity);
            var bannerMessage = new WritableDisconnectMessage(DisconnectReason.ServiceNotAvailable, "Goodbye");
            bannerMessage.WritePrivateBytes(sshDataStream);

            return bannerMessage; // Avoid JIT elimination
        }

        private sealed class WritableDisconnectMessage : DisconnectMessage
        {
            public WritableDisconnectMessage(DisconnectReason reasonCode, string message)
                : base(reasonCode, message)
            {
            }

            public void WritePrivateBytes(SshDataStream sshDataStream)
            {
                WriteBytes(sshDataStream);
            }
        }
    }
}

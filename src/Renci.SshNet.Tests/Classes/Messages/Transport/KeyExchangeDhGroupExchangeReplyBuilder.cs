using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes.Messages.Transport
{
    public class KeyExchangeDhGroupExchangeReplyBuilder
    {
        private byte[] _hostKeyAlgorithm;
        private byte[] _hostKeys;
        private BigInteger _f;
        private byte[] _signature;

        public KeyExchangeDhGroupExchangeReplyBuilder WithHostKey(string hostKeyAlgorithm, params BigInteger[] hostKeys)
        {
            _hostKeyAlgorithm = Encoding.UTF8.GetBytes(hostKeyAlgorithm);

            var sshDataStream = new SshDataStream(0);
            foreach (var hostKey in hostKeys)
                sshDataStream.Write(hostKey);
            _hostKeys = sshDataStream.ToArray();

            return this;
        }

        public KeyExchangeDhGroupExchangeReplyBuilder WithF(BigInteger f)
        {
            _f = f;
            return this;
        }

        public KeyExchangeDhGroupExchangeReplyBuilder WithSignature(byte[] signature)
        {
            _signature = signature;
            return this;
        }

        public byte[] Build()
        {
            var sshDataStream = new SshDataStream(0);
            sshDataStream.WriteByte(KeyExchangeDhGroupExchangeReply.MessageNumber);
            sshDataStream.Write((uint)(4 + _hostKeyAlgorithm.Length + _hostKeys.Length));
            sshDataStream.Write((uint) _hostKeyAlgorithm.Length);
            sshDataStream.Write(_hostKeyAlgorithm, 0, _hostKeyAlgorithm.Length);
            sshDataStream.Write(_hostKeys, 0, _hostKeys.Length);
            sshDataStream.Write(_f);
            sshDataStream.WriteBinary(_signature);
            return sshDataStream.ToArray();
        }
    }
}

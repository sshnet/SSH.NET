using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes.Messages.Transport
{
    public class KeyExchangeDhGroupExchangeGroupBuilder
    {
        private BigInteger _safePrime;
        private BigInteger _subGroup;

        public KeyExchangeDhGroupExchangeGroupBuilder WithSafePrime(BigInteger safePrime)
        {
            _safePrime = safePrime;
            return this;
        }

        public KeyExchangeDhGroupExchangeGroupBuilder WithSubGroup(BigInteger subGroup)
        {
            _subGroup = subGroup;
            return this;
        }

        public byte[] Build()
        {
            var sshDataStream = new SshDataStream(0);
            sshDataStream.WriteByte(KeyExchangeDhGroupExchangeGroup.MessageNumber);
            sshDataStream.Write(_safePrime);
            sshDataStream.Write(_subGroup);
            return sshDataStream.ToArray();
        }
    }
}

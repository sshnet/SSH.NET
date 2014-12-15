using System.Collections.Generic;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    public class SftpVersionResponseBuilder
    {
        private readonly uint _version;
        private readonly IDictionary<string, string> _extensions;

        private SftpVersionResponseBuilder(uint version)
        {
            _version = version;
            _extensions = new Dictionary<string, string>();
        }

        public static SftpVersionResponseBuilder Create(uint version)
        {
            return new SftpVersionResponseBuilder(version);
        }

        public SftpVersionResponseBuilder AddExtension(string name, string data)
        {
            _extensions.Add(name, data);
            return this;
        }

        public byte[] Build()
        {
            var extensions = BuildExtensions();

            var sshDataStream = new SshDataStream(4 + 1 + 4 + extensions.Length);
            sshDataStream.Write((uint)sshDataStream.Capacity - 4);
            sshDataStream.WriteByte((byte)SftpMessageTypes.Version);
            sshDataStream.Write(_version);
            sshDataStream.Write(extensions, 0, extensions.Length);
            return sshDataStream.ToArray();
        }

        private byte[] BuildExtensions()
        {
            var sshDataStream = new SshDataStream(0);
            foreach (var extensionPair in _extensions)
            {
                sshDataStream.Write(extensionPair.Key, Encoding.ASCII);
                sshDataStream.Write(extensionPair.Value, Encoding.ASCII);
            }
            return sshDataStream.ToArray();
        }
    }
}

using Renci.SshNet.Common;
using Renci.SshNet.Security;
using System.Text;

namespace Renci.SshNet
{
    class OpenSSHKeyReader : SshData
    {
        // See doc: https://github.com/openssh/openssh-portable/blob/master/PROTOCOL.key
        // Reference: https://github.com/net-ssh/net-ssh/blob/dd13dd44d68b7fa82d4ca9a3bbe18e30c855f1d2/lib/net/ssh/authentication/ed25519.rb

        const string AUTH_MAGIC = "openssh-key-v1";

        public OpenSSHKeyReader(byte[] data)
        {
            Load(data);
        }

        public ED25519Key ReadKey()
        {
            var magic = Encoding.ASCII.GetString(ReadBytes(AUTH_MAGIC.Length));
            if (magic != AUTH_MAGIC)
                throw new SshException("Unsupported OPENSSH Format: " + magic);

            ReadByte(); // 0 terminated String

            var cipher = ReadString(Encoding.ASCII);
            var kdfname = ReadString(Encoding.ASCII);
            var kdfoptions = ReadString(Encoding.ASCII);

            var num_keys = ReadUInt32();

            if (num_keys > 1)
                throw new SshException("Currently just one key is supported");

            // we don't care for the Public-Keys
            for (int i = 0; i < num_keys; i++)
            {
                ReadString(Encoding.ASCII);
            }

            var len = ReadUInt32();

            // XXX decrypt Data

            var checkint1 = ReadUInt32();
            var checkint2 = ReadUInt32();

            if (checkint1 != checkint2)
                throw new SshException(string.Format("Check Integer mismatch: {0} <-> {1}", checkint1, checkint2));

            var keytype = ReadString(Encoding.ASCII);
            if (keytype != "ssh-ed25519")
                throw new SshException("Unsupported Key-Type: " + keytype);
            len = ReadUInt32();
            var pk = ReadBytes((int)len).Reverse();
            len = ReadUInt32();
            var sk = ReadBytes((int)len);
            var comment = ReadString(Encoding.ASCII);

            return new ED25519Key(pk, sk);
        }

        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
        }
    }
}

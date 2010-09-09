using System;
using System.Collections.Generic;

namespace Renci.SshClient.Security
{
    internal class CompressionZlibOpenSsh : Compression
    {
        private bool _active;

        public override string Name
        {
            get { return "zlib@openssh.org"; }
        }

        public CompressionZlibOpenSsh(Session session)
            : base(session)
        {
            session.MessageReceived += Session_MessageReceived;
        }

        private void Session_MessageReceived(object sender, Common.MessageReceivedEventArgs e)
        {
            if (e.Message is Messages.Authentication.SuccessMessage)
            {
                this._active = true;
                this.Session.MessageReceived -= Session_MessageReceived;
            }
        }

        public override IEnumerable<byte> Compress(IEnumerable<byte> data)
        {
            if (!this._active)
            {
                return data;
            }

            throw new NotImplementedException();

            //using (var output = new MemoryStream())
            //{
            //    using (var input = new MemoryStream(data.ToArray()))
            //    using (var compress = new DeflateStream(output, CompressionMode.Compress))
            //    {
            //        compress.FlushMode = FlushType.Partial;

            //        input.CopyTo(compress);

            //        var result = new List<byte>();

            //        result.Add(0x78);
            //        result.Add(0x9c);

            //        result.AddRange(output.ToArray());

            //        return result;
            //    }
            //}
        }

        public override IEnumerable<byte> Uncompress(IEnumerable<byte> data)
        {
            if (!this._active)
            {
                return data;
            }

            throw new NotImplementedException();

            //Create the decompressed file.
            //using (var output = new MemoryStream())
            //{
            //    using (var input = new MemoryStream(data.ToArray()))
            //    {
            //        input.ReadByte();
            //        input.ReadByte();

            //        using (var decompress = new DeflateStream(input, CompressionMode.Decompress))
            //        {
            //            // Copy the decompression stream 
            //            // into the output file.
            //            decompress.CopyTo(output);
            //        }
            //    }

            //    return output.ToArray();
            //}
        }

    }
}
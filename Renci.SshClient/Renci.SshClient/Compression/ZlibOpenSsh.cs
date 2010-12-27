using System;
using System.Collections.Generic;

namespace Renci.SshClient.Compression
{
    /// <summary>
    /// Represents "zlib@openssh.org" compression implementation
    /// </summary>
    internal class ZlibOpenSsh : Compressor
    {
        private bool _active;

        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "zlib@openssh.org"; }
        }

        /// <summary>
        /// Initializes the algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        public override void Init(Session session)
        {
            base.Init(session);

            session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;
        }

        private void Session_UserAuthenticationSuccessReceived(object sender, MessageEventArgs<Messages.Authentication.SuccessMessage> e)
        {
            this._active = true;
            this.Session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
        }

        /// <summary>
        /// Compresses the specified data.
        /// </summary>
        /// <param name="data">Data to compress.</param>
        /// <returns>
        /// Compressed data
        /// </returns>
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

        /// <summary>
        /// Decompresses the specified data.
        /// </summary>
        /// <param name="data">Compressed data.</param>
        /// <returns>
        /// Decompressed data.
        /// </returns>
        public override IEnumerable<byte> Decompress(IEnumerable<byte> data)
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
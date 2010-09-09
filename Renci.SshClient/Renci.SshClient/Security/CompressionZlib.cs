using System;
using System.Collections.Generic;

namespace Renci.SshClient.Security
{
    internal class CompressionZlib : Compression
    {
        private bool _active;

        public override string Name
        {
            get { return "zlib"; }
        }

        public CompressionZlib(Session session)
            : base(session)
        {
            session.MessageReceived += Session_MessageReceived;
        }

        private void Session_MessageReceived(object sender, Common.MessageReceivedEventArgs e)
        {
            if (e.Message is Messages.Transport.NewKeysMessage)
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
        }

        public override IEnumerable<byte> Uncompress(IEnumerable<byte> data)
        {
            if (!this._active)
            {
                return data;
            }

            throw new NotImplementedException();
        }
    }
}
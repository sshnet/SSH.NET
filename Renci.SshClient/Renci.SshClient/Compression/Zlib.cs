using System;
using System.Collections.Generic;

namespace Renci.SshClient.Compression
{
    internal class Zlib : Compressor
    {
        private bool _active;

        public override string Name
        {
            get { return "zlib"; }
        }

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
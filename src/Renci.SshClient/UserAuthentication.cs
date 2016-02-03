using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient
{
    public class UserAuthentication2
    {
        private Session _session;

        public string Username { get; set; }

        internal UserAuthentication2(Session session)
        {
            this._session = session;
            this.Username = session.ConnectionInfo.Username;
        }
    }
}

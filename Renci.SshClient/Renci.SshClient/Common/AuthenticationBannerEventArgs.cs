using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
{
    public class AuthenticationBannerEventArgs : AuthenticationEventArgs
    {
        public string BannerMessage { get; private set; }

        public string Language { get; private set; }

        public AuthenticationBannerEventArgs(string username, string message, string language)
            : base(username)
        {
            this.BannerMessage = message;
            this.Language = language;
        }
    }
}

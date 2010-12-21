using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
{
    public class AuthenticationPrompt
    {
        public int Id { get; private set; }
        /// <summary>
        /// Gets or sets a value indicating whether the user input should be echoed as characters are typed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user input should be echoed as characters are typed; otherwise, <c>false</c>.
        /// </value>
        public bool IsEchoed { get; private set; }

        public string Request { get; private set; }

        public string Response { get; set; }

        public AuthenticationPrompt(int id, bool isEchoed, string request)
        {
            this.Id = id;
            this.IsEchoed = isEchoed;
            this.Request = request;
        }
    }
}

using System.Text;

namespace Renci.SshClient.Messages.Authentication
{
    internal class RequestMessageHost : RequestMessage
    {
        public override string MethodName
        {
            get
            {
                return "hostbased";
            }
        }

        /// <summary>
        /// Gets the public key algorithm for host key
        /// </summary>
        public string PublicKeyAlgorithm { get; private set; }

        /// <summary>
        /// Gets or sets the public host key and certificates for client host.
        /// </summary>
        /// <value>
        /// The public host key.
        /// </value>
        public string PublicHostKey { get; private set; }

        /// <summary>
        /// Gets or sets the name of the client host.
        /// </summary>
        /// <value>
        /// The name of the client host.
        /// </value>
        public string ClientHostName { get; private set; }

        /// <summary>
        /// Gets or sets the client username on the client host
        /// </summary>
        /// <value>
        /// The client username.
        /// </value>
        public string ClientUsername { get; private set; }

        public string Signature { get; set; }

        public RequestMessageHost(ServiceNames serviceName, string username, string publicKeyAlgorithm, string publicHostKey, string clientHostName, string clientUsername)
            : base(serviceName, username)
        {
            this.PublicKeyAlgorithm = publicKeyAlgorithm;
            this.PublicHostKey = publicHostKey;
            this.ClientHostName = clientHostName;
            this.ClientUsername = clientUsername;
        }

        protected override void SaveData()
        {
            base.SaveData();


            this.Write(this.PublicKeyAlgorithm);
            this.Write(this.PublicHostKey);
            this.Write(this.ClientHostName, Encoding.ASCII);
            this.Write(this.ClientUsername, Encoding.UTF8);

            if (!string.IsNullOrEmpty(this.Signature))
                this.Write(this.Signature);
        }
    }
}

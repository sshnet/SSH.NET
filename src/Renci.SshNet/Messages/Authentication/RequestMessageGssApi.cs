namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents "gssapi-with-mic" SSH_MSG_USERAUTH_REQUEST message.
    /// </summary>
    internal sealed class RequestMessageGssApi : RequestMessage
    {
        private readonly byte[][] _supportedMechanismOids;

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                if (_supportedMechanismOids?.Length > 0)
                {
                    capacity += 4; // mechanism count length
                    foreach (var oid in _supportedMechanismOids)
                    {
                        capacity += oid.Length; // mechanism
                    }
                }

                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessageGssApi"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="supportedMechanismOids">The supported mechanism oids.</param>
        public RequestMessageGssApi(ServiceName serviceName, string username, params byte[][] supportedMechanismOids)
            : base(serviceName, username, "gssapi-with-mic")
        {
            _supportedMechanismOids = supportedMechanismOids;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            if (_supportedMechanismOids?.Length > 0)
            {
                Write((uint)_supportedMechanismOids.Length);
                foreach (var oid in _supportedMechanismOids)
                {
                    WriteBinaryString(oid);
                }
            }
        }
    }
}

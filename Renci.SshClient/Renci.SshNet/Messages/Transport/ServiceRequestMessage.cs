using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_SERVICE_REQUEST message.
    /// </summary>
    [Message("SSH_MSG_SERVICE_REQUEST", 5)]
    public class ServiceRequestMessage : Message
    {
#if TUNING
        private readonly byte[] _serviceName;
#endif

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
#if TUNING
        public ServiceName ServiceName
        {
            get { return _serviceName.ToServiceName(); }
        }
#else
        public ServiceName ServiceName { get; private set; }
#endif

#if TUNING
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
                capacity += 4; // ServiceName length
                capacity += _serviceName.Length; // ServiceName
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRequestMessage"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public ServiceRequestMessage(ServiceName serviceName)
        {
#if TUNING
            _serviceName = serviceName.ToArray();
#else
            ServiceName = serviceName;
#endif
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            throw new InvalidOperationException("Load data is not supported.");
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
#if TUNING
            WriteBinaryString(_serviceName);
#else
            switch (ServiceName)
            {
                case ServiceName.UserAuthentication:
                    WriteAscii("ssh-userauth");
                    break;
                case ServiceName.Connection:
                    WriteAscii("ssh-connection");
                    break;
                default:
                    throw new NotSupportedException("Not supported service name");
            }

#endif
        }
    }
}

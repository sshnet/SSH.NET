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
        private readonly byte[] _serviceName;

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        public ServiceName ServiceName
        {
            get { return _serviceName.ToServiceName(); }
        }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRequestMessage"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public ServiceRequestMessage(ServiceName serviceName)
        {
            _serviceName = serviceName.ToArray();
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
            WriteBinaryString(_serviceName);
        }

        internal override void Process(Session session)
        {
            session.OnServiceRequestReceived(this);
        }
    }
}

using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_SERVICE_ACCEPT message.
    /// </summary>
    [Message("SSH_MSG_SERVICE_ACCEPT", MessageNumber)]
    public class ServiceAcceptMessage : Message
    {
        internal const byte MessageNumber = 6;

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        public ServiceName ServiceName { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            ServiceName = ReadBinary().ToServiceName();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
#pragma warning disable MA0025 // Implement the functionality instead of throwing NotImplementedException
            throw new NotImplementedException();
#pragma warning restore MA0025 // Implement the functionality instead of throwing NotImplementedException
        }

        internal override void Process(Session session)
        {
            session.OnServiceAcceptReceived(this);
        }
    }
}

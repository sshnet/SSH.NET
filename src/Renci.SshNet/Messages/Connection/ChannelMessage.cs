using System.Globalization;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Base class for all channel specific SSH messages.
    /// </summary>
    public abstract class ChannelMessage : Message
    {
        /// <summary>
        /// Gets or sets the local channel number.
        /// </summary>
        /// <value>
        /// The local channel number.
        /// </value>
        public uint LocalChannelNumber { get; protected set; }

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
                capacity += 4; // LocalChannelNumber
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="ChannelMessage"/>.
        /// </summary>
        protected ChannelMessage()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ChannelMessage"/> with the specified local channel number.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        protected ChannelMessage(uint localChannelNumber)
        {
            LocalChannelNumber = localChannelNumber;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            LocalChannelNumber = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            Write(LocalChannelNumber);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} : #{1}", base.ToString(), LocalChannelNumber);
        }
    }
}

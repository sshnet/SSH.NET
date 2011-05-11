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
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.LocalChannelNumber = this.ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.Write(this.LocalChannelNumber);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} : #{1}", base.ToString(), this.LocalChannelNumber);
        }
    }
}

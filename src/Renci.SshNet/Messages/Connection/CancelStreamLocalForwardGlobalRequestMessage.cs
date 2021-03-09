using System;

namespace Renci.SshNet.Messages.Connection
{
    internal class CancelStreamLocalForwardGlobalRequestMessage : GlobalRequestMessage
    {
        private byte[] _socketPath;

        public CancelStreamLocalForwardGlobalRequestMessage(string socketPath)
            : base(Ascii.GetBytes("cancel-streamlocal-forward@openssh.com"), true)
        {
            SocketPath = socketPath;
        }

        /// <summary>
        /// Gets the socket path to bind to.
        /// </summary>
        public string SocketPath
        {
            get { return Utf8.GetString(_socketPath, 0, _socketPath.Length); }
            private set { _socketPath = Utf8.GetBytes(value); }
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
                capacity += 4; // AddressToBind length
                capacity += _socketPath.Length; // AddressToBind
                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(_socketPath);
        }
    }
}

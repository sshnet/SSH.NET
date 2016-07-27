using System;
using System.Text;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "exec" type channel request information
    /// </summary>
    internal class ExecRequestInfo : RequestInfo
    {
        private byte[] _command;

        /// <summary>
        /// Channel request name
        /// </summary>
        public const string Name = "exec";

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public override string RequestName
        {
            get { return Name; }
        }

        /// <summary>
        /// Gets command to execute.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public string Command
        {
            get { return Encoding.GetString(_command, 0, _command.Length); }
        }

        /// <summary>
        /// Gets the encoding.
        /// </summary>
        /// <value>
        /// The encoding.
        /// </value>
        public Encoding Encoding { get; private set; }

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
                capacity += 4; // Command length
                capacity += _command.Length; // Command
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecRequestInfo"/> class.
        /// </summary>
        public ExecRequestInfo()
        {
            WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecRequestInfo"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> or <paramref name="encoding"/> is <c>null</c>.</exception>
        public ExecRequestInfo(string command, Encoding encoding)
            : this()
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            _command = encoding.GetBytes(command);
            Encoding = encoding;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            _command = ReadBinary();
            Encoding = Utf8;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_command);
        }
    }
}

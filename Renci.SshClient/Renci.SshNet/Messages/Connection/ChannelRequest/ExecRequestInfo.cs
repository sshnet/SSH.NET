using System.Text;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "exec" type channel request information
    /// </summary>
    internal class ExecRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "exec";

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public override string RequestName
        {
            get { return ExecRequestInfo.NAME; }
        }

        /// <summary>
        /// Gets command to execute.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public string Command { get; private set; }

        /// <summary>
        /// Gets the encoding.
        /// </summary>
        /// <value>
        /// The encoding.
        /// </value>
        public Encoding Encoding { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecRequestInfo"/> class.
        /// </summary>
        public ExecRequestInfo()
        {
            this.WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecRequestInfo"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="command"/> is null.</exception>
        public ExecRequestInfo(string command, Encoding encoding)
            : this()
        {
            if (command == null)
                throw new System.ArgumentNullException("command");

            this.Command = command;
            this.Encoding = encoding;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            this.Command = this.ReadString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.Command, this.Encoding);
        }
    }
}

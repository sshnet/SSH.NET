using Renci.SshNet.Common;
using System.Collections.Generic;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "pty-req" type channel request information
    /// </summary>
    internal class PseudoTerminalRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "pty-req";

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public override string RequestName
        {
            get { return NAME; }
        }

        /// <summary>
        /// Gets or sets the environment variable.
        /// </summary>
        /// <value>
        /// The environment variable.
        /// </value>
        public string EnvironmentVariable { get; set; }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public uint Columns { get; set; }

        /// <summary>
        /// Gets or sets the rows.
        /// </summary>
        /// <value>
        /// The rows.
        /// </value>
        public uint Rows { get; set; }

        /// <summary>
        /// Gets or sets the width of the pixel.
        /// </summary>
        /// <value>
        /// The width of the pixel.
        /// </value>
        public uint PixelWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the pixel.
        /// </summary>
        /// <value>
        /// The height of the pixel.
        /// </value>
        public uint PixelHeight { get; set; }

        /// <summary>
        /// Gets or sets the terminal mode.
        /// </summary>
        /// <value>
        /// The terminal mode.
        /// </value>
        public IDictionary<TerminalModes, uint> TerminalModeValues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PseudoTerminalRequestInfo"/> class.
        /// </summary>
        public PseudoTerminalRequestInfo()
        {
            this.WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PseudoTerminalRequestInfo"/> class.
        /// </summary>
        /// <param name="environmentVariable">The environment variable.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="terminalModeValues">The terminal mode values.</param>
        public PseudoTerminalRequestInfo(string environmentVariable, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModeValues)
            : this()
        {
            this.EnvironmentVariable = environmentVariable;
            this.Columns = columns;
            this.Rows = rows;
            this.PixelWidth = width;
            this.PixelHeight = height;
            this.TerminalModeValues = terminalModeValues;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.EnvironmentVariable);
            this.Write(this.Columns);
            this.Write(this.Rows);
            this.Write(this.Rows);
            this.Write(this.PixelHeight);

            if (this.TerminalModeValues != null)
            {
                this.Write((uint)this.TerminalModeValues.Count * (1 + 4) + 1);

                foreach (var item in this.TerminalModeValues)
                {
                    this.Write((byte)item.Key);
                    this.Write(item.Value);
                }
                this.Write((byte)0);
            }
            else
            {
                this.Write((uint)0);
            }
        }
    }
}

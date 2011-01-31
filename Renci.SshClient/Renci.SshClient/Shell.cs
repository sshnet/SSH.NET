using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshClient.Channels;
using Renci.SshClient.Common;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient
{
    /// <summary>
    /// Represents instance of the SSH shell object
    /// </summary>
    public class Shell
    {
        private readonly Session _session;

        private ChannelSession _channel;

        private Stream _channelInput;

        private TextWriter _channelOutput;

        private TextWriter _channelExtendedOutput;

        private string _terminalName;

        private uint _columns;

        private uint _rows;

        private uint _width;

        private uint _height;

        private string _terminalMode;

        private Task _dataReaderTask;

        private Encoding _encoding;

        /// <summary>
        /// Gets a value indicating whether this shell is started.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if started is started; otherwise, <c>false</c>.
        /// </value>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// Occurs when shell is starting.
        /// </summary>
        public event EventHandler<EventArgs> Starting;

        /// <summary>
        /// Occurs when shell is started.
        /// </summary>
        public event EventHandler<EventArgs> Started;

        /// <summary>
        /// Occurs when shell is stopping.
        /// </summary>
        public event EventHandler<EventArgs> Stopping;

        /// <summary>
        /// Occurs when shell is stopped.
        /// </summary>
        public event EventHandler<EventArgs> Stopped;

        /// <summary>
        /// Occurs when an error occurred.
        /// </summary>
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// Initializes a new instance of the <see cref="Shell"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <param name="terminalName">Name of the terminal.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="terminalMode">The terminal mode.</param>
        internal Shell(Session session, Stream input, TextWriter output, TextWriter extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, string terminalMode)
        {
            this._session = session;
            this._channelInput = input;
            this._channelOutput = output;
            this._channelExtendedOutput = extendedOutput;
            this._terminalName = terminalName;
            this._columns = columns;
            this._rows = rows;
            this._width = width;
            this._height = height;
            this._terminalMode = terminalMode;
            this._encoding = Encoding.ASCII;
        }

        /// <summary>
        /// Starts this shell.
        /// </summary>
        public void Start()
        {
            if (this.IsStarted)
            {
                throw new SshException("Shell is started.");
            }

            if (this.Starting != null)
            {
                this.Starting(this, new EventArgs());
            }

            this._channel = this._session.CreateChannel<ChannelSession>();
            this._channel.DataReceived += Channel_DataReceived;
            this._channel.ExtendedDataReceived += Channel_ExtendedDataReceived;
            this._channel.Closed += Channel_Closed;
            this._session.Disconnected += Session_Disconnected;
            this._session.ErrorOccured += Session_ErrorOccured;

            this._channel.Open();
            this._channel.SendPseudoTerminalRequest(this._terminalName, this._columns, this._columns, this._width, this._height, this._terminalMode);
            this._channel.SendShellRequest();

            //  Start input stream listener
            this._dataReaderTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    while (this._channel.IsOpen)
                    {
                        var ch = this._channelInput.ReadByte();

                        if (ch > 0)
                        {
                            Debug.WriteLine(ch);

                            this._session.SendMessage(new ChannelDataMessage(this._channel.RemoteChannelNumber, new byte[] { (byte)ch }));
                        }
                        else
                        {
                            //  Wait for data become available
                            Thread.Sleep(30);
                        }
                    }
                }
                catch (Exception exp)
                {
                    this.RaiseError(new ErrorEventArgs(exp));
                }
            });

            this.IsStarted = true;

            if (this.Started != null)
            {
                this.Started(this, new EventArgs());
            }

        }

        /// <summary>
        /// Stops this shell.
        /// </summary>
        public void Stop()
        {
            if (!this.IsStarted)
            {
                throw new SshException("Shell is not started.");
            }

            if (this.Stopping != null)
            {
                this.Stopping(this, new EventArgs());
            }

            this._channel.Close();
            this._channelInput.Close();

            this._dataReaderTask.Wait();

            this._channel.DataReceived -= Channel_DataReceived;
            this._channel.ExtendedDataReceived -= Channel_ExtendedDataReceived;
            this._channel.Closed -= Channel_Closed;
            this._session.Disconnected -= Session_Disconnected;
            this._session.ErrorOccured -= Session_ErrorOccured;

            if (this.Stopped != null)
            {
                this.Stopped(this, new EventArgs());
            }
        }

        private void Session_ErrorOccured(object sender, ErrorEventArgs e)
        {
            this.RaiseError(e);
        }

        private void RaiseError(ErrorEventArgs e)
        {
            if (this.ErrorOccurred != null)
            {
                this.ErrorOccurred(this, e);
            }
        }

        private void Session_Disconnected(object sender, System.EventArgs e)
        {
            this.Stop();
        }

        private void Channel_ExtendedDataReceived(object sender, Common.ChannelDataEventArgs e)
        {
            if (this._channelExtendedOutput != null)
            {
                this._channelExtendedOutput.Write(e.Data);
            }
        }

        private void Channel_DataReceived(object sender, Common.ChannelDataEventArgs e)
        {
            if (this._channelOutput != null)
            {
                this._channelOutput.Write(e.Data);
            }
        }

        private void Channel_Closed(object sender, Common.ChannelEventArgs e)
        {
            this.Stop();
        }
    }
}

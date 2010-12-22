using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshClient.Channels;
using Renci.SshClient.Messages.Connection;
using Renci.SshClient.Common;

namespace Renci.SshClient
{
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

        public bool IsStarted { get; private set; }

        public event EventHandler<EventArgs> Starting;

        public event EventHandler<EventArgs> Started;

        public event EventHandler<EventArgs> Stopping;

        public event EventHandler<EventArgs> Stopped;

        public event EventHandler<ErrorEventArgs> ErrorOccured;

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

                            this._session.SendMessage(new ChannelDataMessage(this._channel.RemoteChannelNumber, Char.ConvertFromUtf32(ch)));
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

                    throw;
                }
            });

            this.IsStarted = true;

            if (this.Started != null)
            {
                this.Started(this, new EventArgs());
            }

        }

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
            this._session.Disconnected -= Session_Disconnected;
            this._session.ErrorOccured -= Session_ErrorOccured;

            if (this.Stopped != null)
            {
                this.Stopped(this, new EventArgs());
            }
        }

        private void Session_ErrorOccured(object sender, ErrorEventArgs e)
        {
            if (this.ErrorOccured != null)
            {
                this.ErrorOccured(this, e);
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

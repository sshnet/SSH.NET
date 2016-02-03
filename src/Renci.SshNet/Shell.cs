using System;
using System.Linq;
using System.IO;
using System.Threading;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System.Collections.Generic;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents instance of the SSH shell object
    /// </summary>
    public partial class Shell : IDisposable
    {
        private readonly ISession _session;

        private IChannelSession _channel;

        private EventWaitHandle _channelClosedWaitHandle;

        private Stream _input;

        private readonly string _terminalName;

        private readonly uint _columns;

        private readonly uint _rows;

        private readonly uint _width;

        private readonly uint _height;

        private readonly IDictionary<TerminalModes, uint> _terminalModes;

        private EventWaitHandle _dataReaderTaskCompleted;

        private readonly Stream _outputStream;

        private readonly Stream _extendedOutputStream;

        private readonly int _bufferSize;

        /// <summary>
        /// Gets a value indicating whether this shell is started.
        /// </summary>
        /// <value>
        /// <c>true</c> if started is started; otherwise, <c>false</c>.
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
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

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
        /// <param name="terminalModes">The terminal modes.</param>
        /// <param name="bufferSize">Size of the buffer for output stream.</param>
        internal Shell(ISession session, Stream input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModes, int bufferSize)
        {
            _session = session;
            _input = input;
            _outputStream = output;
            _extendedOutputStream = extendedOutput;
            _terminalName = terminalName;
            _columns = columns;
            _rows = rows;
            _width = width;
            _height = height;
            _terminalModes = terminalModes;
            _bufferSize = bufferSize;
        }

        /// <summary>
        /// Starts this shell.
        /// </summary>
        /// <exception cref="SshException">Shell is started.</exception>
        public void Start()
        {
            if (IsStarted)
            {
                throw new SshException("Shell is started.");
            }

            if (Starting != null)
            {
                Starting(this, new EventArgs());
            }

            _channel = _session.CreateChannelSession();
            _channel.DataReceived += Channel_DataReceived;
            _channel.ExtendedDataReceived += Channel_ExtendedDataReceived;
            _channel.Closed += Channel_Closed;
            _session.Disconnected += Session_Disconnected;
            _session.ErrorOccured += Session_ErrorOccured;

            _channel.Open();
            _channel.SendPseudoTerminalRequest(_terminalName, _columns, _rows, _width, _height, _terminalModes);
            _channel.SendShellRequest();

            _channelClosedWaitHandle = new AutoResetEvent(false);

            //  Start input stream listener
            _dataReaderTaskCompleted = new ManualResetEvent(false);
            ExecuteThread(() =>
            {
                try
                {
                    var buffer = new byte[_bufferSize];

                    while (_channel.IsOpen)
                    {
                        var asyncResult = _input.BeginRead(buffer, 0, buffer.Length, delegate(IAsyncResult result)
                        {
                            //  If input stream is closed and disposed already dont finish reading the stream
                            if (_input == null)
                                return;

                            var read = _input.EndRead(result);
                            if (read > 0)
                            {
#if TUNING
                                _channel.SendData(buffer, 0, read);
#else
                                _channel.SendData(buffer.Take(read).ToArray());
#endif
                            }

                        }, null);

                        EventWaitHandle.WaitAny(new WaitHandle[] { asyncResult.AsyncWaitHandle, _channelClosedWaitHandle });

                        if (asyncResult.IsCompleted)
                            continue;
                        break;
                    }
                }
                catch (Exception exp)
                {
                    RaiseError(new ExceptionEventArgs(exp));
                }
                finally
                {
                    _dataReaderTaskCompleted.Set();
                }
            });

            IsStarted = true;

            if (Started != null)
            {
                Started(this, new EventArgs());
            }
        }

        /// <summary>
        /// Stops this shell.
        /// </summary>
        /// <exception cref="SshException">Shell is not started.</exception>
        public void Stop()
        {
            if (!IsStarted)
            {
                throw new SshException("Shell is not started.");
            }

            if (_channel != null)
            {
                _channel.Close();
            }
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            RaiseError(e);
        }

        private void RaiseError(ExceptionEventArgs e)
        {
            var handler = ErrorOccurred;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            Stop();
        }

        private void Channel_ExtendedDataReceived(object sender, ChannelExtendedDataEventArgs e)
        {
            if (_extendedOutputStream != null)
            {
                _extendedOutputStream.Write(e.Data, 0, e.Data.Length);
            }
        }

        private void Channel_DataReceived(object sender, ChannelDataEventArgs e)
        {
            if (_outputStream != null)
            {
                _outputStream.Write(e.Data, 0, e.Data.Length);
            }
        }

        private void Channel_Closed(object sender, ChannelEventArgs e)
        {
            if (Stopping != null)
            {
                //  Handle event on different thread
                ExecuteThread(() => Stopping(this, new EventArgs()));
            }

            if (_channel.IsOpen)
            {
                _channel.Close();
            }

            _channelClosedWaitHandle.Set();

            _input.Dispose();
            _input = null;

            _dataReaderTaskCompleted.WaitOne(_session.ConnectionInfo.Timeout);
            _dataReaderTaskCompleted.Dispose();
            _dataReaderTaskCompleted = null;

            _channel.DataReceived -= Channel_DataReceived;
            _channel.ExtendedDataReceived -= Channel_ExtendedDataReceived;
            _channel.Closed -= Channel_Closed;
            _session.Disconnected -= Session_Disconnected;
            _session.ErrorOccured -= Session_ErrorOccured;

            if (Stopped != null)
            {
                //  Handle event on different thread
                ExecuteThread(() => Stopped(this, new EventArgs()));
            }

            _channel = null;
        }

        partial void ExecuteThread(Action action);

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged ResourceMessages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged ResourceMessages.
                if (disposing)
                {
                    if (_channelClosedWaitHandle != null)
                    {
                        _channelClosedWaitHandle.Dispose();
                        _channelClosedWaitHandle = null;
                    }

                    if (_channel != null)
                    {
                        _channel.Dispose();
                        _channel = null;
                    }

                    if (_dataReaderTaskCompleted != null)
                    {
                        _dataReaderTaskCompleted.Dispose();
                        _dataReaderTaskCompleted = null;
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Session"/> is reclaimed by garbage collection.
        /// </summary>
        ~Shell()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

    }
}

using System;
using System.IO;
using System.Threading;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System.Collections.Generic;
using Renci.SshNet.Abstractions;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents instance of the SSH shell object
    /// </summary>
    public class Shell : IDisposable
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
            ThreadAbstraction.ExecuteThread(() =>
            {
                try
                {
                    var buffer = new byte[_bufferSize];

                    while (_channel.IsOpen)
                    {
#if FEATURE_STREAM_TAP
                        var readTask = _input.ReadAsync(buffer, 0, buffer.Length);
                        var readWaitHandle = ((IAsyncResult) readTask).AsyncWaitHandle;

                        if (WaitHandle.WaitAny(new[] {readWaitHandle, _channelClosedWaitHandle}) == 0)
                        {
                            var read = readTask.GetAwaiter().GetResult();
                            _channel.SendData(buffer, 0, read);
                            continue;
                        }
#elif FEATURE_STREAM_APM
                        var asyncResult = _input.BeginRead(buffer, 0, buffer.Length, result =>
                            {
                                //  If input stream is closed and disposed already don't finish reading the stream
                                if (_input == null)
                                    return;

                                var read = _input.EndRead(result);
                                _channel.SendData(buffer, 0, read);
                            }, null);

                        WaitHandle.WaitAny(new[] { asyncResult.AsyncWaitHandle, _channelClosedWaitHandle });

                        if (asyncResult.IsCompleted)
                            continue;
#else
                        #error Async receive is not implemented.
#endif
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
                _channel.Dispose();
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
                ThreadAbstraction.ExecuteThread(() => Stopping(this, new EventArgs()));
            }

            _channel.Dispose();
            _channelClosedWaitHandle.Set();

            _input.Dispose();
            _input = null;

            _dataReaderTaskCompleted.WaitOne(_session.ConnectionInfo.Timeout);
            _dataReaderTaskCompleted.Dispose();
            _dataReaderTaskCompleted = null;

            _channel.DataReceived -= Channel_DataReceived;
            _channel.ExtendedDataReceived -= Channel_ExtendedDataReceived;
            _channel.Closed -= Channel_Closed;

            UnsubscribeFromSessionEvents(_session);

            if (Stopped != null)
            {
                //  Handle event on different thread
                ThreadAbstraction.ExecuteThread(() => Stopped(this, new EventArgs()));
            }

            _channel = null;
        }

        /// <summary>
        /// Unsubscribes the current <see cref="Shell"/> from session events.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <remarks>
        /// Does nothing when <paramref name="session"/> is <c>null</c>.
        /// </remarks>
        private void UnsubscribeFromSessionEvents(ISession session)
        {
            if (session == null)
                return;

            session.Disconnected -= Session_Disconnected;
            session.ErrorOccured -= Session_ErrorOccured;
        }

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                UnsubscribeFromSessionEvents(_session);

                var channelClosedWaitHandle = _channelClosedWaitHandle;
                if (channelClosedWaitHandle != null)
                {
                    channelClosedWaitHandle.Dispose();
                    _channelClosedWaitHandle = null;
                }

                var channel = _channel;
                if (channel != null)
                {
                    channel.Dispose();
                    _channel = null;
                }

                var dataReaderTaskCompleted = _dataReaderTaskCompleted;
                if (dataReaderTaskCompleted != null)
                {
                    dataReaderTaskCompleted.Dispose();
                    _dataReaderTaskCompleted = null;
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Shell"/> is reclaimed by garbage collection.
        /// </summary>
        ~Shell()
        {
            Dispose(false);
        }

        #endregion

    }
}

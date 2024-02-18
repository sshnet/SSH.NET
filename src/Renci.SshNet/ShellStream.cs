#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Contains operation for working with SSH Shell.
    /// </summary>
    public class ShellStream : Stream
    {
        private readonly ISession _session;
        private readonly Encoding _encoding;
        private readonly IChannelSession _channel;
        private readonly byte[] _carriageReturnBytes;
        private readonly byte[] _lineFeedBytes;

        private readonly object _sync = new object();

        private byte[] _buffer;
        private int _head; // The index from which the data starts in _buffer.
        private int _tail; // The index at which to add new data into _buffer.
        private bool _disposed;

        /// <summary>
        /// Occurs when data was received.
        /// </summary>
        public event EventHandler<ShellDataEventArgs>? DataReceived;

        /// <summary>
        /// Occurs when an error occurred.
        /// </summary>
        public event EventHandler<ExceptionEventArgs>? ErrorOccurred;

        /// <summary>
        /// Gets a value indicating whether data is available on the <see cref="ShellStream"/> to be read.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if data is available to be read; otherwise, <see langword="false"/>.
        /// </value>
        public bool DataAvailable
        {
            get
            {
                lock (_sync)
                {
                    AssertValid();
                    return _tail != _head;
                }
            }
        }

#pragma warning disable MA0076 // Do not use implicit culture-sensitive ToString in interpolated strings
        [Conditional("DEBUG")]
        private void AssertValid()
        {
            Debug.Assert(Monitor.IsEntered(_sync), $"Should be in lock on {nameof(_sync)}");
            Debug.Assert(_head >= 0, $"{nameof(_head)} should be non-negative but is {_head}");
            Debug.Assert(_tail >= 0, $"{nameof(_tail)} should be non-negative but is {_tail}");
            Debug.Assert(_head < _buffer.Length || _buffer.Length == 0, $"{nameof(_head)} should be < {nameof(_buffer)}.Length but is {_head}");
            Debug.Assert(_tail <= _buffer.Length, $"{nameof(_tail)} should be <= {nameof(_buffer)}.Length but is {_tail}");
            Debug.Assert(_head <= _tail, $"Should have {nameof(_head)} <= {nameof(_tail)} but have {_head} <= {_tail}");
        }
#pragma warning restore MA0076 // Do not use implicit culture-sensitive ToString in interpolated strings

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellStream"/> class.
        /// </summary>
        /// <param name="session">The SSH session.</param>
        /// <param name="terminalName">The <c>TERM</c> environment variable.</param>
        /// <param name="columns">The terminal width in columns.</param>
        /// <param name="rows">The terminal width in rows.</param>
        /// <param name="width">The terminal width in pixels.</param>
        /// <param name="height">The terminal height in pixels.</param>
        /// <param name="terminalModeValues">The terminal mode values.</param>
        /// <param name="bufferSize">The size of the buffer.</param>
        /// <exception cref="SshException">The channel could not be opened.</exception>
        /// <exception cref="SshException">The pseudo-terminal request was not accepted by the server.</exception>
        /// <exception cref="SshException">The request to start a shell was not accepted by the server.</exception>
        internal ShellStream(ISession session, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModeValues, int bufferSize)
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
#else
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }
#endif

            _encoding = session.ConnectionInfo.Encoding;
            _session = session;
            _carriageReturnBytes = _encoding.GetBytes("\r");
            _lineFeedBytes = _encoding.GetBytes("\n");

            _channel = _session.CreateChannelSession();
            _channel.DataReceived += Channel_DataReceived;
            _channel.Closed += Channel_Closed;
            _session.Disconnected += Session_Disconnected;
            _session.ErrorOccured += Session_ErrorOccured;

            _buffer = new byte[bufferSize];

            try
            {
                _channel.Open();

                if (!_channel.SendPseudoTerminalRequest(terminalName, columns, rows, width, height, terminalModeValues))
                {
                    throw new SshException("The pseudo-terminal request was not accepted by the server. Consult the server log for more information.");
                }

                if (!_channel.SendShellRequest())
                {
                    throw new SshException("The request to start a shell was not accepted by the server. Consult the server log for more information.");
                }
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// <see langword="true"/>.
        /// </returns>
        /// <remarks>
        /// It is safe to read from <see cref="ShellStream"/> even after disposal.
        /// </remarks>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// <see langword="false"/>.
        /// </returns>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this stream has not been disposed and the underlying channel
        /// is still open, otherwise <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// A value of <see langword="true"/> does not necessarily mean a write will succeed. It is possible
        /// that the channel is closed and/or the stream is disposed by another thread between a call to
        /// <see cref="CanWrite"/> and the call to write.
        /// </remarks>
        public override bool CanWrite
        {
            get { return !_disposed; }
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Gets the number of bytes currently available for reading.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        public override long Length
        {
            get
            {
                lock (_sync)
                {
                    AssertValid();
                    return _tail - _head;
                }
            }
        }

        /// <summary>
        /// This property always returns 0, and throws <see cref="NotSupportedException"/>
        /// when calling the setter.
        /// </summary>
        /// <returns>
        /// 0.
        /// </returns>
        /// <exception cref="NotSupportedException">The setter is called.</exception>
#pragma warning disable SA1623 // The property's documentation should begin with 'Gets or sets'
        public override long Position
#pragma warning restore SA1623 // The property's documentation should begin with 'Gets or sets'
        {
            get { return 0; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// This method always throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>Never.</returns>
        /// <exception cref="NotSupportedException">Always.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method always throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="NotSupportedException">Always.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Expects the specified expression and performs action when one is found.
        /// </summary>
        /// <param name="expectActions">The expected expressions and actions to perform.</param>
        public void Expect(params ExpectAction[] expectActions)
        {
            Expect(Timeout.InfiniteTimeSpan, expectActions);
        }

        /// <summary>
        /// Expects the specified expression and performs action when one is found.
        /// </summary>
        /// <param name="timeout">Time to wait for input. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
        /// <param name="expectActions">The expected expressions and actions to perform, if the specified time elapsed and expected condition have not met, that method will exit without executing any action.</param>
        /// <remarks>
        /// If a TimeSpan representing -1 millisecond is specified for the <paramref name="timeout"/> parameter,
        /// this method blocks indefinitely until either the regex matches the data in the buffer, or the stream
        /// is closed (via disposal or via the underlying channel closing).
        /// </remarks>
        public void Expect(TimeSpan timeout, params ExpectAction[] expectActions)
        {
            _ = ExpectRegex(timeout, lookback: -1, expectActions);
        }

        /// <summary>
        /// Expects the specified expression and performs action when one is found.
        /// </summary>
        /// <param name="timeout">Time to wait for input. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
        /// <param name="lookback">The amount of data to search through from the most recent data in the buffer, or -1 to always search the entire buffer.</param>
        /// <param name="expectActions">The expected expressions and actions to perform, if the specified time elapsed and expected condition have not met, that method will exit without executing any action.</param>
        /// <remarks>
        /// <para>
        /// If a TimeSpan representing -1 millisecond is specified for the <paramref name="timeout"/> parameter,
        /// this method blocks indefinitely until either the regex matches the data in the buffer, or the stream
        /// is closed (via disposal or via the underlying channel closing).
        /// </para>
        /// <para>
        /// Use the <paramref name="lookback"/> parameter to constrain the search space to a fixed-size rolling window at the end of the buffer.
        /// This can reduce the amount of work done in cases where lots of output from the shell is expected to be received before the matching expression is found.
        /// </para>
        /// <para>
        /// Note: in situations with high volumes of data and a small value for <paramref name="lookback"/>, some data may not be searched through.
        /// It is recommended to set <paramref name="lookback"/> to a large enough value to be able to search all data as it comes in,
        /// but which still places a limit on the amount of work needed.
        /// </para>
        /// </remarks>
        public void Expect(TimeSpan timeout, int lookback, params ExpectAction[] expectActions)
        {
            _ = ExpectRegex(timeout, lookback, expectActions);
        }

        /// <summary>
        /// Expects the expression specified by text.
        /// </summary>
        /// <param name="text">The text to expect.</param>
        /// <returns>
        /// The text available in the shell up to and including the expected text,
        /// or <see langword="null"/> if the the stream is closed without a match.
        /// </returns>
        public string? Expect(string text)
        {
            return Expect(text, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Expects the expression specified by text.
        /// </summary>
        /// <param name="text">The text to expect.</param>
        /// <param name="timeout">Time to wait for input. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
        /// <param name="lookback">The amount of data to search through from the most recent data in the buffer, or -1 to always search the entire buffer.</param>
        /// <returns>
        /// The text available in the shell up to and including the expected expression,
        /// or <see langword="null"/> if the specified time has elapsed or the stream is closed
        /// without a match.
        /// </returns>
        /// <remarks><inheritdoc cref="Expect(TimeSpan, int, ExpectAction[])"/></remarks>
        public string? Expect(string text, TimeSpan timeout, int lookback = -1)
        {
            ValidateTimeout(timeout);
            ValidateLookback(lookback);

            var timeoutTime = DateTime.Now.Add(timeout);

            var expectBytes = _encoding.GetBytes(text);

            lock (_sync)
            {
                while (true)
                {
                    AssertValid();

                    var searchHead = lookback == -1
                        ? _head
                        : Math.Max(_tail - lookback, _head);

                    Debug.Assert(_head <= searchHead && searchHead <= _tail);

#if NETFRAMEWORK || NETSTANDARD2_0
                    var indexOfMatch = _buffer.IndexOf(expectBytes, searchHead, _tail - searchHead);
#else
                    var indexOfMatch = _buffer.AsSpan(searchHead, _tail - searchHead).IndexOf(expectBytes);
#endif

                    if (indexOfMatch >= 0)
                    {
                        var returnText = _encoding.GetString(_buffer, _head, searchHead - _head + indexOfMatch + expectBytes.Length);

                        _head = searchHead + indexOfMatch + expectBytes.Length;

                        AssertValid();

                        return returnText;
                    }

                    if (_disposed)
                    {
                        return null;
                    }

                    if (timeout == Timeout.InfiniteTimeSpan)
                    {
                        Monitor.Wait(_sync);
                    }
                    else
                    {
                        var waitTimeout = timeoutTime - DateTime.Now;

                        if (waitTimeout < TimeSpan.Zero || !Monitor.Wait(_sync, waitTimeout))
                        {
                            return null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Expects the expression specified by regular expression.
        /// </summary>
        /// <param name="regex">The regular expression to expect.</param>
        /// <returns>
        /// The text available in the shell up to and including the expected expression,
        /// or <see langword="null"/> if the stream is closed without a match.
        /// </returns>
        public string? Expect(Regex regex)
        {
            return Expect(regex, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Expects the expression specified by regular expression.
        /// </summary>
        /// <param name="regex">The regular expression to expect.</param>
        /// <param name="timeout">Time to wait for input. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
        /// <param name="lookback">The amount of data to search through from the most recent data in the buffer, or -1 to always search the entire buffer.</param>
        /// <returns>
        /// The text available in the shell up to and including the expected expression,
        /// or <see langword="null"/> if the specified timeout has elapsed or the stream
        /// is closed without a match.
        /// </returns>
        /// <remarks>
        /// <inheritdoc cref="Expect(TimeSpan, int, ExpectAction[])"/>
        /// </remarks>
        public string? Expect(Regex regex, TimeSpan timeout, int lookback = -1)
        {
            return ExpectRegex(timeout, lookback, [new ExpectAction(regex, s => { })]);
        }

        private string? ExpectRegex(TimeSpan timeout, int lookback, ExpectAction[] expectActions)
        {
            ValidateTimeout(timeout);
            ValidateLookback(lookback);

            var timeoutTime = DateTime.Now.Add(timeout);

            lock (_sync)
            {
                while (true)
                {
                    AssertValid();

                    var bufferText = _encoding.GetString(_buffer, _head, _tail - _head);

                    var searchStart = lookback == -1
                        ? 0
                        : Math.Max(bufferText.Length - lookback, 0);

                    foreach (var expectAction in expectActions)
                    {
#if NET7_0_OR_GREATER
                        var matchEnumerator = expectAction.Expect.EnumerateMatches(bufferText.AsSpan(searchStart));

                        if (matchEnumerator.MoveNext())
                        {
                            var match = matchEnumerator.Current;

                            var returnText = bufferText.Substring(0, searchStart + match.Index + match.Length);
#else
                        var match = expectAction.Expect.Match(bufferText, searchStart);

                        if (match.Success)
                        {
                            var returnText = bufferText.Substring(0, match.Index + match.Length);
#endif
                            _head += _encoding.GetByteCount(returnText);

                            AssertValid();

                            expectAction.Action(returnText);

                            return returnText;
                        }
                    }

                    if (_disposed)
                    {
                        return null;
                    }

                    if (timeout == Timeout.InfiniteTimeSpan)
                    {
                        Monitor.Wait(_sync);
                    }
                    else
                    {
                        var waitTimeout = timeoutTime - DateTime.Now;

                        if (waitTimeout < TimeSpan.Zero || !Monitor.Wait(_sync, waitTimeout))
                        {
                            return null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Begins the expect.
        /// </summary>
        /// <param name="expectActions">The expect actions.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        public IAsyncResult BeginExpect(params ExpectAction[] expectActions)
        {
            return BeginExpect(Timeout.InfiniteTimeSpan, callback: null, state: null, expectActions);
        }

        /// <summary>
        /// Begins the expect.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="expectActions">The expect actions.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        public IAsyncResult BeginExpect(AsyncCallback? callback, params ExpectAction[] expectActions)
        {
            return BeginExpect(Timeout.InfiniteTimeSpan, callback, state: null, expectActions);
        }

        /// <summary>
        /// Begins the expect.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="expectActions">The expect actions.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        public IAsyncResult BeginExpect(AsyncCallback? callback, object? state, params ExpectAction[] expectActions)
        {
            return BeginExpect(Timeout.InfiniteTimeSpan, callback, state, expectActions);
        }

        /// <summary>
        /// Begins the expect.
        /// </summary>
        /// <param name="timeout">The timeout. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="expectActions">The expect actions.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        public IAsyncResult BeginExpect(TimeSpan timeout, AsyncCallback? callback, object? state, params ExpectAction[] expectActions)
        {
            return BeginExpect(timeout, lookback: -1, callback, state, expectActions);
        }

        /// <summary>
        /// Begins the expect.
        /// </summary>
        /// <param name="timeout">The timeout. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
        /// <param name="lookback">The amount of data to search through from the most recent data in the buffer, or -1 to always search the entire buffer.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="expectActions">The expect actions.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        public IAsyncResult BeginExpect(TimeSpan timeout, int lookback, AsyncCallback? callback, object? state, params ExpectAction[] expectActions)
        {
            return TaskToAsyncResult.Begin(Task.Run(() => ExpectRegex(timeout, lookback, expectActions)), callback, state);
        }

        /// <summary>
        /// Ends the execute.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        /// <returns>
        /// The text available in the shell up to and including the expected expression.
        /// </returns>
        public string? EndExpect(IAsyncResult asyncResult)
        {
            return TaskToAsyncResult.End<string?>(asyncResult);
        }

        /// <summary>
        /// Reads the next line from the shell. If a line is not available it will block and wait for a new line.
        /// </summary>
        /// <returns>
        /// The line read from the shell.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method blocks indefinitely until either a line is available in the buffer, or the stream is closed
        /// (via disposal or via the underlying channel closing).
        /// </para>
        /// <para>
        /// When the stream is closed and there are no more newlines in the buffer, this method returns the remaining data
        /// (if any) and then <see langword="null"/> indicating that no more data is in the buffer.
        /// </para>
        /// </remarks>
        public string? ReadLine()
        {
            return ReadLine(Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Reads a line from the shell. If line is not available it will block the execution and will wait for new line.
        /// </summary>
        /// <param name="timeout">Time to wait for input. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
        /// <returns>
        /// The line read from the shell, or <see langword="null"/> when no input is received for the specified timeout.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If a TimeSpan representing -1 millisecond is specified for the <paramref name="timeout"/> parameter, this method
        /// blocks indefinitely until either a line is available in the buffer, or the stream is closed (via disposal or via
        /// the underlying channel closing).
        /// </para>
        /// <para>
        /// When the stream is closed and there are no more newlines in the buffer, this method returns the remaining data
        /// (if any) and then <see langword="null"/> indicating that no more data is in the buffer.
        /// </para>
        /// </remarks>
        public string? ReadLine(TimeSpan timeout)
        {
            ValidateTimeout(timeout);

            var timeoutTime = DateTime.Now.Add(timeout);

            lock (_sync)
            {
                while (true)
                {
                    AssertValid();

#if NETFRAMEWORK || NETSTANDARD2_0
                    var indexOfCr = _buffer.IndexOf(_carriageReturnBytes, _head, _tail - _head);
#else
                    var indexOfCr = _buffer.AsSpan(_head, _tail - _head).IndexOf(_carriageReturnBytes);
#endif
                    if (indexOfCr >= 0)
                    {
                        // We have found \r. We only need to search for \n up to and just after the \r
                        // (in order to consume \r\n if we can).
#if NETFRAMEWORK || NETSTANDARD2_0
                        var indexOfLf = indexOfCr + _carriageReturnBytes.Length + _lineFeedBytes.Length <= _tail - _head
                            ? _buffer.IndexOf(_lineFeedBytes, _head, indexOfCr + _carriageReturnBytes.Length + _lineFeedBytes.Length)
                            : _buffer.IndexOf(_lineFeedBytes, _head, indexOfCr);
#else
                        var indexOfLf = indexOfCr + _carriageReturnBytes.Length + _lineFeedBytes.Length <= _tail - _head
                            ? _buffer.AsSpan(_head, indexOfCr + _carriageReturnBytes.Length + _lineFeedBytes.Length).IndexOf(_lineFeedBytes)
                            : _buffer.AsSpan(_head, indexOfCr).IndexOf(_lineFeedBytes);
#endif
                        if (indexOfLf >= 0 && indexOfLf < indexOfCr)
                        {
                            // If there is \n before the \r, then return up to the \n
                            var returnText = _encoding.GetString(_buffer, _head, indexOfLf);

                            _head += indexOfLf + _lineFeedBytes.Length;

                            AssertValid();

                            return returnText;
                        }
                        else if (indexOfLf == indexOfCr + _carriageReturnBytes.Length)
                        {
                            // If we have \r\n, then consume both
                            var returnText = _encoding.GetString(_buffer, _head, indexOfCr);

                            _head += indexOfCr + _carriageReturnBytes.Length + _lineFeedBytes.Length;

                            AssertValid();

                            return returnText;
                        }
                        else
                        {
                            // Return up to the \r
                            var returnText = _encoding.GetString(_buffer, _head, indexOfCr);

                            _head += indexOfCr + _carriageReturnBytes.Length;

                            AssertValid();

                            return returnText;
                        }
                    }
                    else
                    {
                        // There is no \r. What about \n?
#if NETFRAMEWORK || NETSTANDARD2_0
                        var indexOfLf = _buffer.IndexOf(_lineFeedBytes, _head, _tail - _head);
#else
                        var indexOfLf = _buffer.AsSpan(_head, _tail - _head).IndexOf(_lineFeedBytes);
#endif
                        if (indexOfLf >= 0)
                        {
                            var returnText = _encoding.GetString(_buffer, _head, indexOfLf);

                            _head += indexOfLf + _lineFeedBytes.Length;

                            AssertValid();

                            return returnText;
                        }
                    }

                    if (_disposed)
                    {
                        var lastLine = _head == _tail
                            ? null
                            : _encoding.GetString(_buffer, _head, _tail - _head);

                        _head = _tail = 0;

                        return lastLine;
                    }

                    if (timeout == Timeout.InfiniteTimeSpan)
                    {
                        _ = Monitor.Wait(_sync);
                    }
                    else
                    {
                        var waitTimeout = timeoutTime - DateTime.Now;

                        if (waitTimeout < TimeSpan.Zero || !Monitor.Wait(_sync, waitTimeout))
                        {
                            return null;
                        }
                    }
                }
            }
        }

        private static void ValidateTimeout(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "Value must be non-negative or equal to -1 millisecond (for infinite timeout)");
            }
        }

        private static void ValidateLookback(int lookback)
        {
            if (lookback is <= 0 and not -1)
            {
                throw new ArgumentOutOfRangeException(nameof(lookback), "Value must be positive or equal to -1 (for no window)");
            }
        }

        /// <summary>
        /// Reads all of the text currently available in the shell.
        /// </summary>
        /// <returns>
        /// The text available in the shell.
        /// </returns>
        public string Read()
        {
            lock (_sync)
            {
                AssertValid();

                var text = _encoding.GetString(_buffer, _head, _tail - _head);

                _head = _tail = 0;

                return text;
            }
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_sync)
            {
                while (_head == _tail && !_disposed)
                {
                    _ = Monitor.Wait(_sync);
                }

                AssertValid();

                var bytesRead = Math.Min(count, _tail - _head);

                Buffer.BlockCopy(_buffer, _head, buffer, offset, bytesRead);

                _head += bytesRead;

                AssertValid();

                return bytesRead;
            }
        }

        /// <summary>
        /// Writes the specified text to the shell.
        /// </summary>
        /// <param name="text">The text to be written to the shell.</param>
        /// <remarks>
        /// <para>
        /// If <paramref name="text"/> is <see langword="null"/>, nothing is written.
        /// </para>
        /// <para>
        /// Data is not buffered before being written to the shell. If you have text to send in many pieces,
        /// consider wrapping this stream in a <see cref="StreamWriter"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public void Write(string? text)
        {
            if (text is null)
            {
                return;
            }

            var data = _encoding.GetBytes(text);

            Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes a sequence of bytes to the shell.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method sends <paramref name="count"/> bytes from buffer to the shell.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin sending bytes to the shell.</param>
        /// <param name="count">The number of bytes to be sent to the shell.</param>
        /// <remarks>
        /// Data is not buffered before being written to the shell. If you have data to send in many pieces,
        /// consider wrapping this stream in a <see cref="BufferedStream"/>.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
#if NET7_0_OR_GREATER
            ObjectDisposedException.ThrowIf(_disposed, this);
#else
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
#endif // NET7_0_OR_GREATER

            _channel.SendData(buffer, offset, count);
        }

        /// <summary>
        /// Writes the line to the shell.
        /// </summary>
        /// <param name="line">The line to be written to the shell.</param>
        /// <remarks>
        /// If <paramref name="line"/> is <see langword="null"/>, only the line terminator is written.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public void WriteLine(string line)
        {
            Write(line + "\r");
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                base.Dispose(disposing);
                return;
            }

            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                // Do not dispose _session (we don't own it)
                _session.Disconnected -= Session_Disconnected;
                _session.ErrorOccured -= Session_ErrorOccured;

                // But we do own _channel
                _channel.DataReceived -= Channel_DataReceived;
                _channel.Closed -= Channel_Closed;
                _channel.Dispose();

                Monitor.PulseAll(_sync);
            }

            base.Dispose(disposing);
        }

        private void Session_ErrorOccured(object? sender, ExceptionEventArgs e)
        {
            ErrorOccurred?.Invoke(this, e);
        }

        private void Session_Disconnected(object? sender, EventArgs e)
        {
            Dispose();
        }

        private void Channel_Closed(object? sender, ChannelEventArgs e)
        {
            Dispose();
        }

        private void Channel_DataReceived(object? sender, ChannelDataEventArgs e)
        {
            lock (_sync)
            {
                AssertValid();

                // Ensure sufficient buffer space and copy the new data in.

                if (_buffer.Length - _tail >= e.Data.Length)
                {
                    // If there is enough space after _tail for the new data,
                    // then copy the data there.
                    Buffer.BlockCopy(e.Data, 0, _buffer, _tail, e.Data.Length);
                    _tail += e.Data.Length;
                }
                else
                {
                    // We can't fit the new data after _tail.

                    var newLength = _tail - _head + e.Data.Length;

                    if (newLength <= _buffer.Length)
                    {
                        // If there is sufficient space at the start of the buffer,
                        // then move the current data to the start of the buffer.
                        Buffer.BlockCopy(_buffer, _head, _buffer, 0, _tail - _head);
                    }
                    else
                    {
                        // Otherwise, we're gonna need a bigger buffer.
                        var newBuffer = new byte[_buffer.Length * 2];
                        Buffer.BlockCopy(_buffer, _head, newBuffer, 0, _tail - _head);
                        _buffer = newBuffer;
                    }

                    // Copy the new data into the freed-up space.
                    Buffer.BlockCopy(e.Data, 0, _buffer, _tail - _head, e.Data.Length);

                    _head = 0;
                    _tail = newLength;
                }

                AssertValid();

                Monitor.PulseAll(_sync);
            }

            DataReceived?.Invoke(this, new ShellDataEventArgs(e.Data));
        }
    }
}

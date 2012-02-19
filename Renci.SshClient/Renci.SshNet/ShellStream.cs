using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System.Threading;
using System.Text.RegularExpressions;

namespace Renci.SshNet
{
    public enum LineMatchModes
    {
        Everything,
        Compleated,
        Partial,
    }

    public class ShellStream : Stream
    {
        int _maxLines = 0;

        List<string> _compleatedLines;
        StringBuilder _currentLine;

        Regex[] _patternMatchers = null;
        LineMatchModes _lineMatchMode;
        int _patternMatcherIndex;
        string _patternCaptureBuffer;

        private readonly Session _session;
        private ChannelSession _channel;

        public delegate void DataReceivedHandler(byte[] data);
        public event DataReceivedHandler DataReceived;

        public delegate void LineReadHandler(string Line);
        public event LineReadHandler LineRead;

        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        internal ShellStream(Session session, string terminalName, uint columns, uint rows, uint width, uint height, int maxLines, params KeyValuePair<TerminalModes, uint>[] terminalModeValues)
        {
            this._session = session;

            _compleatedLines = new List<string>();
            _currentLine = new StringBuilder();

            this._channel = this._session.CreateChannel<ChannelSession>();
            this._channel.DataReceived += new EventHandler<ChannelDataEventArgs>(_channel_DataReceived);
            this._channel.Closed += new EventHandler<ChannelEventArgs>(_channel_Closed);
            this._session.Disconnected += new EventHandler<EventArgs>(_session_Disconnected);
            this._session.ErrorOccured += new EventHandler<ExceptionEventArgs>(_session_ErrorOccured);

            this._channel.Open();
            this._channel.SendPseudoTerminalRequest(terminalName, columns, rows, width, height, terminalModeValues);
            this._channel.SendShellRequest();
        }

        void _session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            this.OnRaiseError(e);
        }

        void _session_Disconnected(object sender, EventArgs e)
        {
            //  If channel is open then close it to cause Channel_Closed method to be called
            if (this._channel != null && this._channel.IsOpen)
            {
                this._channel.SendEof();

                this._channel.Close();
            }
        }

        void _channel_Closed(object sender, ChannelEventArgs e)
        {
            this.Dispose();
        }

        void _channel_DataReceived(object sender, ChannelDataEventArgs e)
        {
            lock (this._compleatedLines)
            {
                OnDataReceived(e.Data);

                int startingBufferCount = this._compleatedLines.Count();

                for (int bufferOffset = 0; bufferOffset < e.Data.Length; bufferOffset++)
                {
                    // we got a line terminator, save the last line to the line queue
                    if (e.Data[bufferOffset] == '\n')
                    {
                        // Check if there was a \r preceading the \n and remove it
                        if (this._currentLine.Length > 0 && _currentLine[_currentLine.Length - 1] == '\r')
                            this._currentLine.Remove(_currentLine.Length - 1, 1);

                        // move the current line to the end of the line buffer
                        string lineBuffer = _currentLine.ToString();
                        this._compleatedLines.Add(lineBuffer);
                        this._currentLine.Clear();

                        // raise a OnLineRead event
                        OnLineRead(lineBuffer);
                    }
                    else
                    {
                        // Everything else, add it to the current line
                        this._currentLine.Append((char)e.Data[bufferOffset]);
                    }
                }

                // If there are any pattern matchers defined, from an existing call to ReadLines, check to see if any of the new lines match it
                if (this._patternMatchers != null)
                {
                    string results = ExtractCaptureBuffer(this._compleatedLines, this._patternMatchers, out this._patternMatcherIndex, _currentLine, this._lineMatchMode, startingBufferCount);
                    if (results != null)
                    {
                        this._patternCaptureBuffer = results;
                        // Signal ReadLines to continue
                        Monitor.Pulse(this._compleatedLines);
                    }
                }
            }

        }

        private void OnRaiseError(ExceptionEventArgs e)
        {
            if (this.ErrorOccurred != null)
                this.ErrorOccurred(this, e);
        }

        #region Not Implemented Stream Methods

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Generic overridden Stream methods
        public override bool CanRead
        {
            get
            {
                return (false);
            }
        }

        public override bool CanSeek
        {
            get
            {
                return (false);
            }
        }

        public override bool CanWrite
        {
            get
            {
                return (true);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset != 0 && count != buffer.Length)
            {
                byte[] constrainedBuffer = new byte[count - offset];

                Array.Copy(buffer, offset, constrainedBuffer, 0, count - offset);

                this._channel.SendData(constrainedBuffer);
            }
            else
                this._channel.SendData(buffer);
        }

        #endregion

        public void Write(string line)
        {
            byte[] dataToSend = Encoding.ASCII.GetBytes(line);

            this.Write(dataToSend, 0, dataToSend.Length);
        }

        public void WriteLine(string line)
        {
            this.Write(line + '\n');
        }

        public void WriteLine()
        {
            this.Write("\n");
        }


        private void OnDataReceived(byte[] data)
        {
            if (DataReceived != null)
                DataReceived(data);
        }

        private void OnLineRead(string line)
        {
            if (LineRead != null)
                LineRead(line);
        }

        public void Clear()
        {
            this._compleatedLines.Clear();
            this._currentLine.Clear();
        }

        public string ExpectLine()
        {
            string result = ExpectLine(Timeout.Infinite);

            return (result);
        }

        public string ExpectLine(int millisecondTimeout)
        {
            Regex matchAnything = new Regex("");

            string result = Expect(matchAnything, millisecondTimeout, LineMatchModes.Compleated);

            return (result);
        }

        public string TryExpectLine()
        {
            string result = ExpectLine(0);

            return (result);
        }

        public string Expect(string SearchPattern, int millisecondTimeout, LineMatchModes MatchMode)
        {
            Regex searchPattern = new Regex(Regex.Escape(SearchPattern));

            string result = Expect(searchPattern, millisecondTimeout, MatchMode);

            return (result);
        }

        public string Expect(Regex SearchPattern, int millisecondTimeout, LineMatchModes MatchMode)
        {
            int patternIndex = 0;
            Regex[] searchPatternsArray = { SearchPattern };

            string result = Expect(searchPatternsArray, out patternIndex, millisecondTimeout, MatchMode);

            return (result);
        }

        public string Expect(Regex[] searchPatterns, out Regex matchedPattern, int millisecondTimeout, LineMatchModes matchMode)
        {
            int matchedIndex = 0;

            string results = Expect(searchPatterns, out matchedIndex, millisecondTimeout, matchMode);

            matchedPattern = searchPatterns[matchedIndex];

            return (results);
        }

        public string Expect(out Regex matchedPattern, int millisecondTimeout, LineMatchModes matchMode, params Regex[] searchPatterns)
        {
            string result = Expect(searchPatterns, out matchedPattern, millisecondTimeout, matchMode);

            return (result);
        }

        public string Expect(Regex[] searchPatterns, out int matchedIndex, int millisecondTimeout, LineMatchModes matchMode)
        {
            lock (this._compleatedLines)
            {
                string result = ExtractCaptureBuffer(this._compleatedLines, searchPatterns, out matchedIndex, _currentLine, matchMode, 0);
                if (result != null)
                    return (result);
                else
                {
                    this._patternMatchers = searchPatterns;
                    this._lineMatchMode = matchMode;

                    bool dataAvalableFlag = Monitor.Wait(this._compleatedLines, millisecondTimeout);

                    this._patternMatchers = null;

                    if (dataAvalableFlag)
                    {
                        matchedIndex = this._patternMatcherIndex;
                        result = this._patternCaptureBuffer;

                        this._patternCaptureBuffer = null;

                        return (result);

                    }
                    else
                        return (null);
                }
            }
        }

        public string ReadAll()
        {
            lock (this._compleatedLines)
            {

                if (this._currentLine.Length > 0)
                {
                    this._currentLine.Insert(0,'\n');
                    this._currentLine.Insert(0, String.Join("\n", this._compleatedLines));
                }
                
                string results = this._currentLine.ToString();

                this.Clear();

                return (results);
            }
        }

        private string ExtractCaptureBuffer(List<string> lineBufferToMatch, Regex[] matchedPatterns, out int matchedIndex, StringBuilder currentLine, LineMatchModes matchMode, int startIndex)
        {
            // Check the line buffer to see if a compleated line matches
            if (matchMode == LineMatchModes.Compleated || matchMode == LineMatchModes.Everything)
            {
                for (int lineNumber = startIndex; lineNumber < lineBufferToMatch.Count; lineNumber++)
                {
                    for (int matcherIndex = 0; matcherIndex < matchedPatterns.Length; matcherIndex++)
                    {
                        if (matchedPatterns[matcherIndex].IsMatch(lineBufferToMatch[lineNumber]))
                        {
                            matchedIndex = matcherIndex;

                            string[] result = lineBufferToMatch.GetRange(0, lineNumber + 1).ToArray();

                            lineBufferToMatch.RemoveRange(0, lineNumber + 1);


                            return (String.Join("\n", result));
                        }
                    }
                }
            }

            if (matchMode == LineMatchModes.Partial || matchMode == LineMatchModes.Everything)
            {
                for (int matcherIndex = 0; matcherIndex < matchedPatterns.Length; matcherIndex++)
                {
                    if (matchedPatterns[matcherIndex].IsMatch(currentLine.ToString()))
                    {
                        matchedIndex = matcherIndex;

                        StringBuilder results = new StringBuilder();
                        results.AppendLine(String.Join("\n", lineBufferToMatch.ToArray()));

                        results.Append(currentLine.ToString());

                        this.Clear();

                        return (results.ToString());
                    }
                }
            }

            matchedIndex = 0;
            return (null);
        }
    }
}

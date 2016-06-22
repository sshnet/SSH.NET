using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Renci.SshNet.Tests.Common
{
    public class HttpProxyStub : IDisposable
    {
        private readonly IPEndPoint _endPoint;
        private AsyncSocketListener _listener;
        private HttpRequestParser _httpRequestParser;
        private readonly IList<byte[]> _responses;

        public HttpProxyStub(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
            _responses = new List<byte[]>();
        }

        public HttpRequest HttpRequest
        {
            get
            {
                if (_httpRequestParser == null)
                    throw new InvalidOperationException("The proxy is not started.");
                return _httpRequestParser.HttpRequest;
            }
        }

        public IList<byte[]> Responses
        {
            get { return _responses; }
        }

        public void Start()
        {
            _httpRequestParser = new HttpRequestParser();

            _listener = new AsyncSocketListener(_endPoint);
            _listener.BytesReceived += OnBytesReceived;
            _listener.Start();
        }

        public void Stop()
        {
            if (_listener != null)
                _listener.Stop();
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        private void OnBytesReceived(byte[] bytesReceived, Socket socket)
        {
            _httpRequestParser.ProcessData(bytesReceived);

            if (_httpRequestParser.CurrentState == HttpRequestParser.State.Content)
            {
                foreach (var response in Responses)
                    socket.Send(response);
                socket.Shutdown(SocketShutdown.Send);
            }
        }

        private class HttpRequestParser
        {
            private readonly List<byte> _buffer;
            private readonly HttpRequest _httpRequest;

            public enum State
            {
                RequestLine,
                Headers,
                Content
            }

            public HttpRequestParser()
            {
                CurrentState = State.RequestLine;
                _buffer = new List<byte>();
                _httpRequest = new HttpRequest();
            }

            public HttpRequest HttpRequest
            {
                get { return _httpRequest; }
            }

            public State CurrentState { get; private set; }

            public void ProcessData(byte[] data)
            {
                var position = 0;

                while (position != data.Length)
                {
                    if (CurrentState == State.RequestLine)
                    {
                        var requestLine = ReadLine(data, ref position);
                        if (requestLine != null)
                        {
                            _httpRequest.RequestLine = requestLine;
                            CurrentState = State.Headers;
                        }
                    }

                    if (CurrentState == State.Headers)
                    {
                        var line = ReadLine(data, ref position);
                        if (line != null)
                        {
                            if (line.Length == 0)
                            {
                                CurrentState = State.Content;
                            }
                            else
                            {
                                _httpRequest.Headers.Add(line);
                            }
                        }
                    }

                    if (CurrentState == State.Content)
                    {
                        if (position < data.Length)
                        {
                            var currentContent = _httpRequest.MessageBody;
                            var newBufferSize = currentContent.Length + (data.Length - position);
                            var copyBuffer = new byte[newBufferSize];
                            Array.Copy(currentContent, copyBuffer, currentContent.Length);
                            Array.Copy(data, position, copyBuffer, currentContent.Length, data.Length - position);
                            _httpRequest.MessageBody = copyBuffer;
                            break;
                        }
                    }
                }
            }

            private string ReadLine(byte[] data, ref int position)
            {
                for (; position < data.Length; position++)
                {
                    var b = data[position];
                    if (b == '\n')
                    {
                        var buffer = _buffer.ToArray();
                        var bytesInLine = buffer.Length;
                        // when the previous byte was a CR, then do not include it in line
                        if (buffer.Length > 0 && buffer[buffer.Length - 1] == '\r')
                            bytesInLine -= 1;
                        // clear the buffer
                        _buffer.Clear();
                        // move position up one position as we've processed the current byte
                        position++;
                        return Encoding.ASCII.GetString(buffer, 0, bytesInLine);
                    }
                    _buffer.Add(b);
                }

                return null;
            }
        }
    }
}
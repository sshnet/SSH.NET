namespace Renci.SshNet.IntegrationTests.Issue67
{
    class UnblockStreamReaderLockObject
    {
        public char[] buffer;
        public int len;
        public int pos;
    };

    public class UnblockStreamReader
    {
        const int BUFFER_SIZE = 65536;
        readonly Thread _readThread;
        private readonly UnblockStreamReaderLockObject _lockObject;
        public int GetUnreadBufferLength()
        {
            return _lockObject.len;
        }
        public int GetUnreadBufferPosition()
        {
            return _lockObject.pos;
        }
        readonly StreamReader _streamReader;

        public UnblockStreamReader(StreamReader streamReader)
        {
            _lockObject = new UnblockStreamReaderLockObject
            {
                buffer = new char[BUFFER_SIZE + 1],
                len = 0,
                pos = 0
            };

            _streamReader = streamReader;
            _readThread = new Thread(ReadThreadProc)
            {
                Name = "UnblockStreamReader thread"
            };
            _readThread.Start();
        }

        public void Close()
        {
            _readThread.Interrupt();
            lock (_lockObject)
            {
                _lockObject.len = 0;
                _lockObject.pos = 0;
            }
        }

        private void ReadThreadProc(object param)
        {
            char[] buf = new char[1];
            int readLen = 0;
            bool isSleep = false;
            try
            {
                while (true)
                {
                    lock (_lockObject)
                    {
                        if (_lockObject.len >= BUFFER_SIZE)
                        {
                            isSleep = true;
                        }
                    }
                    if (isSleep)
                    {
                        isSleep = false;
                        Thread.Sleep(10);
                        continue;
                    }
                    readLen = _streamReader.Read(buf, 0, 1);
                    if (readLen > 0)
                    {
                        lock (_lockObject)
                        {
                            if ((_lockObject.pos + _lockObject.len) >= BUFFER_SIZE)
                            {
                                for (int i = 0; i < _lockObject.len; i++)
                                {
                                    _lockObject.buffer[i] = _lockObject.buffer[_lockObject.pos + i];
                                }
                                _lockObject.pos = 0;
                            }

                            _lockObject.buffer[_lockObject.pos + _lockObject.len] = buf[0];

                            _lockObject.len++;
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception e)
            {
                e.ToString();
                return;
            }
        }

        public int ReadChar(ref char buf)
        {
            lock (_lockObject)
            {
                if (_lockObject.len == 0)
                {
                    return 0;
                }
                buf = _lockObject.buffer[_lockObject.pos];
                _lockObject.pos++;
                _lockObject.len--;
            }
            return 1;
        }

        public string ReadToEnd(bool isRemove = true)
        {
            string resultString;
            lock (_lockObject)
            {
                if (_lockObject.len == 0)
                {
                    return null;
                }
                resultString = new string(_lockObject.buffer, _lockObject.pos, _lockObject.len);
                if (isRemove)
                {
                    _lockObject.pos = 0;
                    _lockObject.len = 0;
                }
                return resultString;
            }
        }

        public string ReadLine(char lineEndFlag = '\n')
        {
            string resultString;
            while (true)
            {
                lock (_lockObject)
                {
                    if (_lockObject.len == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    for (int i = 0; i < _lockObject.len; i++)
                    {
                        if (_lockObject.buffer[_lockObject.pos + i] == lineEndFlag)
                        {
                            resultString = new string(_lockObject.buffer, _lockObject.pos, i + 1);
                            _lockObject.pos = _lockObject.pos + i + 1;
                            _lockObject.len = _lockObject.len - i - 1;
                            return resultString;
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }
    }
}

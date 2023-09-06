#if NETFRAMEWORK

using System.IO;

namespace SshNetTests.Issue67
{
    internal class SharpSshStream : ISshStream
    {
        Tamir.SharpSsh.SshStream _sshStream;

        public void Connect(string host, string userName, string password)
        {
            _sshStream = new Tamir.SharpSsh.SshStream(host, userName, password);
        }

        public void Close()
        {
            if (_sshStream != null)
                _sshStream.Close();
        }

        public void Write(string data)
        {
            StreamWriter streamWriter = GetStreamWriter();
            streamWriter.Write(data + "\r");
            streamWriter.Flush();
        }

        StreamReader _streamReader = null;
        public StreamReader GetStreamReader()
        {
            if (_streamReader != null)
            {
                return _streamReader;
            }
            else
            {
                if (_sshStream == null)
                {
                    return null;
                }
                _streamReader = new StreamReader(_sshStream);
                return _streamReader;
            }
        }

        StreamWriter _streamWriter = null;
        public StreamWriter GetStreamWriter()
        {
            if (_streamWriter != null)
            {
                return _streamWriter;
            }
            else
            {
                if (_sshStream == null)
                {
                    return null;
                }

                _streamWriter = new StreamWriter(_sshStream);
                return _streamWriter;
            }
        }
    }
}

#endif // NETFRAMEWORK
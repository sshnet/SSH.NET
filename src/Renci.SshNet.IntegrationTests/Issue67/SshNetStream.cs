namespace Renci.SshNet.IntegrationTests.Issue67
{
    internal class SshNetStream : ISshStream
    {
        Renci.SshNet.ShellStream _shellStream;
        Renci.SshNet.SshClient _sshClinet;

        public void Connect(string host, string userName, string password)
        {
            _sshClinet = new Renci.SshNet.SshClient(host, userName, password);
            _sshClinet.Connect();
            _shellStream = _sshClinet.CreateShellStream("ShellStream", 512, 24, 512, 512, 1024);
        }

        public void Close()
        {
            _sshClinet?.Disconnect();
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
                if (_shellStream == null)
                {
                    return null;
                }
                _streamReader = new StreamReader(_shellStream);
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
                if (_shellStream == null)
                {
                    return null;
                }
                _streamWriter = new StreamWriter(_shellStream);
                return _streamWriter;
            }
        }
    }
}

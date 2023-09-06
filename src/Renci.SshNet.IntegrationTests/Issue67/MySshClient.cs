using System.ComponentModel;

namespace Renci.SshNet.IntegrationTests.Issue67
{
    public class MySshClient : IDisposable
    {
        public MySshClient(string host, string userName, string password, string sshStreamType)
        {
            _host = host;
            _userName = userName;
            _password = password;
            _sshStreamType = sshStreamType;
        }

        private readonly string _host;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _sshStreamType;
        private readonly int _noResponseTimeoutSeconds = 60;

        private readonly Component _component = new Component();
        private bool _disposed = false;

        ~MySshClient()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _component.Dispose();
                }

                Close();

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private ISshStream SshStream
        {
            get;
            set;
        }

        public void Connect()
        {
            SshStream = SshStreamFactory.CreateSshStream(_sshStreamType);
            SshStream.Connect(_host, _userName, _password);

            try
            {
                UnblockStreamReader = new UnblockStreamReader(SshStream.GetStreamReader());
                InitEnv();
            }
            catch (Exception ex)
            {
                Close();
                throw ex;
            }
        }

        protected virtual void InitEnv()
        {
            SshStream.Write("set +o vi");
            SshStream.Write("set +o viraw");
            SshStream.Write("export PROMPT_COMMAND=");
            SshStream.Write("export PS1=" + Prompt);
            var response = ReadResponse("export PS1=" + Prompt + "\r\n", _noResponseTimeoutSeconds);
            response = ReadResponse(Prompt, _noResponseTimeoutSeconds);

            string helloMessage = "Hello this is test message!";
            SshStream.Write("echo '" + helloMessage + "'");
            response = ReadResponse(helloMessage + "\r\n", _noResponseTimeoutSeconds);
            response = ReadResponse(Prompt, _noResponseTimeoutSeconds);

            SshStream.Write("stty columns 512");
            response = ReadResponse(Prompt, _noResponseTimeoutSeconds);

            SshStream.Write("stty rows 24");
            response = ReadResponse(Prompt, _noResponseTimeoutSeconds);

            SshStream.Write("export LANG=en_US.UTF-8");
            response = ReadResponse(Prompt, _noResponseTimeoutSeconds);

            SshStream.Write("export NLS_LANG=American_America.ZHS16GBK");
            response = ReadResponse(Prompt, _noResponseTimeoutSeconds);

            SshStream.Write("unalias grep");
            response = ReadResponse(Prompt, _noResponseTimeoutSeconds);
        }

        protected virtual void Close()
        {
            if (UnblockStreamReader != null)
            {
                UnblockStreamReader.Close();
                UnblockStreamReader = null;
            }
            if (SshStream != null)
            {
                SshStream.Close();
                SshStream = null;
            }
        }

        protected UnblockStreamReader UnblockStreamReader
        {
            get;
            private set;
        }

        public string Prompt
        {
            get
            {
                return "[SHINE_COMMAND_PROMPT]";
            }
        }

        public void Write(string data)
        {
            if (SshStream == null)
            {
                Connect();
            }
            if (UnblockStreamReader.GetUnreadBufferLength() > 0)
            {
                UnblockStreamReader.ReadToEnd();
            }
            SshStream.Write(data);
        }

        public string[] ReadResponse(string prompt, int noResponseTimeoutSeconds)
        {
            List<UntilInfo> untilInfoList = new List<UntilInfo>() { new UntilInfo(prompt) };
            string[] response = UnblockStreamUtility.ReadUntil(UnblockStreamReader, untilInfoList, noResponseTimeoutSeconds);
            return response;
        }

        public string[] ReadResponse(List<UntilInfo> untilInfoList, int noResponseTimeoutSeconds)
        {
            string[] response = UnblockStreamUtility.ReadUntil(UnblockStreamReader, untilInfoList, noResponseTimeoutSeconds);
            return response;
        }

        public string[] RunCommand(string command)
        {
            SshStream.Write(command);
            return ReadResponse(Prompt, _noResponseTimeoutSeconds);
        }
    }
}

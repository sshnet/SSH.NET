namespace Renci.SshClient
{
    public class Connection
    {
        private Session _session;

        public ConnectionInfo ConnectionInfo { get; private set; }

        private Shell _shell;
        public Shell Shell
        {
            get
            {
                if (this._shell == null)
                {
                    this._shell = new Shell(this._session);
                }
                return this._shell;
            }
        }

        private Sftp _sftp;

        public Sftp Sftp
        {
            get
            {
                if (this._sftp == null)
                {
                    this._sftp = new Sftp(this._session);
                }
                return this._sftp;
            }
        }

        public Connection(ConnectionInfo connectionInfo)
        {
            this._session = Session.CreateSession(connectionInfo);
        }

        public void Connect()
        {
            this._session.Connect();
        }

        public void Disconnect()
        {
            this._session.Disconnect();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Examples
{
    class SshClientExamples
    {
        private void Example()
        {
            #region CreateSshClientWithUsernamePassword
            using (var client = new SshClient("host", "username", "password"))
            {
                client.Connect();
                //  Do something here
                client.Disconnect();
            }
            #endregion

            #region ConnectUsingPrivateKey
            using (var client = new SshClient("host", "username", new PrivateKeyFile(File.OpenRead(@"private.key"))))
            {
                client.Connect();
                client.Disconnect();
            }
            #endregion

            #region ConnectUsingPrivateKeyAndPassphrase
            using (var client = new SshClient("host", "username", new PrivateKeyFile(File.OpenRead(@"private.key"), "passphrase")))
            {
                client.Connect();
                client.Disconnect();
            }
            #endregion

        }
    }
}

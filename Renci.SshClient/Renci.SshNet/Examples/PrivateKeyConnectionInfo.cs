using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Examples
{
    class PrivateKeyConnectionInfoExample
    {
        private void Example()
        {
            #region ConnectUsingPrivateKeyConnectionInfo
            var connectionInfo = new PrivateKeyConnectionInfo("host", 1234, "username", new PrivateKeyFile(File.OpenRead(@"rsa_pass_key.txt"), "tester"));
            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
            #endregion
        }
    }
}

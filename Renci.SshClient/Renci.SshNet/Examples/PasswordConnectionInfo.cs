using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Examples
{
    class PasswordConnectionInfoExamples
    {
        private void Example()
        {
            #region ConnectUsingPasswordConnectionInfo
            var connectionInfo = new PasswordConnectionInfo("host", "username", "password");
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                //  Do something here
                client.Disconnect();
            }
            #endregion
        }

        private void Example1()
        {
            #region ChangePasswordWhenConnecting
            var connectionInfo = new PasswordConnectionInfo("host", "username", "password");
            var encoding = new Renci.SshNet.Common.ASCIIEncoding();
            connectionInfo.PasswordExpired += delegate(object sender, AuthenticationPasswordChangeEventArgs e)
            {
                e.NewPassword = encoding.GetBytes("123456");
            };

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();

                client.Disconnect();
            }
            #endregion

        }
    }
}

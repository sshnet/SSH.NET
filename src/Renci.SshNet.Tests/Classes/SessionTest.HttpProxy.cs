using System;
using System.Net;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Connection;

namespace Renci.SshNet.Tests.Classes
{
    public partial class SessionTest
    {
        private static ConnectionInfo CreateConnectionInfoWithHttpProxy(IPEndPoint proxyEndPoint, IPEndPoint serverEndPoint, string proxyUserName)
        {
            return new ConnectionInfo(
                serverEndPoint.Address.ToString(),
                serverEndPoint.Port,
                "eric",
                ProxyTypes.Http,
                proxyEndPoint.Address.ToString(),
                proxyEndPoint.Port,
                proxyUserName,
                "proxypwd",
                new NoneAuthenticationMethod("eric"));
        }

        private static string CreateProxyAuthorizationHeader(ConnectionInfo connectionInfo)
        {
            return string.Format("Proxy-Authorization: Basic {0}",
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(string.Format("{0}:{1}", connectionInfo.ProxyUsername,
                        connectionInfo.ProxyPassword))));
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Represents remote connection information class.
    /// </summary>
    [TestClass]
    public class ConnectionInfoTest : TestBase
    {
        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass null as proxy host.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_ProxyHost_Null()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.Http, null, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too large proxy port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_ProxyPort_Large()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.Http, Resources.HOST, int.MaxValue, Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too small proxy port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_ProxyPort_Small()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.Http, Resources.HOST, int.MinValue, Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass a valid proxy port.")]
        [Owner("Kenneth_aa")]
        public void Test_ConnectionInfo_ProxyPort_Valid()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass null as host.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Host_Null()
        {
            new ConnectionInfo(null, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass a valid host.")]
        [Owner("Kenneth_aa")]
        public void Test_ConnectionInfo_Host_Valid()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too large port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_Port_Large()
        {
            new ConnectionInfo(Resources.HOST, int.MaxValue, Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too small port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_Port_Small()
        {
            new ConnectionInfo(Resources.HOST, int.MinValue, Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass a valid port.")]
        [Owner("Kenneth_aa")]
        public void Test_ConnectionInfo_Port_Valid()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass null as session.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_ConnectionInfo_Authenticate_Null()
        {
            var ret = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
            ret.Authenticate(null);
        }
    }
}
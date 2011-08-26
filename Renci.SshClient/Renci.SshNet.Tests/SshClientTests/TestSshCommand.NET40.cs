using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;
using System.IO;

namespace Renci.SshNet.Tests.SshClientTests
{
	public partial class TestSshCommand
	{

		//[TestMethod]
		public void Test_MultipleThread_10000_MultipleConnections()
		{
			try
			{
				System.Threading.Tasks.Parallel.For(0, 10000,
					() =>
					{
						var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD);
						client.Connect();
						return client;
					},
					(int counter, ParallelLoopState pls, SshClient client) =>
					{
						var result = ExecuteTestCommand(client);
						Debug.WriteLine(string.Format("TestMultipleThreadMultipleConnections #{0}", counter));
						Assert.IsTrue(result);
						return client;
					},
					(SshClient client) =>
					{
						client.Disconnect();
						client.Dispose();
					}
				);
			}
			catch (Exception exp)
			{
				Assert.Fail(exp.ToString());
			}
		}

		//[TestMethod]
		public void Test_MultipleThread_10000_MultipleSessions()
		{
			using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
			{
				client.Connect();
				System.Threading.Tasks.Parallel.For(0, 10000,
					(counter) =>
					{
						var result = ExecuteTestCommand(client);
						Debug.WriteLine(string.Format("TestMultipleThreadMultipleConnections #{0}", counter));
						Assert.IsTrue(result);
					}
				);

				client.Disconnect();
			}
		}

	}
}

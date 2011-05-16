using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;
using System.Diagnostics;

namespace Renci.SshNet.Tests.SftpClientTests
{
	/// <summary>
	/// Summary description for DeleteFileTest
	/// </summary>
	[TestClass]
	public class DeleteFileTest
	{
		[TestInitialize()]
		public void CleanCurrentFolder()
		{
			using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
			{
				client.Connect();
				client.RunCommand("rm -rf *");
				client.Disconnect();
			}
		}

		[TestMethod]
		[TestCategory("Sftp")]
		[Description("Test passing null to DeleteFile.")]
		[ExpectedException(typeof(ArgumentException))]
		public void Test_Sftp_DeleteFile_Null()
		{
			using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
			{
				sftp.DeleteFile(null);
			}
		}
	}
}

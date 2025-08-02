using System.Diagnostics;

using Renci.SshNet.Common;
using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.IntegrationTests
{
    // TODO: DeleteDirectory (fail + success
    // TODO: DeleteFile (fail + success
    // TODO: Delete (fail + success

    [TestClass]
    public class SftpTests : TestBase
    {
        private IConnectionInfoFactory _connectionInfoFactory;
        private IConnectionInfoFactory _adminConnectionInfoFactory;
        private IRemotePathTransformation _remotePathTransformation;

        [TestInitialize]
        public void SetUp()
        {
            _connectionInfoFactory = new LinuxVMConnectionFactory(SshServerHostName, SshServerPort);
            _adminConnectionInfoFactory = new LinuxAdminConnectionFactory(SshServerHostName, SshServerPort);
            _remotePathTransformation = RemotePathTransformation.ShellQuote;
        }

        [TestMethod]
        [DynamicData(nameof(GetSftpUploadFileFileStreamData), DynamicDataSourceType.Method)]
        public void Sftp_UploadFile_FileStream(int size)
        {
            var file = CreateTempFile(size);

            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.Delete(remoteFile);
                }

                try
                {
                    client.UploadFile(fs, remoteFile);

                    using (var memoryStream = new MemoryStream(size))
                    {
                        DownloadFileRandomMethod(client, remoteFile, memoryStream);

                        memoryStream.Position = 0;
                        Assert.AreEqual(CreateFileHash(file), CreateHash(memoryStream));
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.Delete(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ConnectDisconnect_Serial()
        {
            const int iterations = 100;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                for (var i = 1; i <= iterations; i++)
                {
                    client.Connect();
                    client.Disconnect();
                }
            }
        }

        [TestMethod]
        public void Sftp_ConnectDisconnect_Parallel()
        {
            const int iterations = 10;
            const int threads = 5;

            var startEvent = new ManualResetEvent(false);

            var tasks = Enumerable.Range(1, threads).Select(i =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        using (var client = new SftpClient(_connectionInfoFactory.Create()))
                        {
                            startEvent.WaitOne();

                            for (var j = 0; j < iterations; j++)
                            {
                                client.Connect();
                                client.Disconnect();
                            }
                        }
                    });
                }).ToArray();

            startEvent.Set();

            Task.WaitAll(tasks);
        }

        [TestMethod]
        public void Sftp_BeginUploadFile()
        {
            const string content = "SftpBeginUploadFile";

            var expectedByteCount = (ulong)Encoding.ASCII.GetByteCount(content);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.Delete(remoteFile);
                }

                try
                {
                    using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(content)))
                    {
                        IAsyncResult asyncResultCallback = null;
                        using var callbackCalled = new ManualResetEventSlim(false);

                        var asyncResult = client.BeginUploadFile(memoryStream, remoteFile, ar =>
                        {
                            asyncResultCallback = ar;
                            callbackCalled.Set();
                        });

                        Assert.IsTrue(asyncResult.AsyncWaitHandle.WaitOne(10000));

                        // check async result
                        var sftpUploadAsyncResult = asyncResult as SftpUploadAsyncResult;
                        Assert.IsNotNull(sftpUploadAsyncResult);
                        Assert.IsFalse(sftpUploadAsyncResult.IsUploadCanceled);
                        Assert.IsTrue(sftpUploadAsyncResult.IsCompleted);
                        Assert.IsFalse(sftpUploadAsyncResult.CompletedSynchronously);
                        Assert.AreEqual(expectedByteCount, sftpUploadAsyncResult.UploadedBytes);

                        Assert.IsTrue(callbackCalled.Wait(10000));

                        // check async result callback
                        var sftpUploadAsyncResultCallback = asyncResultCallback as SftpUploadAsyncResult;
                        Assert.IsNotNull(sftpUploadAsyncResultCallback);
                        Assert.IsFalse(sftpUploadAsyncResultCallback.IsUploadCanceled);
                        Assert.IsTrue(sftpUploadAsyncResultCallback.IsCompleted);
                        Assert.IsFalse(sftpUploadAsyncResultCallback.CompletedSynchronously);
                        Assert.AreEqual(expectedByteCount, sftpUploadAsyncResultCallback.UploadedBytes);
                    }

                    // check uploaded file
                    using (var memoryStream = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, memoryStream);
                        memoryStream.Position = 0;
                        var remoteContent = Encoding.ASCII.GetString(memoryStream.ToArray());
                        Assert.AreEqual(content, remoteContent);
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.Delete(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Create_ExistingFile()
        {
            var encoding = Encoding.UTF8;
            var initialContent = "Gert & Ann & Lisa";
            var newContent1 = "Sofie";
            var newContent1Bytes = GetBytesWithPreamble(newContent1, encoding);
            var newContent2 = "Lisa & Sofie";
            var newContent2Bytes = GetBytesWithPreamble(newContent2, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent);

                    #region Write less bytes than the current content, overwriting part of that content

                    using (var fs = client.Create(remoteFile))
                    using (var sw = new StreamWriter(fs, encoding))
                    {
                        sw.Write(newContent1);
                    }

                    var actualContent1 = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(newContent1Bytes, actualContent1);

                    #endregion Write less bytes than the current content, overwriting part of that content

                    #region Write more bytes than the current content, overwriting and appending to that content

                    using (var fs = client.Create(remoteFile))
                    using (var sw = new StreamWriter(fs, encoding))
                    {
                        sw.Write(newContent2);
                    }

                    var actualContent2 = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(newContent2Bytes, actualContent2);

                    #endregion Write more bytes than the current content, overwriting and appending to that content
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Create_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                SftpFileStream fs = null;

                try
                {
                    fs = client.Create(remoteFile);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    fs?.Dispose();

                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Create_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.BufferSize = 512 * 1024;
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var imageStream = GetData("resources.issue #70.png"))
                    {
                        using (var fs = client.Create(remoteFile))
                        {
                            imageStream.CopyTo(fs);
                        }

                        using (var memoryStream = new MemoryStream())
                        {
                            DownloadFileRandomMethod(client, remoteFile, memoryStream);

                            memoryStream.Position = 0;
                            imageStream.Position = 0;

                            Assert.AreEqual(CreateHash(imageStream), CreateHash(memoryStream));
                        }
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllLines_NoEncoding_ExistingFile()
        {
            var initialContent = "\u0100ert & Ann";
            IEnumerable<string> linesToAppend = new[] { "Forever", "&", "\u0116ver" };
            var expectedContent = initialContent + string.Join(Environment.NewLine, linesToAppend) +
                                  Environment.NewLine;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent);
                    client.AppendAllLines(remoteFile, linesToAppend);

                    var text = client.ReadAllText(remoteFile);
                    Assert.AreEqual(expectedContent, text);

                    // ensure we didn't write an UTF-8 BOM
                    using (var fs = client.OpenRead(remoteFile))
                    {
                        var firstByte = fs.ReadByte();
                        Assert.AreEqual(Encoding.UTF8.GetBytes(expectedContent)[0], firstByte);
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllLines_NoEncoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            IEnumerable<string> linesToAppend = new[] { "\u0139isa", "&", "Sofie" };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.AppendAllLines(remoteFile, linesToAppend);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllLines_NoEncoding_FileDoesNotExist()
        {
            IEnumerable<string> linesToAppend = new[] { "\u0139isa", "&", "Sofie" };
            var expectedContent = string.Join(Environment.NewLine, linesToAppend) + Environment.NewLine;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.AppendAllLines(remoteFile, linesToAppend);

                    var text = client.ReadAllText(remoteFile);
                    Assert.AreEqual(expectedContent, text);

                    // ensure we didn't write an UTF-8 BOM
                    using (var fs = client.OpenRead(remoteFile))
                    {
                        var firstByte = fs.ReadByte();
                        Assert.AreEqual(Encoding.UTF8.GetBytes(expectedContent)[0], firstByte);
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllText_NoEncoding_ExistingFile()
        {
            var initialContent = "\u0100ert & Ann";
            var contentToAppend = "Forever&\u0116ver";
            var expectedContent = initialContent + contentToAppend;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent);
                    client.AppendAllText(remoteFile, contentToAppend);

                    var text = client.ReadAllText(remoteFile);
                    Assert.AreEqual(expectedContent, text);

                    // ensure we didn't write an UTF-8 BOM
                    using (var fs = client.OpenRead(remoteFile))
                    {
                        var firstByte = fs.ReadByte();
                        Assert.AreEqual(Encoding.UTF8.GetBytes(expectedContent)[0], firstByte);
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllText_NoEncoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            var contentToAppend = "Forever&\u0116ver";

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.AppendAllText(remoteFile, contentToAppend);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllText_NoEncoding_FileDoesNotExist()
        {
            var contentToAppend = "Forever&\u0116ver";
            var expectedContent = contentToAppend;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.AppendAllText(remoteFile, contentToAppend);

                    var text = client.ReadAllText(remoteFile);
                    Assert.AreEqual(expectedContent, text);

                    // ensure we didn't write an UTF-8 BOM
                    using (var fs = client.OpenRead(remoteFile))
                    {
                        var firstByte = fs.ReadByte();
                        Assert.AreEqual(Encoding.UTF8.GetBytes(expectedContent)[0], firstByte);
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendText_NoEncoding_ExistingFile()
        {
            var initialContent = "\u0100ert & Ann";
            var contentToAppend = "Forever&\u0116ver";
            var expectedContent = initialContent + contentToAppend;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent);

                    using (var sw = client.AppendText(remoteFile))
                    {
                        sw.Write(contentToAppend);
                    }

                    var text = client.ReadAllText(remoteFile);
                    Assert.AreEqual(expectedContent, text);

                    // ensure we didn't write an UTF-8 BOM
                    using (var fs = client.OpenRead(remoteFile))
                    {
                        var firstByte = fs.ReadByte();
                        Assert.AreEqual(Encoding.UTF8.GetBytes(expectedContent)[0], firstByte);
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendText_NoEncoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                StreamWriter sw = null;

                try
                {
                    sw = client.AppendText(remoteFile);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    sw?.Dispose();

                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendText_NoEncoding_FileDoesNotExist()
        {
            var contentToAppend = "\u0100ert & Ann";
            var expectedContent = contentToAppend;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var sw = client.AppendText(remoteFile))
                    {
                        sw.Write(contentToAppend);
                    }

                    // ensure we didn't write an UTF-8 BOM
                    using (var fs = client.OpenRead(remoteFile))
                    {
                        var firstByte = fs.ReadByte();
                        Assert.AreEqual(Encoding.UTF8.GetBytes(expectedContent)[0], firstByte);
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllLines_Encoding_ExistingFile()
        {
            var initialContent = "\u0100ert & Ann";
            IEnumerable<string> linesToAppend = new[] { "Forever", "&", "\u0116ver" };
            var expectedContent = initialContent + string.Join(Environment.NewLine, linesToAppend) +
                                  Environment.NewLine;
            var encoding = GetRandomEncoding();
            var expectedBytes = GetBytesWithPreamble(expectedContent, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent, encoding);
                    client.AppendAllLines(remoteFile, linesToAppend, encoding);

                    var text = client.ReadAllText(remoteFile, encoding);
                    Assert.AreEqual(expectedContent, text);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllLines_Encoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            IEnumerable<string> linesToAppend = new[] { "Forever", "&", "\u0116ver" };
            var encoding = GetRandomEncoding();

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.AppendAllLines(remoteFile, linesToAppend, encoding);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }

                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllLines_Encoding_FileDoesNotExist()
        {
            IEnumerable<string> linesToAppend = new[] { "\u0139isa", "&", "Sofie" };
            var expectedContent = string.Join(Environment.NewLine, linesToAppend) + Environment.NewLine;
            var encoding = GetRandomEncoding();
            var expectedBytes = GetBytesWithPreamble(expectedContent, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.AppendAllLines(remoteFile, linesToAppend, encoding);

                    var text = client.ReadAllText(remoteFile, encoding);
                    Assert.AreEqual(expectedContent, text);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllText_Encoding_ExistingFile()
        {
            var initialContent = "\u0100ert & Ann";
            var contentToAppend = "Forever&\u0116ver";
            var expectedContent = initialContent + contentToAppend;
            var encoding = GetRandomEncoding();
            var expectedBytes = GetBytesWithPreamble(expectedContent, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent, encoding);
                    client.AppendAllText(remoteFile, contentToAppend, encoding);

                    var text = client.ReadAllText(remoteFile, encoding);
                    Assert.AreEqual(expectedContent, text);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllText_Encoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            const string contentToAppend = "Forever&\u0116ver";
            var encoding = GetRandomEncoding();

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.AppendAllText(remoteFile, contentToAppend, encoding);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendAllText_Encoding_FileDoesNotExist()
        {
            const string contentToAppend = "Forever&\u0116ver";
            var expectedContent = contentToAppend;
            var encoding = GetRandomEncoding();
            var expectedBytes = GetBytesWithPreamble(expectedContent, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.AppendAllText(remoteFile, contentToAppend, encoding);

                    var text = client.ReadAllText(remoteFile, encoding);
                    Assert.AreEqual(expectedContent, text);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendText_Encoding_ExistingFile()
        {
            const string initialContent = "\u0100ert & Ann";
            const string contentToAppend = "Forever&\u0116ver";
            var expectedContent = initialContent + contentToAppend;
            var encoding = GetRandomEncoding();
            var expectedBytes = GetBytesWithPreamble(expectedContent, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent, encoding);

                    using (var sw = client.AppendText(remoteFile, encoding))
                    {
                        sw.Write(contentToAppend);
                    }

                    var text = client.ReadAllText(remoteFile, encoding);
                    Assert.AreEqual(expectedContent, text);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendText_Encoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            var encoding = GetRandomEncoding();

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                StreamWriter sw = null;

                try
                {
                    sw = client.AppendText(remoteFile, encoding);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    sw?.Dispose();

                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_AppendText_Encoding_FileDoesNotExist()
        {
            var encoding = GetRandomEncoding();
            const string contentToAppend = "\u0100ert & Ann";
            var expectedBytes = GetBytesWithPreamble(contentToAppend, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var sw = client.AppendText(remoteFile, encoding))
                    {
                        sw.Write(contentToAppend);
                    }

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_CreateText_NoEncoding_ExistingFile()
        {
            var encoding = new UTF8Encoding(false, true);
            const string initialContent = "\u0100ert & Ann";
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);
            const string newContent = "\u0116ver";
            const string expectedContent = "\u0116ver" + " & Ann";
            var expectedContentBytes = GetBytesWithPreamble(expectedContent, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent);

                    using (client.CreateText(remoteFile))
                    {
                    }

                    // verify that original content is left untouched
                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(initialContentBytes, actualBytes);

                    // write content that is less bytes than original content
                    using (var sw = client.CreateText(remoteFile))
                    {
                        sw.Write(newContent);
                    }

                    // verify that original content is only partially overwritten
                    var text = client.ReadAllText(remoteFile);
                    Assert.AreEqual(expectedContent, text);

                    actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedContentBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_CreateText_NoEncoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                StreamWriter sw = null;

                try
                {
                    sw = client.CreateText(remoteFile);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    sw?.Dispose();

                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_CreateText_NoEncoding_FileDoesNotExist()
        {
            var encoding = new UTF8Encoding(false, true);
            var initialContent = "\u0100ert & Ann";
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (client.CreateText(remoteFile))
                    {
                    }

                    // verify that empty file was created
                    Assert.IsTrue(client.Exists(remoteFile));

                    var file = client.GetAttributes(remoteFile);
                    Assert.AreEqual(0, file.Size);

                    client.DeleteFile(remoteFile);

                    using (var sw = client.CreateText(remoteFile))
                    {
                        sw.Write(initialContent);
                    }

                    // verify that content is written to file
                    var text = client.ReadAllText(remoteFile);
                    Assert.AreEqual(initialContent, text);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(initialContentBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_CreateText_Encoding_ExistingFile()
        {
            var encoding = GetRandomEncoding();
            var initialContent = "\u0100ert & Ann";
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);
            var newContent = "\u0116ver";
            var expectedContent = "\u0116ver" + " & Ann";
            var expectedContentBytes = GetBytesWithPreamble(expectedContent, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent, encoding);

                    using (client.CreateText(remoteFile))
                    {
                    }

                    // verify that original content is left untouched
                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(initialContentBytes, actualBytes);

                    // write content that is less bytes than original content
                    using (var sw = client.CreateText(remoteFile, encoding))
                    {
                        sw.Write(newContent);
                    }

                    // verify that original content is only partially overwritten
                    var text = client.ReadAllText(remoteFile, encoding);
                    Assert.AreEqual(expectedContent, text);

                    actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedContentBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_CreateText_Encoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            var encoding = GetRandomEncoding();

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                StreamWriter sw = null;

                try
                {
                    sw = client.CreateText(remoteFile, encoding);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    sw?.Dispose();

                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_CreateText_Encoding_FileDoesNotExist()
        {
            var encoding = GetRandomEncoding();
            var initialContent = "\u0100ert & Ann";
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (client.CreateText(remoteFile, encoding))
                    {
                    }

                    // verify that file containing only preamble was created
                    Assert.IsTrue(client.Exists(remoteFile));

                    var file = client.GetAttributes(remoteFile);
                    Assert.AreEqual(encoding.GetPreamble().Length, file.Size);

                    client.DeleteFile(remoteFile);

                    using (var sw = client.CreateText(remoteFile, encoding))
                    {
                        sw.Write(initialContent);
                    }

                    // verify that content is written to file
                    var text = client.ReadAllText(remoteFile, encoding);
                    Assert.AreEqual(initialContent, text);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(initialContentBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_DownloadFile_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                using (var ms = new MemoryStream())
                {
                    try
                    {
                        client.DownloadFile(remoteFile, ms);
                        Assert.Fail();
                    }
                    catch (SftpPathNotFoundException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("No such file", ex.Message);

                        // ensure file was not created by us
                        Assert.IsFalse(client.Exists(remoteFile));
                    }
                    finally
                    {
                        if (client.Exists(remoteFile))
                        {
                            client.DeleteFile(remoteFile);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ReadAllBytes_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.ReadAllBytes(remoteFile);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);

                    // ensure file was not created by us
                    Assert.IsFalse(client.Exists(remoteFile));
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ReadAllLines_NoEncoding_ExistingFile()
        {
            var encoding = new UTF8Encoding(false, true);
            var lines = new[] { "\u0100ert & Ann", "Forever", "&", "\u0116ver" };
            var linesBytes = GetBytesWithPreamble(string.Join(Environment.NewLine, lines) + Environment.NewLine,
                                                  encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var sw = client.AppendText(remoteFile))
                    {
                        for (var i = 0; i < lines.Length; i++)
                        {
                            sw.WriteLine(lines[i]);
                        }
                    }

                    var actualLines = client.ReadAllLines(remoteFile);
                    Assert.IsNotNull(actualLines);
                    CollectionAssert.AreEqual(lines, actualLines);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(linesBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ReadAllLines_NoEncoding_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.ReadAllLines(remoteFile);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);

                    // ensure file was not created by us
                    Assert.IsFalse(client.Exists(remoteFile));
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ReadAllLines_Encoding_ExistingFile()
        {
            var encoding = GetRandomEncoding();
            var lines = new[] { "\u0100ert & Ann", "Forever", "&", "\u0116ver" };
            var linesBytes = GetBytesWithPreamble(string.Join(Environment.NewLine, lines) + Environment.NewLine,
                                                  encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var sw = client.AppendText(remoteFile, encoding))
                    {
                        for (var i = 0; i < lines.Length; i++)
                        {
                            sw.WriteLine(lines[i]);
                        }
                    }

                    var actualLines = client.ReadAllLines(remoteFile, encoding);
                    Assert.IsNotNull(actualLines);
                    CollectionAssert.AreEqual(lines, actualLines);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(linesBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ReadAllLines_Encoding_FileDoesNotExist()
        {
            var encoding = GetRandomEncoding();

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.ReadAllLines(remoteFile, encoding);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);

                    // ensure file was not created by us
                    Assert.IsFalse(client.Exists(remoteFile));
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ReadAllText_NoEncoding_ExistingFile()
        {
            var encoding = new UTF8Encoding(false, true);
            var lines = new[] { "\u0100ert & Ann", "Forever", "&", "\u0116ver" };
            var expectedText = string.Join(Environment.NewLine, lines) + Environment.NewLine;
            var expectedBytes = GetBytesWithPreamble(expectedText, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var sw = client.AppendText(remoteFile))
                    {
                        for (var i = 0; i < lines.Length; i++)
                        {
                            sw.WriteLine(lines[i]);
                        }
                    }

                    var actualText = client.ReadAllText(remoteFile);
                    Assert.AreEqual(expectedText, actualText);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ReadAllText_NoEncoding_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.ReadAllText(remoteFile);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);

                    // ensure file was not created by us
                    Assert.IsFalse(client.Exists(remoteFile));
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ReadAllText_Encoding_ExistingFile()
        {
            var encoding = GetRandomEncoding();
            var lines = new[] { "\u0100ert & Ann", "Forever", "&", "\u0116ver" };
            var expectedText = string.Join(Environment.NewLine, lines) + Environment.NewLine;
            var expectedBytes = GetBytesWithPreamble(expectedText, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var sw = client.AppendText(remoteFile, encoding))
                    {
                        for (var i = 0; i < lines.Length; i++)
                        {
                            sw.WriteLine(lines[i]);
                        }
                    }

                    var actualText = client.ReadAllText(remoteFile, encoding);
                    Assert.AreEqual(expectedText, actualText);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ReadAllText_Encoding_FileDoesNotExist()
        {
            var encoding = GetRandomEncoding();

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.ReadAllText(remoteFile, encoding);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);

                    // ensure file was not created by us
                    Assert.IsFalse(client.Exists(remoteFile));
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ReadLines_NoEncoding_ExistingFile()
        {
            var lines = new[] { "\u0100ert & Ann", "Forever", "&", "\u0116ver" };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var sw = client.AppendText(remoteFile))
                    {
                        for (var i = 0; i < lines.Length; i++)
                        {
                            sw.WriteLine(lines[i]);
                        }
                    }

                    var actualLines = client.ReadLines(remoteFile);
                    Assert.IsNotNull(actualLines);

                    // These two lines together test double enumeration.
                    Assert.AreEqual(lines[0], actualLines.First());
                    CollectionAssert.AreEqual(lines, actualLines.ToArray());
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Sftp_ReadLines_FileDoesNotExist(bool encoding)
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                // This exception should bubble up immediately
                var nullEx = encoding
                    ? Assert.Throws<ArgumentNullException>(() => client.ReadLines(null, GetRandomEncoding()))
                    : Assert.Throws<ArgumentNullException>(() => client.ReadLines(null));

                Assert.AreEqual("path", nullEx.ParamName);

                try
                {
                    var ex = Assert.ThrowsExactly<SftpPathNotFoundException>(() =>
                    {
                        // The PathNotFound exception is permitted to bubble up only upon
                        // enumerating.
                        var lines = encoding
                            ? client.ReadLines(remoteFile, GetRandomEncoding())
                            : client.ReadLines(remoteFile);

                        using var enumerator = lines.GetEnumerator();

                        _ = enumerator.MoveNext();
                    });

                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);

                    // ensure file was not created by us
                    Assert.IsFalse(client.Exists(remoteFile));
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ReadLines_Encoding_ExistingFile()
        {
            var encoding = GetRandomEncoding();
            var lines = new[] { "\u0100ert & Ann", "Forever", "&", "\u0116ver" };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var sw = client.AppendText(remoteFile, encoding))
                    {
                        for (var i = 0; i < lines.Length; i++)
                        {
                            sw.WriteLine(lines[i]);
                        }
                    }

                    var actualLines = client.ReadLines(remoteFile, encoding);
                    Assert.IsNotNull(actualLines);

                    // These two lines together test double enumeration.
                    Assert.AreEqual(lines[0], actualLines.First());
                    CollectionAssert.AreEqual(lines, actualLines.ToArray());
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllBytes_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            var content = GenerateRandom(size: 5);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllBytes(remoteFile, content);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllBytes_ExistingFile()
        {
            var initialContent = GenerateRandom(size: 13);
            var newContent1 = GenerateRandom(size: 5);
            var expectedContent1 = new ArrayBuilder<byte>().Add(newContent1)
                                                           .Add(initialContent, newContent1.Length, initialContent.Length - newContent1.Length)
                                                           .Build();
            var newContent2 = GenerateRandom(size: 50000);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var fs = client.Create(remoteFile))
                    {
                        fs.Write(initialContent, offset: 0, initialContent.Length);
                    }

                    #region Write less bytes than the current content, overwriting part of that content

                    client.WriteAllBytes(remoteFile, newContent1);

                    var actualContent1 = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedContent1, actualContent1);

                    #endregion Write less bytes than the initial content, overwriting part of that content

                    #region Write more bytes than the current content, overwriting and appending to that content

                    client.WriteAllBytes(remoteFile, newContent2);

                    var actualContent2 = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(newContent2, actualContent2);

                    #endregion Write less bytes than the initial content, overwriting part of that content
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllBytes_FileDoesNotExist()
        {
            var content = GenerateRandom(size: 13);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllBytes(remoteFile, content);

                    var actualContent1 = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(content, actualContent1);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }


        [TestMethod]
        public void Sftp_WriteAllLines_IEnumerable_NoEncoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            IEnumerable<string> linesToWrite = new[] { "Forever", "&", "\u0116ver" };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllLines(remoteFile, linesToWrite);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllLines_IEnumerable_NoEncoding_ExistingFile()
        {
            var encoding = new UTF8Encoding(false, true);
            var initialContent = "\u0100ert & Ann Forever & Ever Lisa & Sofie";
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);
            IEnumerable<string> linesToWrite1 = new[] { "Forever", "&", "\u0116ver" };
            var linesToWrite1Bytes =
                GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite1) + Environment.NewLine, encoding);
            var expectedBytes1 = new ArrayBuilder<byte>().Add(linesToWrite1Bytes)
                                                         .Add(initialContentBytes,
                                                              linesToWrite1Bytes.Length,
                                                              initialContentBytes.Length - linesToWrite1Bytes.Length)
                                                         .Build();
            IEnumerable<string> linesToWrite2 = new[] { "Forever", "&", "\u0116ver", "Gert & Ann", "Lisa + Sofie" };
            var linesToWrite2Bytes =
                GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite2) + Environment.NewLine, encoding);
            var expectedBytes2 = linesToWrite2Bytes;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    // create initial content
                    client.WriteAllText(remoteFile, initialContent);

                    #region Write less bytes than the current content, overwriting part of that content

                    client.WriteAllLines(remoteFile, linesToWrite1);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes1, actualBytes);

                    #endregion Write less bytes than the current content, overwriting part of that content

                    #region Write more bytes than the current content, overwriting and appending to that content

                    client.WriteAllLines(remoteFile, linesToWrite2);

                    actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes2, actualBytes);

                    #endregion Write more bytes than the current content, overwriting and appending to that content
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllLines_IEnumerable_NoEncoding_FileDoesNotExist()
        {
            var encoding = new UTF8Encoding(false, true);
            IEnumerable<string> linesToWrite = new[] { "\u0139isa", "&", "Sofie" };
            var linesToWriteBytes = GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite) + Environment.NewLine, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllLines(remoteFile, linesToWrite);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(linesToWriteBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllLines_IEnumerable_Encoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            var encoding = GetRandomEncoding();
            IEnumerable<string> linesToWrite = new[] { "Forever", "&", "\u0116ver" };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllLines(remoteFile, linesToWrite, encoding);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllLines_IEnumerable_Encoding_ExistingFile()
        {
            var encoding = GetRandomEncoding();
            const string initialContent = "\u0100ert & Ann Forever & Ever Lisa & Sofie";
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);
            IEnumerable<string> linesToWrite1 = new[] { "Forever", "&", "\u0116ver" };
            var linesToWrite1Bytes =
                GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite1) + Environment.NewLine, encoding);
            var expectedBytes1 = new ArrayBuilder<byte>().Add(linesToWrite1Bytes)
                                                         .Add(initialContentBytes,
                                                              linesToWrite1Bytes.Length,
                                                              initialContentBytes.Length - linesToWrite1Bytes.Length)
                                                         .Build();
            IEnumerable<string> linesToWrite2 = new[] { "Forever", "&", "\u0116ver", "Gert & Ann", "Lisa + Sofie" };
            var linesToWrite2Bytes = GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite2) + Environment.NewLine, encoding);
            var expectedBytes2 = linesToWrite2Bytes;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    // create initial content
                    client.WriteAllText(remoteFile, initialContent, encoding);

                    #region Write less bytes than the current content, overwriting part of that content

                    client.WriteAllLines(remoteFile, linesToWrite1, encoding);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes1, actualBytes);

                    #endregion Write less bytes than the current content, overwriting part of that content

                    #region Write more bytes than the current content, overwriting and appending to that content

                    client.WriteAllLines(remoteFile, linesToWrite2, encoding);

                    actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes2, actualBytes);

                    #endregion Write more bytes than the current content, overwriting and appending to that content
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllLines_IEnumerable_Encoding_FileDoesNotExist()
        {
            var encoding = GetRandomEncoding();
            IEnumerable<string> linesToWrite = new[] { "\u0139isa", "&", "Sofie" };
            var linesToWriteBytes = GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite) + Environment.NewLine, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllLines(remoteFile, linesToWrite, encoding);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(linesToWriteBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllLines_Array_NoEncoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            var linesToWrite = new[] { "Forever", "&", "\u0116ver" };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllLines(remoteFile, linesToWrite);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllLines_Array_NoEncoding_ExistingFile()
        {
            var encoding = new UTF8Encoding(false, true);
            const string initialContent = "\u0100ert & Ann Forever & Ever Lisa & Sofie";
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);
            var linesToWrite1 = new[] { "Forever", "&", "\u0116ver" };
            var linesToWrite1Bytes = GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite1) + Environment.NewLine, encoding);
            var expectedBytes1 = new ArrayBuilder<byte>().Add(linesToWrite1Bytes)
                                                         .Add(initialContentBytes, linesToWrite1Bytes.Length, initialContentBytes.Length - linesToWrite1Bytes.Length)
                                                         .Build();
            var linesToWrite2 = new[] { "Forever", "&", "\u0116ver", "Gert & Ann", "Lisa + Sofie" };
            var linesToWrite2Bytes = GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite2) + Environment.NewLine, encoding);
            var expectedBytes2 = linesToWrite2Bytes;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    // create initial content
                    client.WriteAllText(remoteFile, initialContent);

                    #region Write less bytes than the current content, overwriting part of that content

                    client.WriteAllLines(remoteFile, linesToWrite1);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes1, actualBytes);

                    #endregion Write less bytes than the current content, overwriting part of that content

                    #region Write more bytes than the current content, overwriting and appending to that content

                    client.WriteAllLines(remoteFile, linesToWrite2);

                    actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes2, actualBytes);

                    #endregion Write more bytes than the current content, overwriting and appending to that content
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllLines_Array_NoEncoding_FileDoesNotExist()
        {
            var encoding = new UTF8Encoding(false, true);
            var linesToWrite = new[] { "\u0139isa", "&", "Sofie" };
            var linesToWriteBytes = GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite) + Environment.NewLine, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllLines(remoteFile, linesToWrite);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(linesToWriteBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllLines_Array_Encoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            var encoding = GetRandomEncoding();
            var linesToWrite = new[] { "Forever", "&", "\u0116ver" };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllLines(remoteFile, linesToWrite, encoding);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllLines_Array_Encoding_ExistingFile()
        {
            const string initialContent = "\u0100ert & Ann Forever & Ever Lisa & Sofie";

            var encoding = GetRandomEncoding();
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);
            var linesToWrite1 = new[] { "Forever", "&", "\u0116ver" };
            var linesToWrite1Bytes = GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite1) + Environment.NewLine, encoding);
            var expectedBytes1 = new ArrayBuilder<byte>().Add(linesToWrite1Bytes)
                                                         .Add(initialContentBytes, linesToWrite1Bytes.Length, initialContentBytes.Length - linesToWrite1Bytes.Length)
                                                         .Build();
            var linesToWrite2 = new[] { "Forever", "&", "\u0116ver", "Gert & Ann", "Lisa + Sofie" };
            var linesToWrite2Bytes = GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite2) + Environment.NewLine, encoding);
            var expectedBytes2 = linesToWrite2Bytes;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    // create initial content
                    client.WriteAllText(remoteFile, initialContent, encoding);

                    #region Write less bytes than the current content, overwriting part of that content

                    client.WriteAllLines(remoteFile, linesToWrite1, encoding);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes1, actualBytes);

                    #endregion Write less bytes than the current content, overwriting part of that content

                    #region Write more bytes than the current content, overwriting and appending to that content

                    client.WriteAllLines(remoteFile, linesToWrite2, encoding);

                    actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes2, actualBytes);

                    #endregion Write more bytes than the current content, overwriting and appending to that content
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllLines_Array_Encoding_FileDoesNotExist()
        {
            var encoding = GetRandomEncoding();
            var linesToWrite = new[] { "\u0139isa", "&", "Sofie" };
            var linesToWriteBytes = GetBytesWithPreamble(string.Join(Environment.NewLine, linesToWrite) + Environment.NewLine, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllLines(remoteFile, linesToWrite, encoding);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(linesToWriteBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }

            }
        }

        [TestMethod]
        public void Sftp_WriteAllText_NoEncoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            const string initialContent = "\u0100ert & Ann Forever & \u0116ver Lisa & Sofie";

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllText_NoEncoding_ExistingFile()
        {
            const string initialContent = "\u0100ert & Ann Forever & \u0116ver Lisa & Sofie";
            const string newContent1 = "For\u0116ver & Ever";

            var encoding = new UTF8Encoding(false, true);
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);
            var newContent1Bytes = GetBytesWithPreamble(newContent1, encoding);
            var expectedBytes1 = new ArrayBuilder<byte>().Add(newContent1Bytes)
                                                         .Add(initialContentBytes, newContent1Bytes.Length, initialContentBytes.Length - newContent1Bytes.Length)
                                                         .Build();
            var newContent2 = "Sofie & Lisa For\u0116ver & Ever with \u0100ert & Ann";
            var newContent2Bytes = GetBytesWithPreamble(newContent2, encoding);
            var expectedBytes2 = newContent2Bytes;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent);

                    #region Write less bytes than the current content, overwriting part of that content

                    client.WriteAllText(remoteFile, newContent1);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes1, actualBytes);

                    #endregion Write less bytes than the current content, overwriting part of that content

                    #region Write more bytes than the current content, overwriting and appending to that content

                    client.WriteAllText(remoteFile, newContent2);

                    actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes2, actualBytes);

                    #endregion Write more bytes than the current content, overwriting and appending to that content
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllText_NoEncoding_FileDoesNotExist()
        {
            const string initialContent = "\u0100ert & Ann Forever & \u0116ver Lisa & Sofie";

            var encoding = new UTF8Encoding(false, true);
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(initialContentBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllText_Encoding_DirectoryDoesNotExist()
        {
            const string remoteFile = "/home/sshnet/directorydoesnotexist/test";

            var encoding = GetRandomEncoding();
            const string content = "For\u0116ver & Ever";

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, content, encoding);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllText_Encoding_ExistingFile()
        {
            const string initialContent = "\u0100ert & Ann Forever & \u0116ver Lisa & Sofie";
            const string newContent1 = "For\u0116ver & Ever";
            const string newContent2 = "Sofie & Lisa For\u0116ver & Ever with \u0100ert & Ann";

            var encoding = GetRandomEncoding();
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);
            var newContent1Bytes = GetBytesWithPreamble(newContent1, encoding);
            var expectedBytes1 = new ArrayBuilder<byte>().Add(newContent1Bytes)
                                                         .Add(initialContentBytes, newContent1Bytes.Length, initialContentBytes.Length - newContent1Bytes.Length)
                                                         .Build();
            var newContent2Bytes = GetBytesWithPreamble(newContent2, encoding);
            var expectedBytes2 = newContent2Bytes;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent, encoding);

                    #region Write less bytes than the current content, overwriting part of that content

                    client.WriteAllText(remoteFile, newContent1, encoding);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes1, actualBytes);

                    #endregion Write less bytes than the current content, overwriting part of that content

                    #region Write more bytes than the current content, overwriting and appending to that content

                    client.WriteAllText(remoteFile, newContent2, encoding);

                    actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(expectedBytes2, actualBytes);

                    #endregion Write more bytes than the current content, overwriting and appending to that content
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_WriteAllText_Encoding_FileDoesNotExist()
        {
            const string initialContent = "\u0100ert & Ann Forever & \u0116ver Lisa & Sofie";

            var encoding = GetRandomEncoding();
            var initialContentBytes = GetBytesWithPreamble(initialContent, encoding);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, initialContent, encoding);

                    var actualBytes = client.ReadAllBytes(remoteFile);
                    CollectionAssert.AreEqual(initialContentBytes, actualBytes);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_BeginDownloadFile_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var ms = new MemoryStream())
                    {
                        var asyncResult = client.BeginDownloadFile(remoteFile, ms);
                        try
                        {
                            client.EndDownloadFile(asyncResult);
                            Assert.Fail();
                        }
                        catch (SftpPathNotFoundException ex)
                        {
                            Assert.IsNull(ex.InnerException);
                            Assert.AreEqual("No such file", ex.Message);

                            // ensure file was not created by us
                            Assert.IsFalse(client.Exists(remoteFile));
                        }
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_BeginListDirectory_DirectoryDoesNotExist()
        {
            const string remoteDirectory = "/home/sshnet/test123";

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                {
                    command.Execute();
                }
            }

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var asyncResult = client.BeginListDirectory(remoteDirectory, null, null);
                try
                {
                    client.EndListDirectory(asyncResult);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("No such file", ex.Message);

                    // ensure directory was not created by us
                    Assert.IsFalse(client.Exists(remoteDirectory));
                }
            }
        }

        [TestMethod]
        public void Sftp_BeginUploadFile_InputAndPath_DirectoryDoesNotExist()
        {
            const int size = 50 * 1024 * 1024;
            const string remoteDirectory = "/home/sshnet/test123";
            const string remoteFile = remoteDirectory + "/test";

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                {
                    command.Execute();
                }
            }

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var memoryStream = CreateMemoryStream(size);
                memoryStream.Position = 0;

                var asyncResult = client.BeginUploadFile(memoryStream, remoteFile);
                try
                {
                    client.EndUploadFile(asyncResult);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException)
                {
                }
            }
        }

        [TestMethod]
        public void Sftp_BeginUploadFile_InputAndPath_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    var uploadMemoryStream = new MemoryStream();
                    var sw = new StreamWriter(uploadMemoryStream, Encoding.UTF8);
                    sw.Write("Gert & Ann");
                    sw.Flush();
                    uploadMemoryStream.Position = 0;

                    var asyncResult = client.BeginUploadFile(uploadMemoryStream, remoteFile);
                    client.EndUploadFile(asyncResult);

                    using (var downloadMemoryStream = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloadMemoryStream);

                        downloadMemoryStream.Position = 0;

                        using (var sr = new StreamReader(downloadMemoryStream, Encoding.UTF8))
                        {
                            var content = sr.ReadToEnd();
                            Assert.AreEqual("Gert & Ann", content);
                        }
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_BeginUploadFile_InputAndPath_ExistingFile()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, "Gert & Ann & Lisa");

                    var uploadMemoryStream = new MemoryStream();
                    var sw = new StreamWriter(uploadMemoryStream, Encoding.UTF8);
                    sw.Write("Ann & Gert");
                    sw.Flush();
                    uploadMemoryStream.Position = 0;

                    var asyncResult = client.BeginUploadFile(uploadMemoryStream, remoteFile);
                    client.EndUploadFile(asyncResult);

                    using (var downloadMemoryStream = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloadMemoryStream);

                        downloadMemoryStream.Position = 0;

                        using (var sr = new StreamReader(downloadMemoryStream, Encoding.UTF8))
                        {
                            var content = sr.ReadToEnd();
                            Assert.AreEqual("Ann & Gert", content);
                        }
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsFalse_DirectoryDoesNotExist()
        {
            const int size = 50 * 1024 * 1024;
            const string remoteDirectory = "/home/sshnet/test123";
            const string remoteFile = remoteDirectory + "/test";

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                {
                    command.Execute();
                }
            }

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var memoryStream = CreateMemoryStream(size);
                memoryStream.Position = 0;

                var asyncResult = client.BeginUploadFile(memoryStream, remoteFile, false, null, null);
                try
                {
                    client.EndUploadFile(asyncResult);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException)
                {
                }
            }
        }

        [TestMethod]
        public void Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsFalse_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var uploadMemoryStream = new MemoryStream())
                    using (var sw = new StreamWriter(uploadMemoryStream, Encoding.UTF8))
                    {
                        sw.Write("Gert & Ann");
                        sw.Flush();
                        uploadMemoryStream.Position = 0;

                        var asyncResult = client.BeginUploadFile(uploadMemoryStream, remoteFile, false, null, null);
                        client.EndUploadFile(asyncResult);
                    }

                    using (var downloadMemoryStream = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloadMemoryStream);

                        downloadMemoryStream.Position = 0;

                        using (var sr = new StreamReader(downloadMemoryStream, Encoding.UTF8))
                        {
                            var content = sr.ReadToEnd();
                            Assert.AreEqual("Gert & Ann", content);
                        }
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsFalse_ExistingFile()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, "Gert & Ann & Lisa");

                    var uploadMemoryStream = new MemoryStream();
                    var sw = new StreamWriter(uploadMemoryStream, Encoding.UTF8);
                    sw.Write("Ann & Gert");
                    sw.Flush();
                    uploadMemoryStream.Position = 0;

                    var asyncResult = client.BeginUploadFile(uploadMemoryStream, remoteFile, false, null, null);

                    try
                    {
                        client.EndUploadFile(asyncResult);
                        Assert.Fail();
                    }
                    catch (SshException ex)
                    {
                        Assert.AreEqual(typeof(SshException), ex.GetType());
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("Failure", ex.Message);
                    }
                }
                finally
                {

                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsTrue_DirectoryDoesNotExist()
        {
            const int size = 50 * 1024 * 1024;
            const string remoteDirectory = "/home/sshnet/test123";
            const string remoteFile = remoteDirectory + "/test";

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                {
                    command.Execute();
                }
            }

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var memoryStream = CreateMemoryStream(size);
                memoryStream.Position = 0;

                var asyncResult = client.BeginUploadFile(memoryStream, remoteFile, true, null, null);
                try
                {
                    client.EndUploadFile(asyncResult);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException)
                {
                }
            }
        }

        [TestMethod]
        public void Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsTrue_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var uploadMemoryStream = new MemoryStream())
                    using (var sw = new StreamWriter(uploadMemoryStream, Encoding.UTF8))
                    {
                        sw.Write("Gert & Ann");
                        sw.Flush();
                        uploadMemoryStream.Position = 0;

                        var asyncResult = client.BeginUploadFile(uploadMemoryStream, remoteFile, true, null, null);
                        client.EndUploadFile(asyncResult);
                    }

                    using (var downloadMemoryStream = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloadMemoryStream);

                        downloadMemoryStream.Position = 0;

                        using (var sr = new StreamReader(downloadMemoryStream, Encoding.UTF8))
                        {
                            var content = sr.ReadToEnd();
                            Assert.AreEqual("Gert & Ann", content);
                        }
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsTrue_ExistingFile()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllText(remoteFile, "Gert & Ann & Lisa");

                    using (var uploadMemoryStream = new MemoryStream())
                    using (var sw = new StreamWriter(uploadMemoryStream, Encoding.UTF8))
                    {
                        sw.Write("Ann & Gert");
                        sw.Flush();
                        uploadMemoryStream.Position = 0;

                        var asyncResult = client.BeginUploadFile(uploadMemoryStream, remoteFile, true, null, null);
                        client.EndUploadFile(asyncResult);
                    }

                    using (var downloadMemoryStream = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloadMemoryStream);

                        downloadMemoryStream.Position = 0;

                        using (var sr = new StreamReader(downloadMemoryStream, Encoding.UTF8))
                        {
                            var content = sr.ReadToEnd();
                            Assert.AreEqual("Ann & Gert", content);
                        }
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_UploadAndDownloadBigFile()
        {
            const int size = 50 * 1024 * 1024;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.Delete(remoteFile);
                }

                try
                {
                    var memoryStream = CreateMemoryStream(size);
                    memoryStream.Position = 0;
                    var expectedHash = CreateHash(memoryStream);
                    memoryStream.Position = 0;

                    client.UploadFile(memoryStream, remoteFile);

                    memoryStream.SetLength(0);

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    client.DownloadFile(remoteFile, memoryStream);

                    stopwatch.Stop();

                    Assert.AreEqual(size, memoryStream.Length);
                    memoryStream.Position = 0;
                    var actualHash = CreateHash(memoryStream);
                    Assert.AreEqual(expectedHash, actualHash);

                    Console.WriteLine(@"Elapsed: {0} ms", stopwatch.ElapsedMilliseconds);
                    Console.WriteLine(@"Transfer speed: {0:N2} KB/s",
                                      CalculateTransferSpeed(memoryStream.Length, stopwatch.ElapsedMilliseconds));
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.Delete(remoteFile);
                    }
                }
            }
        }

        /// <summary>
        /// Issue 1672
        /// </summary>
        [TestMethod]
        public void Sftp_CurrentWorkingDirectory()
        {
            const string homeDirectory = "/home/sshnet";
            const string otherDirectory = homeDirectory + "/dir";

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(otherDirectory))
                {
                    client.DeleteDirectory(otherDirectory);
                }

                try
                {
                    client.CreateDirectory(otherDirectory);
                    client.ChangeDirectory(otherDirectory);

                    using (var s = CreateStreamWithContent("A"))
                    {
                        client.UploadFile(s, "a.txt");
                    }

                    using (var s = new MemoryStream())
                    {
                        client.DownloadFile("a.txt", s);
                        s.Position = 0;

                        var content = Encoding.ASCII.GetString(s.ToArray());
                        Assert.AreEqual("A", content);
                    }

                    Assert.IsTrue(client.Exists(otherDirectory + "/a.txt"));
                    client.DeleteFile("a.txt");
                    Assert.IsFalse(client.Exists(otherDirectory + "/a.txt"));
                    client.DeleteDirectory(".");
                    Assert.IsFalse(client.Exists(otherDirectory));
                }
                finally
                {
                    if (client.Exists(otherDirectory))
                    {
                        client.DeleteDirectory(otherDirectory);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Exists()
        {
            const string remoteHome = "/home/sshnet";

            #region Setup

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                #region Clean-up

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/DoesNotExist"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/symlink.to.directory.exists"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/directory.exists"}")
                )
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/symlink.to.file.exists"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -f {remoteHome + "/file.exists"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                #endregion Clean-up

                #region Setup

                using (var command = client.CreateCommand($"touch {remoteHome + "/file.exists"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"mkdir {remoteHome + "/directory.exists"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"ln -s {remoteHome + "/file.exists"} {remoteHome + "/symlink.to.file.exists"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"ln -s {remoteHome + "/directory.exists"} {remoteHome + "/symlink.to.directory.exists"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                #endregion Setup
            }

            #endregion Setup

            #region Assert

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                Assert.IsFalse(client.Exists(remoteHome + "/DoesNotExist"));
                Assert.IsTrue(client.Exists(remoteHome + "/file.exists"));
                Assert.IsTrue(client.Exists(remoteHome + "/symlink.to.file.exists"));
                Assert.IsTrue(client.Exists(remoteHome + "/directory.exists"));
                Assert.IsTrue(client.Exists(remoteHome + "/symlink.to.directory.exists"));
            }

            #endregion Assert

            #region Teardown

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/DoesNotExist"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/symlink.to.directory.exists"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/directory.exists"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/symlink.to.file.exists"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -f {remoteHome + "/file.exists"}"))
                {
                    command.Execute();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }
            }

            #endregion Teardown
        }

        [TestMethod]
        public async Task Sftp_ExistsAsync()
        {
            const string remoteHome = "/home/sshnet";

            #region Setup

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                #region Clean-up

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/DoesNotExist"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/symlink.to.directory.exists"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/directory.exists"}")
                )
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/symlink.to.file.exists"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -f {remoteHome + "/file.exists"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                #endregion Clean-up

                #region Setup

                using (var command = client.CreateCommand($"touch {remoteHome + "/file.exists"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"mkdir {remoteHome + "/directory.exists"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"ln -s {remoteHome + "/file.exists"} {remoteHome + "/symlink.to.file.exists"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"ln -s {remoteHome + "/directory.exists"} {remoteHome + "/symlink.to.directory.exists"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                #endregion Setup
            }

            #endregion Setup

            #region Assert

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                await client.ConnectAsync(default).ConfigureAwait(false);

                Assert.IsFalse(await client.ExistsAsync(remoteHome + "/DoesNotExist"));
                Assert.IsTrue(await client.ExistsAsync(remoteHome + "/file.exists"));
                Assert.IsTrue(await client.ExistsAsync(remoteHome + "/symlink.to.file.exists"));
                Assert.IsTrue(await client.ExistsAsync(remoteHome + "/directory.exists"));
                Assert.IsTrue(await client.ExistsAsync(remoteHome + "/symlink.to.directory.exists"));
            }

            #endregion Assert

            #region Teardown

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/DoesNotExist"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/symlink.to.directory.exists"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/directory.exists"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -Rf {remoteHome + "/symlink.to.file.exists"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }

                using (var command = client.CreateCommand($"rm -f {remoteHome + "/file.exists"}"))
                {
                    await command.ExecuteAsync();
                    Assert.AreEqual(0, command.ExitStatus, command.Error);
                }
            }

            #endregion Teardown
        }

        [TestMethod]
        public void Sftp_ListDirectory()
        {
            const string remoteDirectory = "/home/sshnet/test123";

            try
            {
                using (var client = new SshClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();
                    client.RunCommand($@"rm -Rf ""{remoteDirectory}""");
                    client.RunCommand($@"mkdir -p ""{remoteDirectory}""");
                    client.RunCommand($@"mkdir -p ""{remoteDirectory}/sub""");
                    client.RunCommand($@"touch ""{remoteDirectory}/file1""");
                    client.RunCommand($@"touch ""{remoteDirectory}/file2""");
                }

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    client.ChangeDirectory(remoteDirectory);

                    var directoryContent = client.ListDirectory(".").OrderBy(p => p.Name).ToList();
                    Assert.AreEqual(5, directoryContent.Count);

                    Assert.AreEqual(".", directoryContent[0].Name);
                    Assert.AreEqual($"{remoteDirectory}/.", directoryContent[0].FullName);
                    Assert.IsTrue(directoryContent[0].IsDirectory);

                    Assert.AreEqual("..", directoryContent[1].Name);
                    Assert.AreEqual($"{remoteDirectory}/..", directoryContent[1].FullName);
                    Assert.IsTrue(directoryContent[1].IsDirectory);

                    Assert.AreEqual("file1", directoryContent[2].Name);
                    Assert.AreEqual($"{remoteDirectory}/file1", directoryContent[2].FullName);
                    Assert.IsFalse(directoryContent[2].IsDirectory);

                    Assert.AreEqual("file2", directoryContent[3].Name);
                    Assert.AreEqual($"{remoteDirectory}/file2", directoryContent[3].FullName);
                    Assert.IsFalse(directoryContent[3].IsDirectory);

                    Assert.AreEqual("sub", directoryContent[4].Name);
                    Assert.AreEqual($"{remoteDirectory}/sub", directoryContent[4].FullName);
                    Assert.IsTrue(directoryContent[4].IsDirectory);
                }
            }
            finally
            {
                using (var client = new SshClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                    {
                        command.Execute();
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_ChangeDirectory_DirectoryDoesNotExist()
        {
            const string remoteDirectory = "/home/sshnet/test123";

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                {
                    command.Execute();
                }
            }

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                try
                {
                    client.ChangeDirectory(remoteDirectory);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException)
                {
                }
            }
        }

        [TestMethod]
        public async Task Sftp_ChangeDirectory_DirectoryDoesNotExistAsync()
        {
            const string remoteDirectory = "/home/sshnet/test123";

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                await client.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                {
                    await command.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
                }
            }

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                try
                {
                    await client.ChangeDirectoryAsync(remoteDirectory, CancellationToken.None).ConfigureAwait(false);
                    Assert.Fail();
                }
                catch (SftpPathNotFoundException)
                {
                }
            }
        }

        [TestMethod]
        public void Sftp_ChangeDirectory_DirectoryExists()
        {
            const string remoteDirectory = "/home/sshnet/test123";

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                {
                    command.Execute();
                }

                using (var command = client.CreateCommand("mkdir -p " + _remotePathTransformation.Transform(remoteDirectory)))
                {
                    command.Execute();
                }
            }

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    client.ChangeDirectory(remoteDirectory);

                    Assert.AreEqual(remoteDirectory, client.WorkingDirectory);

                    using (var uploadStream = CreateMemoryStream(100))
                    {
                        uploadStream.Position = 0;

                        client.UploadFile(uploadStream, "gert.txt");

                        uploadStream.Position = 0;

                        using (var downloadStream = client.OpenRead(remoteDirectory + "/gert.txt"))
                        {
                            Assert.AreEqual(CreateHash(uploadStream), CreateHash(downloadStream));
                        }
                    }
                }
            }
            finally
            {
                using (var client = new SshClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                    {
                        command.Execute();
                    }
                }
            }
        }

        [TestMethod]
        public async Task Sftp_ChangeDirectory_DirectoryExistsAsync()
        {
            const string remoteDirectory = "/home/sshnet/test123";

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                await client.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                {
                    await command.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
                }

                using (var command = client.CreateCommand("mkdir -p " + _remotePathTransformation.Transform(remoteDirectory)))
                {
                    await command.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
                }
            }

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    await client.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                    await client.ChangeDirectoryAsync(remoteDirectory, CancellationToken.None).ConfigureAwait(false);

                    Assert.AreEqual(remoteDirectory, client.WorkingDirectory);

                    using (var uploadStream = CreateMemoryStream(100))
                    {
                        uploadStream.Position = 0;

                        await client.UploadFileAsync(uploadStream, "gert.txt").ConfigureAwait(false);

                        uploadStream.Position = 0;

                        using (var downloadStream = client.OpenRead(remoteDirectory + "/gert.txt"))
                        {
                            Assert.AreEqual(CreateHash(uploadStream), CreateHash(downloadStream));
                        }
                    }
                }
            }
            finally
            {
                using (var client = new SshClient(_connectionInfoFactory.Create()))
                {
                    await client.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                    using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                    {
                        await command.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_SubsystemExecution_Failed()
        {
            var remoteSshdConfig = new RemoteSshd(_adminConnectionInfoFactory).OpenConfig();

            // Disable SFTP subsystem
            remoteSshdConfig.ClearSubsystems()
                            .Update()
                            .Restart();

            var remoteSshdReconfiguredToDefaultState = false;

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    try
                    {
                        client.Connect();
                        Assert.Fail("Establishing SFTP connection should have failed.");
                    }
                    catch (SshException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("Subsystem 'sftp' could not be executed.", ex.Message);
                    }

                    // Re-enable SFTP subsystem
                    remoteSshdConfig.Reset();

                    remoteSshdReconfiguredToDefaultState = true;

                    // ensure we can reconnect the same SftpClient instance
                    client.Connect();
                    // ensure SFTP session is correctly established
                    Assert.IsTrue(client.Exists("."));
                }
            }
            finally
            {
                if (!remoteSshdReconfiguredToDefaultState)
                {
                    remoteSshdConfig.Reset();
                }
            }
        }

        [TestMethod]
        public void Sftp_SftpFileStream_ReadAndWrite()
        {
            TestReadAndWrite(
                (s, buffer, offset, count) => s.Read(buffer, offset, count),
                (s, buffer, offset, count) => s.Write(buffer, offset, count));
        }

        [TestMethod]
        public void Sftp_SftpFileStream_ReadAndWriteByte()
        {
            TestReadAndWrite(
                read: (s, buffer, offset, count) =>
                {
                    int bytesRead = 0;
                    int b;

                    while (bytesRead < count && (b = s.ReadByte()) != -1)
                    {
                        buffer[offset + bytesRead] = (byte)b;
                        bytesRead++;
                    }

                    return bytesRead;
                },
                write: (s, buffer, offset, count) =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        s.WriteByte(buffer[offset + i]);
                    }
                });
        }

        [TestMethod]
        public void Sftp_SftpFileStream_ReadAndWriteAsync()
        {
            TestReadAndWrite(
                (s, buffer, offset, count) => s.ReadAsync(buffer, offset, count).GetAwaiter().GetResult(),
                (s, buffer, offset, count) => s.WriteAsync(buffer, offset, count).GetAwaiter().GetResult());
        }

        [TestMethod]
        public void Sftp_SftpFileStream_ReadAndWriteBeginEnd()
        {
            TestReadAndWrite(
                (s, buffer, offset, count) => s.EndRead(s.BeginRead(buffer, offset, count, null, null)),
                (s, buffer, offset, count) => s.EndWrite(s.BeginWrite(buffer, offset, count, null, null)));
        }

#if NET
        [TestMethod]
        public void Sftp_SftpFileStream_ReadAndWriteSpan()
        {
            TestReadAndWrite(
                (s, buffer, offset, count) => s.Read(buffer.AsSpan(offset, count)),
                (s, buffer, offset, count) => s.Write(buffer.AsSpan(offset, count)));
        }

        [TestMethod]
        public void Sftp_SftpFileStream_ReadAndWriteAsyncMemory()
        {
            TestReadAndWrite(
                (s, buffer, offset, count) => s.ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult(),
                (s, buffer, offset, count) => s.WriteAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult());
        }
#endif

        private void TestReadAndWrite(
            Func<SftpFileStream, byte[], int, int, int> read,
            Action<SftpFileStream, byte[], int, int> write)
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var s = client.Open(remoteFile, FileMode.CreateNew, FileAccess.Write))
                    {
                        Assert.IsFalse(s.CanRead);
                        Assert.IsTrue(s.CanWrite);

                        write(s, new byte[] { 5, 4, 3, 2, 1 }, 1, 3);
                    }

                    // switch from read to write mode
                    using (var s = client.Open(remoteFile, FileMode.Open, FileAccess.ReadWrite))
                    {
                        Assert.IsTrue(s.CanRead);
                        Assert.IsTrue(s.CanWrite);

                        Assert.AreEqual(4, ReadByte(s));
                        Assert.AreEqual(3, ReadByte(s));

                        Assert.AreEqual(2, s.Position);

                        WriteByte(s, 7);
                        write(s, new byte[] { 8, 9, 10, 11, 12 }, 1, 3);

                        Assert.AreEqual(6, s.Position);
                    }

                    using (var s = client.Open(remoteFile, FileMode.Open, FileAccess.Read))
                    {
                        Assert.IsTrue(s.CanRead);
                        Assert.IsFalse(s.CanWrite);

                        Assert.AreEqual(6, s.Length);

                        var buffer = new byte[s.Length];
                        Assert.AreEqual(6, read(s, buffer, 0, buffer.Length));

                        CollectionAssert.AreEqual(new byte[] { 4, 3, 7, 9, 10, 11 }, buffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, ReadByte(s));
                    }

                    // switch from read to write mode, and back to read mode and finally
                    // append a byte
                    using (var s = client.Open(remoteFile, FileMode.Open, FileAccess.ReadWrite))
                    {
                        Assert.AreEqual(4, ReadByte(s));
                        Assert.AreEqual(3, ReadByte(s));
                        Assert.AreEqual(7, ReadByte(s));

                        write(s, new byte[] { 0, 1, 6, 4 }, 1, 2);

                        Assert.AreEqual(11, ReadByte(s));

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, ReadByte(s));

                        WriteByte(s, 12);
                    }

                    // switch from write to read mode, and back to write mode
                    using (var s = client.Open(remoteFile, FileMode.Open, FileAccess.ReadWrite))
                    {
                        WriteByte(s, 5);
                        Assert.AreEqual(3, ReadByte(s));
                        WriteByte(s, 13);

                        Assert.AreEqual(3, s.Position);
                    }

                    using (var s = client.Open(remoteFile, FileMode.Open, FileAccess.Read))
                    {
                        Assert.AreEqual(7, s.Length);

                        var buffer = new byte[s.Length];
                        Assert.AreEqual(7, read(s, buffer, 0, buffer.Length));

                        CollectionAssert.AreEqual(new byte[] { 5, 3, 13, 1, 6, 11, 12 }, buffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, ReadByte(s));
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }

            int ReadByte(SftpFileStream s)
            {
                var buffer = new byte[1];
                var bytesRead = read(s, buffer, 0, 1);
                return bytesRead == 0 ? -1 : buffer[0];
            }

            void WriteByte(SftpFileStream s, byte b)
            {
                write(s, [b], 0, 1);
            }
        }

        [TestMethod]
        public void Sftp_SftpFileStream_SetLength_ReduceLength()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var s = client.Open(remoteFile, FileMode.CreateNew, FileAccess.Write))
                    {
                        s.Write(new byte[] { 5, 4, 3, 2, 1 }, 1, 3);
                    }

                    // reduce length while in write mode, with data in write buffer, and before
                    // current position
                    using (var s = client.Open(remoteFile, FileMode.Append, FileAccess.Write))
                    {
                        s.Position = 3;
                        s.Write(new byte[] { 6, 7, 8, 9 }, offset: 0, count: 4);

                        Assert.AreEqual(7, s.Position);

                        // verify buffer has not yet been flushed
                        using (var fs = client.Open(remoteFile, FileMode.Open, FileAccess.Read))
                        {
                            Assert.AreEqual(4, fs.ReadByte());
                            Assert.AreEqual(3, fs.ReadByte());
                            Assert.AreEqual(2, fs.ReadByte());

                            // Ensure we've reached end of the stream
                            Assert.AreEqual(-1, fs.ReadByte());
                        }

                        s.SetLength(5);

                        Assert.AreEqual(5, s.Position);

                        // verify that buffer was flushed and size has been modified
                        using (var fs = client.Open(remoteFile, FileMode.Open, FileAccess.Read))
                        {
                            Assert.AreEqual(4, fs.ReadByte());
                            Assert.AreEqual(3, fs.ReadByte());
                            Assert.AreEqual(2, fs.ReadByte());
                            Assert.AreEqual(6, fs.ReadByte());
                            Assert.AreEqual(7, fs.ReadByte());

                            // Ensure we've reached end of the stream
                            Assert.AreEqual(-1, fs.ReadByte());
                        }

                        s.WriteByte(1);
                    }

                    // verify that last byte was correctly written to the file
                    using (var s = client.Open(remoteFile, FileMode.Open, FileAccess.Read))
                    {
                        Assert.AreEqual(6, s.Length);

                        var buffer = new byte[s.Length + 2];
                        Assert.AreEqual(6, s.Read(buffer, offset: 0, buffer.Length));

                        CollectionAssert.AreEqual(new byte[] { 4, 3, 2, 6, 7, 1, 0, 0 }, buffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, s.ReadByte());
                    }

                    // reduce length while in read mode, but beyond current position
                    using (var s = client.Open(remoteFile, FileMode.Open, FileAccess.ReadWrite))
                    {
                        var buffer = new byte[1];
                        Assert.AreEqual(1, s.Read(buffer, offset: 0, buffer.Length));

                        CollectionAssert.AreEqual(new byte[] { 4 }, buffer);

                        s.SetLength(3);

                        using (var w = client.Open(remoteFile, FileMode.Open, FileAccess.Write))
                        {
                            w.Write(new byte[] { 8, 1, 6, 2 }, offset: 0, count: 4);
                        }

                        // verify that position was not changed
                        Assert.AreEqual(1, s.Position);

                        // verify that read buffer was cleared
                        Assert.AreEqual(1, s.ReadByte());
                        Assert.AreEqual(6, s.ReadByte());
                        Assert.AreEqual(2, s.ReadByte());

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, s.ReadByte());

                        Assert.AreEqual(4, s.Length);
                    }

                    // reduce length while in read mode, but before current position
                    using (var s = client.Open(remoteFile, FileMode.Open, FileAccess.ReadWrite))
                    {
                        var buffer = new byte[4];
                        Assert.AreEqual(4, s.Read(buffer, offset: 0, buffer.Length));

                        CollectionAssert.AreEqual(new byte[] { 8, 1, 6, 2 }, buffer);

                        Assert.AreEqual(4, s.Position);

                        s.SetLength(3);

                        // verify that position was moved to last byte
                        Assert.AreEqual(3, s.Position);

                        using (var w = client.Open(remoteFile, FileMode.Open, FileAccess.Read))
                        {
                            Assert.AreEqual(3, w.Length);

                            Assert.AreEqual(8, w.ReadByte());
                            Assert.AreEqual(1, w.ReadByte());
                            Assert.AreEqual(6, w.ReadByte());

                            // Ensure we've reached end of the stream
                            Assert.AreEqual(-1, w.ReadByte());
                        }

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, s.ReadByte());
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_SftpFileStream_Seek_BeyondEndOfFile_SeekOriginBegin()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.BufferSize = 500;
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF but not beyond buffer size
                    // do not write anything
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(offset: 3L, SeekOrigin.Begin);

                        Assert.AreEqual(3, newPosition);
                        Assert.AreEqual(3, fs.Position);
                        Assert.AreEqual(1, fs.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(1, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF and beyond buffer size
                    // do not write anything
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(offset: 700L, SeekOrigin.Begin);

                        Assert.AreEqual(700, newPosition);
                        Assert.AreEqual(700, fs.Position);
                        Assert.AreEqual(1, fs.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(1, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF but not beyond buffer size
                    // write less bytes than buffer size
                    var seekOffset = 3L;

                    // buffer holding the data that we'll write to the file
                    var writeBuffer = GenerateRandom(size: 7);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(seekOffset, SeekOrigin.Begin);

                        Assert.AreEqual(seekOffset, newPosition);
                        Assert.AreEqual(seekOffset, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);

                        Assert.AreEqual(seekOffset + writeBuffer.Length, fs.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(seekOffset + writeBuffer.Length, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        var soughtOverReadBuffer = new byte[seekOffset - 1];
                        Assert.AreEqual(soughtOverReadBuffer.Length, fs.Read(soughtOverReadBuffer, offset: 0, soughtOverReadBuffer.Length));
                        Assert.IsTrue(new byte[soughtOverReadBuffer.Length].IsEqualTo(soughtOverReadBuffer));

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(readBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF and beyond buffer size
                    // write less bytes than buffer size
                    seekOffset = 700L;

                    // buffer holding the data that we'll write to the file
                    writeBuffer = GenerateRandom(size: 4);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(seekOffset, SeekOrigin.Begin);

                        Assert.AreEqual(seekOffset, newPosition);
                        Assert.AreEqual(seekOffset, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);

                        Assert.AreEqual(seekOffset + writeBuffer.Length, fs.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(seekOffset + writeBuffer.Length, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        var soughtOverReadBuffer = new byte[seekOffset - 1];
                        Assert.AreEqual(soughtOverReadBuffer.Length, fs.Read(soughtOverReadBuffer, offset: 0, soughtOverReadBuffer.Length));
                        Assert.IsTrue(new byte[soughtOverReadBuffer.Length].IsEqualTo(soughtOverReadBuffer));

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(readBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF but not beyond buffer size
                    // write more bytes than buffer size
                    writeBuffer = GenerateRandom(size: 600);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(offset: 3L, SeekOrigin.Begin);

                        Assert.AreEqual(3, newPosition);
                        Assert.AreEqual(3, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(3 + writeBuffer.Length, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());
                        Assert.AreEqual(0x00, fs.ReadByte());
                        Assert.AreEqual(0x00, fs.ReadByte());

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(writeBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF and beyond buffer size
                    // write more bytes than buffer size
                    writeBuffer = GenerateRandom(size: 600);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(offset: 550, SeekOrigin.Begin);

                        Assert.AreEqual(550, newPosition);
                        Assert.AreEqual(550, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(550 + writeBuffer.Length, fs.Length);

                        Assert.AreEqual(0x04, fs.ReadByte());

                        var soughtOverReadBuffer = new byte[550 - 1];
                        Assert.AreEqual(550 - 1, fs.Read(soughtOverReadBuffer, offset: 0, soughtOverReadBuffer.Length));
                        Assert.IsTrue(new byte[550 - 1].IsEqualTo(soughtOverReadBuffer));

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(writeBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_SftpFileStream_Seek_BeyondEndOfFile_SeekOriginEnd()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.BufferSize = 500;
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF but not beyond buffer size
                    // do not write anything
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(offset: 3L, SeekOrigin.End);

                        Assert.AreEqual(4, newPosition);
                        Assert.AreEqual(4, fs.Position);
                        Assert.AreEqual(1, fs.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(1, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF and beyond buffer size
                    // do not write anything
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(offset: 700L, SeekOrigin.End);

                        Assert.AreEqual(701, newPosition);
                        Assert.AreEqual(701, fs.Position);
                        Assert.AreEqual(1, fs.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(1, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF but not beyond buffer size
                    // write less bytes than buffer size
                    var seekOffset = 3L;

                    // buffer holding the data that we'll write to the file
                    var writeBuffer = GenerateRandom(size: 7);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(seekOffset, SeekOrigin.End);

                        Assert.AreEqual(4, newPosition);
                        Assert.AreEqual(4, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);

                        Assert.AreEqual(1 + seekOffset + writeBuffer.Length, fs.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(1 + seekOffset + writeBuffer.Length, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        var soughtOverReadBuffer = new byte[seekOffset];
                        Assert.AreEqual(soughtOverReadBuffer.Length, fs.Read(soughtOverReadBuffer, offset: 0, soughtOverReadBuffer.Length));
                        Assert.IsTrue(new byte[soughtOverReadBuffer.Length].IsEqualTo(soughtOverReadBuffer));

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(readBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF and beyond buffer size
                    // write less bytes than buffer size
                    seekOffset = 700L;

                    // buffer holding the data that we'll write to the file
                    writeBuffer = GenerateRandom(size: 4);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(seekOffset, SeekOrigin.End);

                        Assert.AreEqual(1 + seekOffset, newPosition);
                        Assert.AreEqual(1 + seekOffset, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(1 + seekOffset + writeBuffer.Length, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        var soughtOverReadBuffer = new byte[seekOffset];
                        Assert.AreEqual(soughtOverReadBuffer.Length, fs.Read(soughtOverReadBuffer, offset: 0, soughtOverReadBuffer.Length));
                        Assert.IsTrue(new byte[soughtOverReadBuffer.Length].IsEqualTo(soughtOverReadBuffer));

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(readBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF but not beyond buffer size
                    // write more bytes than buffer size
                    seekOffset = 3L;
                    writeBuffer = GenerateRandom(size: 600);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(seekOffset, SeekOrigin.End);

                        Assert.AreEqual(1 + seekOffset, newPosition);
                        Assert.AreEqual(1 + seekOffset, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(1 + seekOffset + writeBuffer.Length, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        var soughtOverReadBuffer = new byte[seekOffset];
                        Assert.AreEqual(soughtOverReadBuffer.Length, fs.Read(soughtOverReadBuffer, offset: 0, soughtOverReadBuffer.Length));
                        Assert.IsTrue(new byte[soughtOverReadBuffer.Length].IsEqualTo(soughtOverReadBuffer));

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(writeBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    // seek beyond EOF and beyond buffer size
                    // write more bytes than buffer size
                    seekOffset = 550L;
                    writeBuffer = GenerateRandom(size: 600);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(seekOffset, SeekOrigin.End);

                        Assert.AreEqual(1 + seekOffset, newPosition);
                        Assert.AreEqual(1 + seekOffset, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(1 + seekOffset + writeBuffer.Length, fs.Length);

                        Assert.AreEqual(0x04, fs.ReadByte());

                        var soughtOverReadBuffer = new byte[seekOffset];
                        Assert.AreEqual(soughtOverReadBuffer.Length, fs.Read(soughtOverReadBuffer, offset: 0, soughtOverReadBuffer.Length));
                        Assert.IsTrue(new byte[soughtOverReadBuffer.Length].IsEqualTo(soughtOverReadBuffer));

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(writeBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_SftpFileStream_Seek_NegativeOffSet_SeekOriginEnd()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.BufferSize = 500;
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                        fs.WriteByte(0x07);
                        fs.WriteByte(0x05);
                    }

                    // seek within file and not beyond buffer size
                    // do not write anything
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(offset: -2L, SeekOrigin.End);

                        Assert.AreEqual(1, newPosition);
                        Assert.AreEqual(1, fs.Position);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(3, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());
                        Assert.AreEqual(0x07, fs.ReadByte());
                        Assert.AreEqual(0x05, fs.ReadByte());

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // buffer holding the data that we'll write to the file
                    var writeBuffer = GenerateRandom(size: (int)client.BufferSize + 200);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);
                    }

                    // seek within EOF and beyond buffer size
                    // do not write anything
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(offset: -100L, SeekOrigin.End);

                        Assert.AreEqual(600, newPosition);
                        Assert.AreEqual(600, fs.Position);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(writeBuffer.Length, fs.Length);

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(writeBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // seek within EOF and within buffer size
                    // write less bytes than buffer size
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);

                        var newPosition = fs.Seek(offset: -3, SeekOrigin.End);

                        Assert.AreEqual(697, newPosition);
                        Assert.AreEqual(697, fs.Position);

                        fs.WriteByte(0x01);
                        fs.WriteByte(0x05);
                        fs.WriteByte(0x04);
                        fs.WriteByte(0x07);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(writeBuffer.Length + 1, fs.Length);

                        var readBuffer = new byte[writeBuffer.Length - 3];
                        Assert.AreEqual(readBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        Assert.IsTrue(readBuffer.SequenceEqual(writeBuffer.Take(readBuffer.Length)));

                        Assert.AreEqual(0x01, fs.ReadByte());
                        Assert.AreEqual(0x05, fs.ReadByte());
                        Assert.AreEqual(0x04, fs.ReadByte());
                        Assert.AreEqual(0x07, fs.ReadByte());

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    // buffer holding the data that we'll write to the file
                    writeBuffer = GenerateRandom(size: (int)client.BufferSize * 4);

                    // seek within EOF and beyond buffer size
                    // write less bytes than buffer size
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);

                        var newPosition = fs.Seek(offset: -(client.BufferSize * 2), SeekOrigin.End);

                        Assert.AreEqual(1000, newPosition);
                        Assert.AreEqual(1000, fs.Position);

                        fs.WriteByte(0x01);
                        fs.WriteByte(0x05);
                        fs.WriteByte(0x04);
                        fs.WriteByte(0x07);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(writeBuffer.Length, fs.Length);

                        // First part of file should not have been touched
                        var readBuffer = new byte[(int)client.BufferSize * 2];
                        Assert.AreEqual(readBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        Assert.IsTrue(readBuffer.SequenceEqual(writeBuffer.Take(readBuffer.Length)));

                        // Check part that should have been updated
                        Assert.AreEqual(0x01, fs.ReadByte());
                        Assert.AreEqual(0x05, fs.ReadByte());
                        Assert.AreEqual(0x04, fs.ReadByte());
                        Assert.AreEqual(0x07, fs.ReadByte());

                        // Remaining bytes should not have been touched
                        readBuffer = new byte[((int)client.BufferSize * 2) - 4];
                        Assert.AreEqual(readBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        Assert.IsTrue(readBuffer.SequenceEqual(writeBuffer.Skip(((int)client.BufferSize * 2) + 4).Take(readBuffer.Length)));

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        /// https://github.com/sshnet/SSH.NET/issues/253
        [TestMethod]
        public void Sftp_SftpFileStream_Seek_Issue253()
        {
            var buf = Encoding.UTF8.GetBytes("123456");

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var ws = client.OpenWrite(remoteFile))
                    {
                        ws.Write(buf, offset: 0, count: 3);
                    }

                    using (var ws = client.OpenWrite(remoteFile))
                    {
                        var newPosition = ws.Seek(offset: 3, SeekOrigin.Begin);

                        Assert.AreEqual(3, newPosition);
                        Assert.AreEqual(3, ws.Position);

                        ws.Write(buf, 3, 3);
                    }

                    var actual = client.ReadAllText(remoteFile, Encoding.UTF8);
                    Assert.AreEqual("123456", actual);
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_SftpFileStream_Seek_WithinReadBuffer()
        {
            var originalContent = GenerateRandom(size: 800);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.BufferSize = 500;
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.Write(originalContent, offset: 0, originalContent.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        var readBuffer = new byte[200];

                        Assert.AreEqual(readBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));

                        var newPosition = fs.Seek(offset: 3L, SeekOrigin.Begin);

                        Assert.AreEqual(3L, newPosition);
                        Assert.AreEqual(3L, fs.Position);
                    }

                    client.DeleteFile(remoteFile);

                    #region Seek beyond EOF and beyond buffer size do not write anything

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(1, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(offset: 700L, SeekOrigin.Begin);

                        Assert.AreEqual(700L, newPosition);
                        Assert.AreEqual(700L, fs.Position);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(1, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    #endregion Seek beyond EOF and beyond buffer size do not write anything

                    #region Seek beyond EOF but not beyond buffer size and write less bytes than buffer size

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    var seekOffset = 3L;
                    var writeBuffer = GenerateRandom(size: 7);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(seekOffset, SeekOrigin.Begin);

                        Assert.AreEqual(seekOffset, newPosition);
                        Assert.AreEqual(seekOffset, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(seekOffset + writeBuffer.Length, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        var soughtOverReadBuffer = new byte[seekOffset - 1];
                        Assert.AreEqual(soughtOverReadBuffer.Length, fs.Read(soughtOverReadBuffer, offset: 0, soughtOverReadBuffer.Length));
                        Assert.IsTrue(new byte[soughtOverReadBuffer.Length].IsEqualTo(soughtOverReadBuffer));

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(readBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    #endregion Seek beyond EOF but not beyond buffer size and write less bytes than buffer size

                    #region Seek beyond EOF and beyond buffer size and write less bytes than buffer size

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    seekOffset = 700L;
                    writeBuffer = GenerateRandom(size: 4);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(seekOffset, SeekOrigin.Begin);

                        Assert.AreEqual(seekOffset, newPosition);
                        Assert.AreEqual(seekOffset, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(seekOffset + writeBuffer.Length, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());

                        var soughtOverReadBuffer = new byte[seekOffset - 1];
                        Assert.AreEqual(soughtOverReadBuffer.Length, fs.Read(soughtOverReadBuffer, offset: 0, soughtOverReadBuffer.Length));
                        Assert.IsTrue(new byte[soughtOverReadBuffer.Length].IsEqualTo(soughtOverReadBuffer));

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(readBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    #endregion Seek beyond EOF and beyond buffer size and write less bytes than buffer size

                    #region Seek beyond EOF but not beyond buffer size and write more bytes than buffer size

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    seekOffset = 3L;
                    writeBuffer = GenerateRandom(size: 600);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(seekOffset, SeekOrigin.Begin);

                        Assert.AreEqual(seekOffset, newPosition);
                        Assert.AreEqual(seekOffset, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(seekOffset + writeBuffer.Length, fs.Length);
                        Assert.AreEqual(0x04, fs.ReadByte());
                        Assert.AreEqual(0x00, fs.ReadByte());
                        Assert.AreEqual(0x00, fs.ReadByte());

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(writeBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    #endregion Seek beyond EOF but not beyond buffer size and write more bytes than buffer size

                    #region Seek beyond EOF and beyond buffer size and write more bytes than buffer size

                    // create single-byte file
                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        fs.WriteByte(0x04);
                    }

                    seekOffset = 550L;
                    writeBuffer = GenerateRandom(size: 600);

                    using (var fs = client.OpenWrite(remoteFile))
                    {
                        var newPosition = fs.Seek(seekOffset, SeekOrigin.Begin);

                        Assert.AreEqual(seekOffset, newPosition);
                        Assert.AreEqual(seekOffset, fs.Position);

                        fs.Write(writeBuffer, offset: 0, writeBuffer.Length);
                    }

                    using (var fs = client.OpenRead(remoteFile))
                    {
                        Assert.AreEqual(seekOffset + writeBuffer.Length, fs.Length);

                        Assert.AreEqual(0x04, fs.ReadByte());

                        var soughtOverReadBuffer = new byte[seekOffset - 1];
                        Assert.AreEqual(seekOffset - 1, fs.Read(soughtOverReadBuffer, offset: 0, soughtOverReadBuffer.Length));
                        Assert.IsTrue(new byte[seekOffset - 1].IsEqualTo(soughtOverReadBuffer));

                        var readBuffer = new byte[writeBuffer.Length];
                        Assert.AreEqual(writeBuffer.Length, fs.Read(readBuffer, offset: 0, readBuffer.Length));
                        CollectionAssert.AreEqual(writeBuffer, readBuffer);

                        // Ensure we've reached end of the stream
                        Assert.AreEqual(-1, fs.ReadByte());
                    }

                    client.DeleteFile(remoteFile);

                    #endregion Seek beyond EOF and beyond buffer size and write more bytes than buffer size
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_SftpFileStream_SetLength_FileDoesNotExist()
        {
            var size = new Random().Next(500, 5000);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    using (var s = client.Open(remoteFile, FileMode.Append, FileAccess.Write))
                    {
                        s.SetLength(size);
                    }

                    Assert.IsTrue(client.Exists(remoteFile));

                    var attributes = client.GetAttributes(remoteFile);
                    Assert.IsTrue(attributes.IsRegularFile);
                    Assert.AreEqual(size, attributes.Size);

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(new byte[size]), CreateHash(downloaded));
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_Append_Write_ExistingFile()
        {
            const int fileSize = 5 * 1024;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            using (var input = CreateMemoryStream(fileSize))
            {
                input.Position = 0;

                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.UploadFile(input, remoteFile);

                    using (var s = client.Open(remoteFile, FileMode.Append, FileAccess.Write))
                    {
                        Assert.IsFalse(s.CanRead);
                        Assert.IsTrue(s.CanWrite);

                        var buffer = new byte[] { 0x05, 0x0f, 0x0d, 0x0a, 0x04 };
                        s.Write(buffer, offset: 0, buffer.Length);
                        input.Write(buffer, offset: 0, buffer.Length);
                    }

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);

                        input.Position = 0;
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(input), CreateHash(downloaded));
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_Append_Write_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    #region Verify if merely opening the file for append creates a zero-byte file

                    using (client.Open(remoteFile, FileMode.Append, FileAccess.Write))
                    {
                    }

                    Assert.IsTrue(client.Exists(remoteFile));

                    var attributes = client.GetAttributes(remoteFile);
                    Assert.IsTrue(attributes.IsRegularFile);
                    Assert.AreEqual(0L, attributes.Size);

                    #endregion Verify if merely opening the file for append creates it

                    client.DeleteFile(remoteFile);

                    #region Verify if content is actually written to the file

                    var content = GenerateRandom(size: 100);

                    using (var s = client.Open(remoteFile, FileMode.Append, FileAccess.Write))
                    {
                        s.Write(content, offset: 0, content.Length);
                    }

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(content), CreateHash(downloaded));
                    }

                    #endregion Verify if content is actually written to the file
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_PathAndMode_ModeIsCreate_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    #region Verify if merely opening the file for create creates a zero-byte file

                    using (client.Open(remoteFile, FileMode.Create))
                    {
                    }

                    Assert.IsTrue(client.Exists(remoteFile));

                    var attributes = client.GetAttributes(remoteFile);
                    Assert.IsTrue(attributes.IsRegularFile);
                    Assert.AreEqual(0L, attributes.Size);

                    #endregion Verify if merely opening the file for create creates a zero-byte file

                    client.DeleteFile(remoteFile);

                    #region Verify if content is actually written to the file

                    var content = GenerateRandom(size: 100);

                    using (var s = client.Open(remoteFile, FileMode.Create))
                    {
                        Assert.IsTrue(s.CanRead);
                        Assert.IsTrue(s.CanWrite);

                        s.Write(content, offset: 0, content.Length);
                    }

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(content), CreateHash(downloaded));
                    }

                    #endregion Verify if content is actually written to the file
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_PathAndMode_ModeIsCreate_ExistingFile()
        {
            const int fileSize = 5 * 1024;
            var newContent = new byte[] { 0x07, 0x03, 0x02, 0x0b };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            using (var input = CreateMemoryStream(fileSize))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    input.Position = 0;
                    client.UploadFile(input, remoteFile);

                    using (var stream = client.Open(remoteFile, FileMode.Create))
                    {
                        // Verify if merely opening the file for create overwrites the file
                        var attributes = client.GetAttributes(remoteFile);
                        Assert.IsTrue(attributes.IsRegularFile);
                        Assert.AreEqual(0L, attributes.Size);

                        stream.Write(newContent, offset: 0, newContent.Length);
                        stream.Position = 0;

                        Assert.AreEqual(CreateHash(newContent), CreateHash(stream));
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_PathAndModeAndAccess_ModeIsCreate_AccessIsReadWrite_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    #region Verify if merely opening the file for create creates a zero-byte file

                    using (client.Open(remoteFile, FileMode.Create, FileAccess.ReadWrite))
                    {
                    }

                    Assert.IsTrue(client.Exists(remoteFile));

                    var attributes = client.GetAttributes(remoteFile);
                    Assert.IsTrue(attributes.IsRegularFile);
                    Assert.AreEqual(0L, attributes.Size);

                    #endregion Verify if merely opening the file for create creates a zero-byte file

                    client.DeleteFile(remoteFile);

                    #region Verify if content is actually written to the file

                    var content = GenerateRandom(size: 100);

                    using (var s = client.Open(remoteFile, FileMode.Create, FileAccess.ReadWrite))
                    {
                        s.Write(content, offset: 0, content.Length);
                    }

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(content), CreateHash(downloaded));
                    }

                    #endregion Verify if content is actually written to the file
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_PathAndModeAndAccess_ModeIsCreate_AccessIsReadWrite_ExistingFile()
        {
            const int fileSize = 5 * 1024;
            var newContent = new byte[] { 0x07, 0x03, 0x02, 0x0b };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            using (var input = CreateMemoryStream(fileSize))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    input.Position = 0;
                    client.UploadFile(input, remoteFile);

                    using (var stream = client.Open(remoteFile, FileMode.Create, FileAccess.ReadWrite))
                    {
                        // Verify if merely opening the file for create overwrites the file
                        var attributes = client.GetAttributes(remoteFile);
                        Assert.IsTrue(attributes.IsRegularFile);
                        Assert.AreEqual(0L, attributes.Size);

                        stream.Write(newContent, offset: 0, newContent.Length);
                        stream.Position = 0;

                        Assert.AreEqual(CreateHash(newContent), CreateHash(stream));
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_PathAndModeAndAccess_ModeIsCreate_AccessIsWrite_ExistingFile()
        {
            // use new content that contains less bytes than original content to
            // verify whether file is first truncated
            var originalContent = new byte[] { 0x05, 0x0f, 0x0d, 0x0a, 0x04 };
            var newContent = new byte[] { 0x07, 0x03, 0x02, 0x0b };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllBytes(remoteFile, originalContent);

                    using (var s = client.Open(remoteFile, FileMode.Create, FileAccess.Write))
                    {
                        s.Write(newContent, offset: 0, newContent.Length);
                    }

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);

                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(newContent), CreateHash(downloaded));
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_PathAndModeAndAccess_ModeIsCreate_AccessIsWrite_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    #region Verify if merely opening the file for create creates a zero-byte file

                    using (client.Open(remoteFile, FileMode.Create, FileAccess.Write))
                    {
                    }

                    Assert.IsTrue(client.Exists(remoteFile));

                    var attributes = client.GetAttributes(remoteFile);
                    Assert.IsTrue(attributes.IsRegularFile);
                    Assert.AreEqual(0L, attributes.Size);

                    #endregion Verify if merely opening the file for create creates a zero-byte file

                    client.DeleteFile(remoteFile);

                    #region Verify if content is actually written to the file

                    var content = GenerateRandom(size: 100);

                    using (var s = client.Open(remoteFile, FileMode.Create, FileAccess.Write))
                    {
                        s.Write(content, offset: 0, content.Length);
                    }

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(content), CreateHash(downloaded));
                    }

                    #endregion Verify if content is actually written to the file
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_CreateNew_Write_ExistingFile()
        {
            const int fileSize = 5 * 1024;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            using (var input = CreateMemoryStream(fileSize))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                input.Position = 0;

                try
                {
                    client.UploadFile(input, remoteFile);

                    Stream stream = null;

                    try
                    {
                        stream = client.Open(remoteFile, FileMode.CreateNew, FileAccess.Write);
                        Assert.Fail();
                    }
                    catch (SshException)
                    {
                    }
                    finally
                    {
                        stream?.Dispose();
                    }

                    // Verify that the file was not modified
                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);

                        input.Position = 0;
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(input), CreateHash(downloaded));
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_CreateNew_Write_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    #region Verify if merely opening the file creates a zero-byte file

                    using (client.Open(remoteFile, FileMode.CreateNew, FileAccess.Write))
                    {
                    }

                    Assert.IsTrue(client.Exists(remoteFile));

                    var attributes = client.GetAttributes(remoteFile);
                    Assert.IsTrue(attributes.IsRegularFile);
                    Assert.AreEqual(0L, attributes.Size);

                    #endregion Verify if merely opening the file creates it

                    client.DeleteFile(remoteFile);

                    #region Verify if content is actually written to the file

                    var content = GenerateRandom(size: 100);

                    using (var s = client.Open(remoteFile, FileMode.CreateNew, FileAccess.Write))
                    {
                        s.Write(content, offset: 0, content.Length);
                    }

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(content), CreateHash(downloaded));
                    }

                    #endregion Verify if content is actually written to the file
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_Open_Write_ExistingFile()
        {
            // use new content that contains less bytes than original content to
            // verify whether file is first truncated
            var originalContent = new byte[] { 0x05, 0x0f, 0x0d, 0x0a, 0x04 };
            var newContent = new byte[] { 0x07, 0x03, 0x02, 0x0b };
            var expectedContent = new byte[] { 0x07, 0x03, 0x02, 0x0b, 0x04 };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllBytes(remoteFile, originalContent);

                    using (var s = client.Open(remoteFile, FileMode.Open, FileAccess.Write))
                    {
                        s.Write(newContent, offset: 0, newContent.Length);
                    }

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);

                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(expectedContent), CreateHash(downloaded));
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_Open_Write_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    Stream stream = null;

                    try
                    {
                        stream = client.Open(remoteFile, FileMode.Open, FileAccess.Write);
                        Assert.Fail();
                    }
                    catch (SshException)
                    {
                    }
                    finally
                    {
                        stream?.Dispose();
                    }

                    Assert.IsFalse(client.Exists(remoteFile));
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_OpenOrCreate_Write_ExistingFile()
        {
            // use new content that contains less bytes than original content to
            // verify whether file is first truncated
            var originalContent = new byte[] { 0x05, 0x0f, 0x0d, 0x0a, 0x04 };
            var newContent = new byte[] { 0x07, 0x03, 0x02, 0x0b };
            var expectedContent = new byte[] { 0x07, 0x03, 0x02, 0x0b, 0x04 };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    client.WriteAllBytes(remoteFile, originalContent);

                    using (var s = client.Open(remoteFile, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        s.Write(newContent, offset: 0, newContent.Length);
                    }

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);

                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(expectedContent), CreateHash(downloaded));
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_OpenOrCreate_Write_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    #region Verify if merely opening the file creates a zero-byte file

                    using (client.Open(remoteFile, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                    }

                    Assert.IsTrue(client.Exists(remoteFile));

                    var attributes = client.GetAttributes(remoteFile);
                    Assert.IsTrue(attributes.IsRegularFile);
                    Assert.AreEqual(0L, attributes.Size);

                    #endregion Verify if merely opening the file creates it

                    client.DeleteFile(remoteFile);

                    #region Verify if content is actually written to the file

                    var content = GenerateRandom(size: 100);

                    using (var s = client.Open(remoteFile, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        s.Write(content, offset: 0, content.Length);
                    }

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(content), CreateHash(downloaded));
                    }

                    #endregion Verify if content is actually written to the file
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_Truncate_Write_ExistingFile()
        {
            const int fileSize = 5 * 1024;

            // use new content that contains less bytes than original content to
            // verify whether file is first truncated
            var originalContent = new byte[] { 0x05, 0x0f, 0x0d, 0x0a, 0x04 };
            var newContent = new byte[] { 0x07, 0x03, 0x02, 0x0b };

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            using (var input = CreateMemoryStream(fileSize))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                input.Position = 0;

                try
                {
                    client.WriteAllBytes(remoteFile, originalContent);

                    using (var s = client.Open(remoteFile, FileMode.Truncate, FileAccess.Write))
                    {
                        s.Write(newContent, offset: 0, newContent.Length);
                    }

                    using (var downloaded = new MemoryStream())
                    {
                        DownloadFileRandomMethod(client, remoteFile, downloaded);

                        input.Position = 0;
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateHash(newContent), CreateHash(downloaded));
                    }
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_Open_Truncate_Write_FileDoesNotExist()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var remoteFile = GenerateUniqueRemoteFileName();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                try
                {
                    Stream stream = null;

                    try
                    {
                        stream = client.Open(remoteFile, FileMode.Truncate, FileAccess.Write);
                        Assert.Fail();
                    }
                    catch (SshException)
                    {
                    }

                    Assert.IsFalse(client.Exists(remoteFile));
                }
                finally
                {
                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

        [TestMethod]
        public void Sftp_SetLastAccessTime()
        {
            var testFilePath = "/home/sshnet/test-file.txt";
            var testContent = "File";
            using var client = new SftpClient(_connectionInfoFactory.Create());
            client.Connect();

            using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

            client.UploadFile(fileStream, testFilePath);

            try
            {
                var newTime = new DateTime(1986, 03, 15, 01, 02, 03, 123, DateTimeKind.Local);

                client.SetLastAccessTime(testFilePath, newTime);
                var time = client.GetLastAccessTime(testFilePath);

                DateTimeAssert.AreEqual(newTime.TruncateToWholeSeconds(), time);
            }
            finally
            {
                client.DeleteFile(testFilePath);
            }
        }


        [TestMethod]
        public void Sftp_SetLastAccessTimeUtc()
        {
            var testFilePath = "/home/sshnet/test-file.txt";
            var testContent = "File";
            using var client = new SftpClient(_connectionInfoFactory.Create());
            client.Connect();

            using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

            client.UploadFile(fileStream, testFilePath);
            try
            {
                var newTime = new DateTime(1986, 03, 15, 01, 02, 03, 123, DateTimeKind.Utc);

                client.SetLastAccessTimeUtc(testFilePath, newTime);
                var time = client.GetLastAccessTimeUtc(testFilePath);

                DateTimeAssert.AreEqual(newTime.TruncateToWholeSeconds(), time);
            }
            finally
            {
                client.DeleteFile(testFilePath);
            }
        }

        [TestMethod]
        public void Sftp_SetLastWriteTime()
        {
            var testFilePath = "/home/sshnet/test-file.txt";
            var testContent = "File";
            using var client = new SftpClient(_connectionInfoFactory.Create());
            client.Connect();

            using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

            client.UploadFile(fileStream, testFilePath);
            try
            {
                var newTime = new DateTime(1986, 03, 15, 01, 02, 03, 123, DateTimeKind.Local);

                client.SetLastWriteTime(testFilePath, newTime);
                var time = client.GetLastWriteTime(testFilePath);

                DateTimeAssert.AreEqual(newTime.TruncateToWholeSeconds(), time);
            }
            finally
            {
                client.DeleteFile(testFilePath);
            }
        }

        [TestMethod]
        public void Sftp_SetLastWriteTimeUtc()
        {
            var testFilePath = "/home/sshnet/test-file.txt";
            var testContent = "File";
            using var client = new SftpClient(_connectionInfoFactory.Create());
            client.Connect();

            using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

            client.UploadFile(fileStream, testFilePath);
            try
            {
                var newTime = new DateTime(1986, 03, 15, 01, 02, 03, 123, DateTimeKind.Utc);

                client.SetLastWriteTimeUtc(testFilePath, newTime);
                var time = client.GetLastWriteTimeUtc(testFilePath);

                DateTimeAssert.AreEqual(newTime.TruncateToWholeSeconds(), time);
            }
            finally
            {
                client.DeleteFile(testFilePath);
            }
        }

        private static IEnumerable<object[]> GetSftpUploadFileFileStreamData()
        {
            yield return new object[] { 0 };
            yield return new object[] { 5 * 1024 * 1024 };
        }

        private static Encoding GetRandomEncoding()
        {
            var random = new Random().Next(1, 4);
            switch (random)
            {
                case 1:
                    return Encoding.Unicode;
                case 2:
                    return Encoding.UTF8;
                default:
                    Debug.Assert(random == 3);
                    return Encoding.UTF32;
            }
        }

        private void DownloadFileRandomMethod(SftpClient client, string path, Stream output)
        {
            Console.Write($"Downloading '{path}'");

            var random = new Random().Next(1, 6);
            switch (random)
            {
                case 1:
                    Console.WriteLine($" with {nameof(SftpClient.DownloadFile)}");

                    client.DownloadFile(path, output);

                    break;
                case 2:
                    Console.WriteLine($" with {nameof(SftpClient.DownloadFileAsync)}");

                    client.DownloadFileAsync(path, output).GetAwaiter().GetResult();

                    break;
                case 3:
                    Console.WriteLine($" with {nameof(SftpClient.BeginDownloadFile)}");

                    client.EndDownloadFile(client.BeginDownloadFile(path, output));

                    break;
                case 4:
                    Console.WriteLine($" with {nameof(SftpFileStream.CopyTo)}");

                    using (var fs = client.OpenRead(path))
                    {
                        fs.CopyTo(output);
                    }

                    break;
                default:
                    Debug.Assert(random == 5);
                    Console.WriteLine($" with {nameof(SftpFileStream.CopyToAsync)}");

                    using (var fs = client.OpenAsync(path, FileMode.Open, FileAccess.Read, CancellationToken.None).GetAwaiter().GetResult())
                    {
                        fs.CopyToAsync(output).GetAwaiter().GetResult();
                    }

                    break;
            }
        }

        private static byte[] GetBytesWithPreamble(string text, Encoding encoding)
        {
            var preamble = encoding.GetPreamble();
            var textBytes = encoding.GetBytes(text);

            if (preamble.Length != 0)
            {
                var textAndPreambleBytes = new byte[preamble.Length + textBytes.Length];
                Buffer.BlockCopy(preamble, srcOffset: 0, textAndPreambleBytes, dstOffset: 0, preamble.Length);
                Buffer.BlockCopy(textBytes, srcOffset: 0, textAndPreambleBytes, preamble.Length, textBytes.Length);
                return textAndPreambleBytes;
            }

            return textBytes;
        }

        private static decimal CalculateTransferSpeed(long length, long elapsedMilliseconds)
        {
            return (length / 1024m) / (elapsedMilliseconds / 1000m);
        }

        private static void SftpCreateRemoteFile(SftpClient client, string remoteFile, int size)
        {
            var file = CreateTempFile(size);

            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    client.UploadFile(fs, remoteFile);
                }
            }
            finally
            {
                File.Delete(file);
            }
        }

        private static byte[] GenerateRandom(int size)
        {
            var random = new Random();
            var randomContent = new byte[size];
            random.NextBytes(randomContent);
            return randomContent;
        }

        private static Stream CreateStreamWithContent(string content)
        {
            var memoryStream = new MemoryStream();
            var sw = new StreamWriter(memoryStream, Encoding.ASCII, 1024);
            sw.Write(content);
            sw.Flush();
            memoryStream.Position = 0;
            return memoryStream;
        }

        private static string GenerateUniqueRemoteFileName()
        {
            return $"/home/sshnet/{Guid.NewGuid():D}";
        }
    }
}

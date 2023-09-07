using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests
{
    // TODO SCP: UPLOAD / DOWNLOAD ZERO LENGTH FILES
    // TODO SCP: UPLOAD / DOWNLOAD EMPTY DIRECTORY
    // TODO SCP: UPLOAD DIRECTORY THAT ALREADY EXISTS ON REMOTE HOST

    [TestClass]
    public class ScpTests : TestBase
    {
        private IConnectionInfoFactory _connectionInfoFactory;
        private IRemotePathTransformation _remotePathTransformation;

        [TestInitialize]
        public void SetUp()
        {
            _connectionInfoFactory = new LinuxVMConnectionFactory(SshServerHostName, SshServerPort);
            _remotePathTransformation = RemotePathTransformation.ShellQuote;
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpDownloadStreamDirectoryDoesNotExistData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Download_Stream_DirectoryDoesNotExist()
        {
            foreach (var data in GetScpDownloadStreamDirectoryDoesNotExistData())
            {
                Scp_Download_Stream_DirectoryDoesNotExist((IRemotePathTransformation) data[0],
                                                           (string) data[1],
                                                           (string) data[2]);
            }
        }
#endif
        public void Scp_Download_Stream_DirectoryDoesNotExist(IRemotePathTransformation remotePathTransformation,
                                                              string remotePath,
                                                              string remoteFile)
        {
            var completeRemotePath = CombinePaths(remotePath, remoteFile);

            // remove complete directory if it's not the home directory of the user
            // or else remove the remote file
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(completeRemotePath))
                {
                    client.DeleteFile(completeRemotePath);
                }

                if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                {
                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }
                }
            }

            try
            {
                using (var downloaded = new MemoryStream())
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Download(completeRemotePath, downloaded);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {completeRemotePath}: No such file or directory", ex.Message);
                    }
                }
            }
            finally
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(completeRemotePath))
                    {
                        client.DeleteFile(completeRemotePath);
                    }

                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (client.Exists(remotePath))
                        {
                            client.DeleteDirectory(remotePath);
                        }
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpDownloadStreamFileDoesNotExistData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Download_Stream_FileDoesNotExist()
        {
            foreach (var data in GetScpDownloadStreamFileDoesNotExistData())
            {
                Scp_Download_Stream_FileDoesNotExist((IRemotePathTransformation)data[0],
                                                      (string)data[1],
                                                      (string)data[2]);
            }
        }
#endif
        public void Scp_Download_Stream_FileDoesNotExist(IRemotePathTransformation remotePathTransformation,
                                                         string remotePath,
                                                         string remoteFile)
        {
            var completeRemotePath = CombinePaths(remotePath, remoteFile);

            // remove complete directory if it's not the home directory of the user
            // or else remove the remote file
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(completeRemotePath))
                {
                    client.DeleteFile(completeRemotePath);
                }

                if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                {
                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }

                    client.CreateDirectory(remotePath);
                }
            }

            try
            {
                using (var downloaded = new MemoryStream())
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Download(completeRemotePath, downloaded);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {completeRemotePath}: No such file or directory", ex.Message);
                    }
                }
            }
            finally
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(completeRemotePath))
                    {
                        client.DeleteFile(completeRemotePath);
                    }

                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (client.Exists(remotePath))
                        {
                            client.DeleteDirectory(remotePath);
                        }
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpDownloadDirectoryInfoDirectoryDoesNotExistData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Download_DirectoryInfo_DirectoryDoesNotExist()
        {
            foreach (var data in GetScpDownloadDirectoryInfoDirectoryDoesNotExistData())
            {
                Scp_Download_DirectoryInfo_DirectoryDoesNotExist((IRemotePathTransformation)data[0],
                                                                 (string)data[1]);
            }
        }
#endif
        public void Scp_Download_DirectoryInfo_DirectoryDoesNotExist(IRemotePathTransformation remotePathTransformation,
                                                                     string remotePath)
        {
            var localDirectory = Path.GetTempFileName();
            File.Delete(localDirectory);
            Directory.CreateDirectory(localDirectory);

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }
                }

                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Download(remotePath, new DirectoryInfo(localDirectory));
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {remotePath}: No such file or directory", ex.Message);
                    }
                }
            }
            finally
            {
                Directory.Delete(localDirectory, true);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpDownloadDirectoryInfoExistingFileData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Download_DirectoryInfo_ExistingFile()
        {
            foreach (var data in GetScpDownloadDirectoryInfoExistingFileData())
            {
                Scp_Download_DirectoryInfo_ExistingFile((IRemotePathTransformation)data[0],
                                                         (string)data[1]);
            }
        }
#endif
        public void Scp_Download_DirectoryInfo_ExistingFile(IRemotePathTransformation remotePathTransformation,
                                                            string remotePath)
        {
            var content = CreateMemoryStream(100);
            content.Position = 0;

            var localDirectory = Path.GetTempFileName();
            File.Delete(localDirectory);
            Directory.CreateDirectory(localDirectory);

            var localFile = Path.Combine(localDirectory, PosixPath.GetFileName(remotePath));

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();
                    client.UploadFile(content, remotePath);
                }

                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();
                    client.Download(remotePath, new DirectoryInfo(localDirectory));
                }

                Assert.IsTrue(File.Exists(localFile));

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    using (var downloaded = new MemoryStream())
                    {
                        client.DownloadFile(remotePath, downloaded);
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateFileHash(localFile), CreateHash(downloaded));
                    }
                }
            }
            finally
            {
                Directory.Delete(localDirectory, true);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(remotePath))
                    {
                        client.DeleteFile(remotePath);
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpDownloadDirectoryInfoExistingDirectoryData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Download_DirectoryInfo_ExistingDirectory()
        {
            foreach (var data in GetScpDownloadDirectoryInfoExistingDirectoryData())
            {
                Scp_Download_DirectoryInfo_ExistingDirectory((IRemotePathTransformation)data[0],
                                                             (string)data[1]);
            }
        }
#endif
        public void Scp_Download_DirectoryInfo_ExistingDirectory(IRemotePathTransformation remotePathTransformation,
                                                                 string remotePath)
        {
            var localDirectory = Path.GetTempFileName();
            File.Delete(localDirectory);
            Directory.CreateDirectory(localDirectory);

            var localPathFile1 = Path.Combine(localDirectory, "file1 23");
            var remotePathFile1 = CombinePaths(remotePath, "file1 23");
            var contentFile1 = CreateMemoryStream(1024);
            contentFile1.Position = 0;

            var localPathFile2 = Path.Combine(localDirectory, "file2 #$%");
            var remotePathFile2 = CombinePaths(remotePath, "file2 #$%");
            var contentFile2 = CreateMemoryStream(2048);
            contentFile2.Position = 0;

            var localPathSubDirectory = Path.Combine(localDirectory, "subdir $1%#");
            var remotePathSubDirectory = CombinePaths(remotePath, "subdir $1%#");

            var localPathFile3 = Path.Combine(localPathSubDirectory, "file3 %$#");
            var remotePathFile3 = CombinePaths(remotePathSubDirectory, "file3 %$#");
            var contentFile3 = CreateMemoryStream(256);
            contentFile3.Position = 0;

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remotePathFile1))
                {
                    client.DeleteFile(remotePathFile1);
                }

                if (client.Exists(remotePathFile2))
                {
                    client.DeleteFile(remotePathFile2);
                }

                if (client.Exists(remotePathFile3))
                {
                    client.DeleteFile(remotePathFile3);
                }

                if (client.Exists(remotePathSubDirectory))
                {
                    client.DeleteDirectory(remotePathSubDirectory);
                }

                if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                {
                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }
                }
            }

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (!client.Exists(remotePath))
                    {
                        client.CreateDirectory(remotePath);
                    }

                    client.UploadFile(contentFile1, remotePathFile1);
                    client.UploadFile(contentFile1, remotePathFile2);

                    client.CreateDirectory(remotePathSubDirectory);
                    client.UploadFile(contentFile3, remotePathFile3);
                }

                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();
                    client.Download(remotePath, new DirectoryInfo(localDirectory));
                }

                var localFiles = Directory.GetFiles(localDirectory);
                Assert.AreEqual(2, localFiles.Length);
                Assert.IsTrue(localFiles.Contains(localPathFile1));
                Assert.IsTrue(localFiles.Contains(localPathFile2));

                var localSubDirecties = Directory.GetDirectories(localDirectory);
                Assert.AreEqual(1, localSubDirecties.Length);
                Assert.AreEqual(localPathSubDirectory, localSubDirecties[0]);

                var localFilesSubDirectory = Directory.GetFiles(localPathSubDirectory);
                Assert.AreEqual(1, localFilesSubDirectory.Length);
                Assert.AreEqual(localPathFile3, localFilesSubDirectory[0]);

                Assert.AreEqual(0, Directory.GetDirectories(localPathSubDirectory).Length);
            }
            finally
            {
                Directory.Delete(localDirectory, true);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(remotePathFile1))
                    {
                        client.DeleteFile(remotePathFile1);
                    }

                    if (client.Exists(remotePathFile2))
                    {
                        client.DeleteFile(remotePathFile2);
                    }

                    if (client.Exists(remotePathFile3))
                    {
                        client.DeleteFile(remotePathFile3);
                    }

                    if (client.Exists(remotePathSubDirectory))
                    {
                        client.DeleteDirectory(remotePathSubDirectory);
                    }

                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (client.Exists(remotePath))
                        {
                            client.DeleteDirectory(remotePath);
                        }
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpDownloadFileInfoDirectoryDoesNotExistData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Download_FileInfo_DirectoryDoesNotExist()
        {
            foreach (var data in GetScpDownloadFileInfoDirectoryDoesNotExistData())
            {
                Scp_Download_FileInfo_DirectoryDoesNotExist((IRemotePathTransformation)data[0],
                                                            (string)data[1],
                                                            (string)data[2]);
            }
        }
#endif
        public void Scp_Download_FileInfo_DirectoryDoesNotExist(IRemotePathTransformation remotePathTransformation,
                                                                string remotePath,
                                                                string remoteFile)
        {
            var completeRemotePath = CombinePaths(remotePath, remoteFile);

            // remove complete directory if it's not the home directory of the user
            // or else remove the remote file
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                {
                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }
                }
            }

            var fileInfo = new FileInfo(Path.GetTempFileName());

            try
            {
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Download(completeRemotePath, fileInfo);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {completeRemotePath}: No such file or directory", ex.Message);
                    }
                }
            }
            finally
            {
                fileInfo.Delete();

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(completeRemotePath))
                    {
                        client.DeleteFile(completeRemotePath);
                    }

                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (client.Exists(remotePath))
                        {
                            client.DeleteDirectory(remotePath);
                        }
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpDownloadFileInfoFileDoesNotExistData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Download_FileInfo_FileDoesNotExist()
        {
            foreach (var data in GetScpDownloadFileInfoFileDoesNotExistData())
            {
                Scp_Download_FileInfo_FileDoesNotExist((IRemotePathTransformation)data[0],
                                                        (string)data[1],
                                                        (string)data[2]);
            }
        }
#endif
        public void Scp_Download_FileInfo_FileDoesNotExist(IRemotePathTransformation remotePathTransformation,
                                                           string remotePath,
                                                           string remoteFile)
        {
            var completeRemotePath = CombinePaths(remotePath, remoteFile);

            // remove complete directory if it's not the home directory of the user
            // or else remove the remote file
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                {
                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }

                    client.CreateDirectory(remotePath);
                }
            }

            var fileInfo = new FileInfo(Path.GetTempFileName());

            try
            {
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Download(completeRemotePath, fileInfo);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {completeRemotePath}: No such file or directory", ex.Message);
                    }
                }
            }
            finally
            {
                fileInfo.Delete();

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(completeRemotePath))
                    {
                        client.DeleteFile(completeRemotePath);
                    }

                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (client.Exists(remotePath))
                        {
                            client.DeleteDirectory(remotePath);
                        }
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpDownloadFileInfoExistingDirectoryData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Download_FileInfo_ExistingDirectory()
        {
            foreach (var data in GetScpDownloadFileInfoExistingDirectoryData())
            {
                Scp_Download_FileInfo_ExistingDirectory((IRemotePathTransformation)data[0],
                                                        (string)data[1]);
            }
        }
#endif
        public void Scp_Download_FileInfo_ExistingDirectory(IRemotePathTransformation remotePathTransformation,
                                                            string remotePath)
        {
            // remove complete directory if it's not the home directory of the user
            // or else remove the remote file
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                {
                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }

                    client.CreateDirectory(remotePath);
                }
            }

            var fileInfo = new FileInfo(Path.GetTempFileName());
            fileInfo.Delete();

            try
            {
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Download(remotePath, fileInfo);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {remotePath}: not a regular file", ex.Message);
                    }

                    Assert.IsFalse(fileInfo.Exists);
                }
            }
            finally
            {
                fileInfo.Delete();

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (client.Exists(remotePath))
                        {
                            client.DeleteDirectory(remotePath);
                        }
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpDownloadFileInfoExistingFileData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Download_FileInfo_ExistingFile()
        {
            foreach (var data in GetScpDownloadFileInfoExistingFileData())
            {
                Scp_Download_FileInfo_ExistingFile((IRemotePathTransformation)data[0],
                                                        (string)data[1],
                                                        (string)data[2],
                                                        (int)data[3]);
            }
        }
#endif
        public void Scp_Download_FileInfo_ExistingFile(IRemotePathTransformation remotePathTransformation,
                                                       string remotePath,
                                                       string remoteFile,
                                                       int size)
        {
            var completeRemotePath = CombinePaths(remotePath, remoteFile);

            // remove complete directory if it's not the home directory of the user
            // or else remove the remote file
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(completeRemotePath))
                {
                    client.DeleteFile(completeRemotePath);
                }

                if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                {
                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }

                    client.CreateDirectory(remotePath);
                }
            }

            var fileInfo = new FileInfo(Path.GetTempFileName());

            try
            {
                var content = CreateMemoryStream(size);
                content.Position = 0;

                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();
                    client.Upload(content, completeRemotePath);
                }

                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();
                    client.Download(completeRemotePath, fileInfo);
                }

                using (var fs = fileInfo.OpenRead())
                {
                    var downloadedBytes = new byte[fs.Length];
                    Assert.AreEqual(downloadedBytes.Length, fs.Read(downloadedBytes, 0, downloadedBytes.Length));
                    content.Position = 0;
                    Assert.AreEqual(CreateHash(content), CreateHash(downloadedBytes));
                }
            }
            finally
            {
                fileInfo.Delete();

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(completeRemotePath))
                    {
                        client.DeleteFile(completeRemotePath);
                    }

                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (client.Exists(remotePath))
                        {
                            client.DeleteDirectory(remotePath);
                        }
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpDownloadStreamExistingDirectoryData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Download_Stream_ExistingDirectory()
        {
            foreach (var data in GetScpDownloadStreamExistingDirectoryData())
            {
                Scp_Download_Stream_ExistingDirectory((IRemotePathTransformation)data[0],
                                                      (string)data[1]);
            }
        }
#endif
        public void Scp_Download_Stream_ExistingDirectory(IRemotePathTransformation remotePathTransformation,
                                                          string remotePath)
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                {
                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }

                    client.CreateDirectory(remotePath);
                }
            }

            var file = Path.GetTempFileName();
            File.Delete(file);

            try
            {
                using (var fs = File.OpenWrite(file))
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Download(remotePath, fs);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {remotePath}: not a regular file", ex.Message);
                    }

                    Assert.AreEqual(0, fs.Length);
                }
            }
            finally
            {
                File.Delete(file);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (client.Exists(remotePath))
                        {
                            client.DeleteDirectory(remotePath);
                        }
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpDownloadStreamExistingFileData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Download_Stream_ExistingFile()
        {
            foreach (var data in GetScpDownloadStreamExistingFileData())
            {
                Scp_Download_Stream_ExistingFile((IRemotePathTransformation)data[0],
                                                 (string)data[1],
                                                 (string)data[2],
                                                 (int)data[3]);
            }
        }
#endif
        public void Scp_Download_Stream_ExistingFile(IRemotePathTransformation remotePathTransformation,
                                                     string remotePath,
                                                     string remoteFile,
                                                     int size)
        {
            var completeRemotePath = CombinePaths(remotePath, remoteFile);

            // remove complete directory if it's not the home directory of the user
            // or else remove the remote file
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(completeRemotePath))
                {
                    client.DeleteFile(completeRemotePath);
                }

                if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                {
                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }

                    client.CreateDirectory(remotePath);
                }
            }

            var file = CreateTempFile(size);

            try
            {
                using (var fs = File.OpenRead(file))
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();
                    client.Upload(fs, completeRemotePath);
                }

                using (var fs = File.OpenRead(file))
                using (var downloaded = new MemoryStream(size))
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    client.Download(completeRemotePath, downloaded);
                    downloaded.Position = 0;
                    Assert.AreEqual(CreateHash(fs), CreateHash(downloaded));
                }
            }
            finally
            {
                File.Delete(file);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(completeRemotePath))
                    {
                        client.DeleteFile(completeRemotePath);
                    }

                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (client.Exists(remotePath))
                        {
                            client.DeleteDirectory(remotePath);
                        }
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpUploadFileStreamDirectoryDoesNotExistData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Upload_FileStream_DirectoryDoesNotExist()
        {
            foreach (var data in GetScpUploadFileStreamDirectoryDoesNotExistData())
            {
                Scp_Upload_FileStream_DirectoryDoesNotExist((IRemotePathTransformation)data[0],
                                                            (string)data[1],
                                                            (string)data[2]);
            }
        }
#endif
        public void Scp_Upload_FileStream_DirectoryDoesNotExist(IRemotePathTransformation remotePathTransformation,
                                                                string remotePath,
                                                                string remoteFile)
        {
            var completeRemotePath = CombinePaths(remotePath, remoteFile);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(completeRemotePath))
                {
                    client.DeleteFile(completeRemotePath);
                }

                if (client.Exists(remotePath))
                {
                    client.DeleteDirectory(remotePath);
                }
            }

            var file = CreateTempFile(1000);

            try
            {
                using (var fs = File.OpenRead(file))
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Upload(fs, completeRemotePath);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {remotePath}: No such file or directory", ex.Message);
                    }
                }
            }
            finally
            {
                File.Delete(file);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(completeRemotePath))
                    {
                        client.DeleteFile(completeRemotePath);
                    }

                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpUploadFileStreamExistingDirectoryData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Upload_FileStream_ExistingDirectory()
        {
            foreach (var data in GetScpUploadFileStreamExistingDirectoryData())
            {
                Scp_Upload_FileStream_ExistingDirectory((IRemotePathTransformation)data[0],
                                                        (string)data[1]);
            }
        }
#endif
        public void Scp_Upload_FileStream_ExistingDirectory(IRemotePathTransformation remotePathTransformation,
                                                            string remoteFile)
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteFile)))
                {
                    command.Execute();
                }
            }

            var file = CreateTempFile(1000);

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();
                    client.CreateDirectory(remoteFile);
                }

                using (var fs = File.OpenRead(file))
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Upload(fs, remoteFile);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {remoteFile}: Is a directory", ex.Message);
                    }
                }
            }
            finally
            {
                File.Delete(file);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(remoteFile))
                    {
                        client.DeleteDirectory(remoteFile);
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(ScpUploadFileStreamExistingFileData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Upload_FileStream_ExistingFile()
        {
            foreach (var data in ScpUploadFileStreamExistingFileData())
            {
                Scp_Upload_FileStream_ExistingFile((IRemotePathTransformation)data[0],
                                                   (string)data[1]);
            }
        }
#endif
        public void Scp_Upload_FileStream_ExistingFile(IRemotePathTransformation remotePathTransformation,
                                                       string remoteFile)
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }
            }

            // original content is bigger than new content to ensure file is fully overwritten
            var originalContent = CreateMemoryStream(2000);
            var file = CreateTempFile(1000);

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    originalContent.Position = 0;
                    client.UploadFile(originalContent, remoteFile);
                }

                using (var fs = File.OpenRead(file))
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();
                    client.Upload(fs, remoteFile);
                }

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    using (var downloaded = new MemoryStream(1000))
                    {
                        client.DownloadFile(remoteFile, downloaded);
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateFileHash(file), CreateHash(downloaded));
                    }
                }
            }
            finally
            {
                File.Delete(file);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpUploadFileStreamFileDoesNotExistData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Upload_FileStream_FileDoesNotExist()
        {
            foreach (var data in GetScpUploadFileStreamFileDoesNotExistData())
            {
                Scp_Upload_FileStream_FileDoesNotExist((IRemotePathTransformation)data[0],
                                                       (string)data[1],
                                                       (string)data[2],
                                                       (int)data[3]);
            }
        }
#endif
        public void Scp_Upload_FileStream_FileDoesNotExist(IRemotePathTransformation remotePathTransformation,
                                                           string remotePath,
                                                           string remoteFile,
                                                           int size)
        {
            var completeRemotePath = CombinePaths(remotePath, remoteFile);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(completeRemotePath))
                {
                    client.DeleteFile(completeRemotePath);
                }

                // remove complete directory if it's not the home directory of the user
                if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                {
                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }
                }
            }

            var file = CreateTempFile(size);

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    // create directory if it's not the home directory of the user
                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (!client.Exists((remotePath)))
                        {
                            client.CreateDirectory(remotePath);
                        }
                    }
                }

                using (var fs = File.OpenRead(file))
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();
                    client.Upload(fs, completeRemotePath);
                }

                using (var fs = File.OpenRead(file))
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    var sftpFile = client.Get(completeRemotePath);
                    Assert.AreEqual(GetAbsoluteRemotePath(client, remotePath, remoteFile), sftpFile.FullName);
                    Assert.AreEqual(size, sftpFile.Length);

                    var downloaded = client.ReadAllBytes(completeRemotePath);
                    Assert.AreEqual(CreateHash(fs), CreateHash(downloaded));
                }
            }
            finally
            {
                File.Delete(file);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(completeRemotePath))
                    {
                        client.DeleteFile(completeRemotePath);
                    }

                    // remove complete directory if it's not the home directory of the user
                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (client.Exists(remotePath))
                        {
                            client.DeleteDirectory(remotePath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// https://github.com/sshnet/SSH.NET/issues/289
        /// </summary>
#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpUploadFileInfoDirectoryDoesNotExistData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Upload_FileInfo_DirectoryDoesNotExist()
        {
            foreach (var data in GetScpUploadFileInfoDirectoryDoesNotExistData())
            {
                Scp_Upload_FileInfo_DirectoryDoesNotExist((IRemotePathTransformation)data[0],
                                                          (string)data[1],
                                                          (string)data[2]);
            }
        }
#endif
        public void Scp_Upload_FileInfo_DirectoryDoesNotExist(IRemotePathTransformation remotePathTransformation,
                                                              string remotePath,
                                                              string remoteFile)
        {
            var completeRemotePath = CombinePaths(remotePath, remoteFile);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(completeRemotePath))
                {
                    client.DeleteFile(completeRemotePath);
                }

                if (client.Exists(remotePath))
                {
                    client.DeleteDirectory(remotePath);
                }
            }

            var file = CreateTempFile(1000);

            try
            {
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Upload(new FileInfo(file), completeRemotePath);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {remotePath}: No such file or directory", ex.Message);
                    }
                }

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    Assert.IsFalse(client.Exists(completeRemotePath));
                    Assert.IsFalse(client.Exists(remotePath));
                }
            }
            finally
            {
                File.Delete(file);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(completeRemotePath))
                    {
                        client.DeleteFile(completeRemotePath);
                    }

                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }
                }
            }
        }

        /// <summary>
        /// https://github.com/sshnet/SSH.NET/issues/286
        /// </summary>
#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpUploadFileInfoExistingDirectoryData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Upload_FileInfo_ExistingDirectory()
        {
            foreach (var data in GetScpUploadFileInfoExistingDirectoryData())
            {
                Scp_Upload_FileInfo_ExistingDirectory((IRemotePathTransformation)data[0],
                                                      (string)data[1]);
            }
        }
#endif
        public void Scp_Upload_FileInfo_ExistingDirectory(IRemotePathTransformation remotePathTransformation,
                                                          string remoteFile)
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteFile)))
                {
                    command.Execute();
                }
            }

            var file = CreateTempFile(1000);

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();
                    client.CreateDirectory(remoteFile);
                }

                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Upload(new FileInfo(file), remoteFile);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {remoteFile}: Is a directory", ex.Message);
                    }
                }
            }
            finally
            {
                File.Delete(file);

                using (var client = new SshClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteFile)))
                    {
                        command.Execute();
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpUploadFileInfoExistingFileData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Upload_FileInfo_ExistingFile()
        {
            foreach (var data in GetScpUploadFileInfoExistingFileData())
            {
                Scp_Upload_FileInfo_ExistingFile((IRemotePathTransformation)data[0],
                                                 (string)data[1]);
            }
        }
#endif
        public void Scp_Upload_FileInfo_ExistingFile(IRemotePathTransformation remotePathTransformation,
                                                     string remoteFile)
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }
            }

            // original content is bigger than new content to ensure file is fully overwritten
            var originalContent = CreateMemoryStream(2000);
            var file = CreateTempFile(1000);

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    originalContent.Position = 0;
                    client.UploadFile(originalContent, remoteFile);
                }

                var fileInfo = new FileInfo(file)
                    {
                        LastAccessTimeUtc = new DateTime(1973, 8, 13, 20, 15, 33, DateTimeKind.Utc),
                        LastWriteTimeUtc = new DateTime(1974, 1, 24, 3, 55, 12, DateTimeKind.Utc)
                    };

                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();
                    client.Upload(fileInfo, remoteFile);
                }

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    var uploadedFile = client.Get(remoteFile);
                    Assert.AreEqual(fileInfo.LastAccessTimeUtc, uploadedFile.LastAccessTimeUtc);
                    Assert.AreEqual(fileInfo.LastWriteTimeUtc, uploadedFile.LastWriteTimeUtc);

                    using (var downloaded = new MemoryStream(1000))
                    {
                        client.DownloadFile(remoteFile, downloaded);
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateFileHash(file), CreateHash(downloaded));
                    }
                }
            }
            finally
            {
                File.Delete(file);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(remoteFile))
                    {
                        client.DeleteFile(remoteFile);
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpUploadFileInfoFileDoesNotExistData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Upload_FileInfo_FileDoesNotExist()
        {
            foreach (var data in GetScpUploadFileInfoFileDoesNotExistData())
            {
                Scp_Upload_FileInfo_FileDoesNotExist((IRemotePathTransformation)data[0],
                                                     (string)data[1],
                                                     (string)data[2],
                                                     (int)data[3]);
            }
        }
#endif
        public void Scp_Upload_FileInfo_FileDoesNotExist(IRemotePathTransformation remotePathTransformation,
                                                         string remotePath,
                                                         string remoteFile,
                                                         int size)
        {
            var completeRemotePath = CombinePaths(remotePath, remoteFile);

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(completeRemotePath))
                {
                    client.DeleteFile(completeRemotePath);
                }

                // remove complete directory if it's not the home directory of the user
                if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                {
                    if (client.Exists(remotePath))
                    {
                        client.DeleteDirectory(remotePath);
                    }
                }
            }

            var file = CreateTempFile(size);

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    // create directory if it's not the home directory of the user
                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (!client.Exists(remotePath))
                        {
                            client.CreateDirectory(remotePath);
                        }
                    }
                }

                var fileInfo = new FileInfo(file)
                    {
                        LastAccessTimeUtc = new DateTime(1973, 8, 13, 20, 15, 33, DateTimeKind.Utc),
                        LastWriteTimeUtc = new DateTime(1974, 1, 24, 3, 55, 12, DateTimeKind.Utc)
                    };

                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();
                    client.Upload(fileInfo, completeRemotePath);
                }

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    var uploadedFile = client.Get(completeRemotePath);
                    Assert.AreEqual(fileInfo.LastAccessTimeUtc, uploadedFile.LastAccessTimeUtc);
                    Assert.AreEqual(fileInfo.LastWriteTimeUtc, uploadedFile.LastWriteTimeUtc);
                    Assert.AreEqual(size, uploadedFile.Length);

                    using (var downloaded = new MemoryStream(size))
                    {
                        client.DownloadFile(completeRemotePath, downloaded);
                        downloaded.Position = 0;
                        Assert.AreEqual(CreateFileHash(file), CreateHash(downloaded));
                    }
                }
            }
            finally
            {
                File.Delete(file);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(completeRemotePath))
                    {
                        client.Delete(completeRemotePath);
                    }

                    // remove complete directory if it's not the home directory of the user
                    if (remotePath.Length > 0 && remotePath != client.WorkingDirectory)
                    {
                        if (client.Exists(remotePath))
                        {
                            client.DeleteDirectory(remotePath);
                        }
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpUploadDirectoryInfoDirectoryDoesNotExistData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Upload_DirectoryInfo_DirectoryDoesNotExist()
        {
            foreach (var data in GetScpUploadDirectoryInfoDirectoryDoesNotExistData())
            {
                Scp_Upload_DirectoryInfo_DirectoryDoesNotExist((IRemotePathTransformation)data[0],
                                                               (string)data[1]);
            }
        }
#endif
        public void Scp_Upload_DirectoryInfo_DirectoryDoesNotExist(IRemotePathTransformation remotePathTransformation,
                                                                   string remoteDirectory)
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists((remoteDirectory)))
                {
                    client.DeleteDirectory(remoteDirectory);
                }
            }

            var localDirectory = Path.GetTempFileName();
            File.Delete(localDirectory);
            Directory.CreateDirectory(localDirectory);

            try
            {
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    try
                    {
                        client.Upload(new DirectoryInfo(localDirectory), remoteDirectory);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {remoteDirectory}: No such file or directory", ex.Message);
                    }
                }

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    Assert.IsFalse(client.Exists(remoteDirectory));
                }
            }
            finally
            {
                Directory.Delete(localDirectory, true);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists((remoteDirectory)))
                    {
                        client.DeleteDirectory(remoteDirectory);
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpUploadDirectoryInfoExistingDirectoryData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Upload_DirectoryInfo_ExistingDirectory()
        {
            foreach (var data in GetScpUploadDirectoryInfoExistingDirectoryData())
            {
                Scp_Upload_DirectoryInfo_ExistingDirectory((IRemotePathTransformation)data[0],
                                                           (string)data[1]);
            }
        }
#endif
        public void Scp_Upload_DirectoryInfo_ExistingDirectory(IRemotePathTransformation remotePathTransformation,
                                                               string remoteDirectory)
        {
            string absoluteRemoteDirectory = GetAbsoluteRemotePath(_connectionInfoFactory, remoteDirectory);

            var remotePathFile1 = CombinePaths(remoteDirectory, "file1");
            var remotePathFile2 = CombinePaths(remoteDirectory, "file2");

            var absoluteremoteSubDirectory1 = CombinePaths(absoluteRemoteDirectory, "sub1");
            var remoteSubDirectory1 = CombinePaths(remoteDirectory, "sub1");
            var remotePathSubFile1 = CombinePaths(remoteSubDirectory1, "file1");
            var remotePathSubFile2 = CombinePaths(remoteSubDirectory1, "file2");
            var absoluteremoteSubDirectory2 = CombinePaths(absoluteRemoteDirectory, "sub2");
            var remoteSubDirectory2 = CombinePaths(remoteDirectory, "sub2");

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remotePathSubFile1))
                {
                    client.DeleteFile(remotePathSubFile1);
                }
                if (client.Exists(remotePathSubFile2))
                {
                    client.DeleteFile(remotePathSubFile2);
                }
                if (client.Exists(remoteSubDirectory1))
                {
                    client.DeleteDirectory(remoteSubDirectory1);
                }
                if (client.Exists(remoteSubDirectory2))
                {
                    client.DeleteDirectory(remoteSubDirectory2);
                }
                if (client.Exists(remotePathFile1))
                {
                    client.DeleteFile(remotePathFile1);
                }
                if (client.Exists(remotePathFile2))
                {
                    client.DeleteFile(remotePathFile2);
                }

                if (remoteDirectory.Length > 0 && remoteDirectory != "." && remoteDirectory != client.WorkingDirectory)
                {
                    if (client.Exists(remoteDirectory))
                    {
                        client.DeleteDirectory(remoteDirectory);
                    }

                    client.CreateDirectory(remoteDirectory);
                }
            }

            var localDirectory = Path.GetTempFileName();
            File.Delete(localDirectory);
            Directory.CreateDirectory(localDirectory);

            var localPathFile1 = Path.Combine(localDirectory, "file1");
            var localPathFile2 = Path.Combine(localDirectory, "file2");

            var localSubDirectory1 = Path.Combine(localDirectory, "sub1");
            var localPathSubFile1 = Path.Combine(localSubDirectory1, "file1");
            var localPathSubFile2 = Path.Combine(localSubDirectory1, "file2");
            var localSubDirectory2 = Path.Combine(localDirectory, "sub2");

            try
            {
                CreateFile(localPathFile1, 2000);
                File.SetLastWriteTimeUtc(localPathFile1, new DateTime(2015, 8, 24, 5, 32, 16, DateTimeKind.Utc));

                CreateFile(localPathFile2, 1000);
                File.SetLastWriteTimeUtc(localPathFile2, new DateTime(2012, 3, 27, 23, 2, 54, DateTimeKind.Utc));

                // create subdirectory with two files
                Directory.CreateDirectory(localSubDirectory1);
                CreateFile(localPathSubFile1, 1000);
                File.SetLastWriteTimeUtc(localPathSubFile1, new DateTime(2013, 4, 12, 16, 54, 22, DateTimeKind.Utc));
                CreateFile(localPathSubFile2, 2000);
                File.SetLastWriteTimeUtc(localPathSubFile2, new DateTime(2015, 8, 4, 12, 43, 12, DateTimeKind.Utc));
                Directory.SetLastWriteTimeUtc(localSubDirectory1,
                                              new DateTime(2014, 6, 12, 13, 2, 44, DateTimeKind.Utc));

                // create empty subdirectory
                Directory.CreateDirectory(localSubDirectory2);
                Directory.SetLastWriteTimeUtc(localSubDirectory2,
                                              new DateTime(2011, 5, 14, 1, 5, 12, DateTimeKind.Utc));

                Directory.SetLastWriteTimeUtc(localDirectory, new DateTime(2015, 10, 14, 22, 45, 11, DateTimeKind.Utc));

                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();
                    client.Upload(new DirectoryInfo(localDirectory), remoteDirectory);
                }

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    Assert.IsTrue(client.Exists(remoteDirectory));

                    var remoteSftpDirectory = client.Get(remoteDirectory);
                    Assert.IsNotNull(remoteSftpDirectory);
                    Assert.AreEqual(absoluteRemoteDirectory, remoteSftpDirectory.FullName);
                    Assert.IsTrue(remoteSftpDirectory.IsDirectory);
                    Assert.IsFalse(remoteSftpDirectory.IsRegularFile);

                    // Due to CVE-2018-20685, we can no longer set the times or modes on a file or directory
                    // that refers to the current directory ('.'), the parent directory ('..') or a directory
                    // containing a forward slash ('/').
                    Assert.AreNotEqual(Directory.GetLastWriteTimeUtc(localDirectory), remoteSftpDirectory.LastWriteTimeUtc);

                    Assert.IsTrue(client.Exists(remotePathFile1));
                    Assert.AreEqual(CreateFileHash(localPathFile1), CreateRemoteFileHash(client, remotePathFile1));
                    var remoteSftpFile = client.Get(remotePathFile1);
                    Assert.IsNotNull(remoteSftpFile);
                    Assert.IsFalse(remoteSftpFile.IsDirectory);
                    Assert.IsTrue(remoteSftpFile.IsRegularFile);
                    Assert.AreEqual(File.GetLastWriteTimeUtc(localPathFile1), remoteSftpFile.LastWriteTimeUtc);

                    Assert.IsTrue(client.Exists(remotePathFile2));
                    Assert.AreEqual(CreateFileHash(localPathFile2), CreateRemoteFileHash(client, remotePathFile2));
                    remoteSftpFile = client.Get(remotePathFile2);
                    Assert.IsNotNull(remoteSftpFile);
                    Assert.IsFalse(remoteSftpFile.IsDirectory);
                    Assert.IsTrue(remoteSftpFile.IsRegularFile);
                    Assert.AreEqual(File.GetLastWriteTimeUtc(localPathFile2), remoteSftpFile.LastWriteTimeUtc);

                    Assert.IsTrue(client.Exists(remoteSubDirectory1));
                    remoteSftpDirectory = client.Get(remoteSubDirectory1);
                    Assert.IsNotNull(remoteSftpDirectory);
                    Assert.AreEqual(absoluteremoteSubDirectory1, remoteSftpDirectory.FullName);
                    Assert.IsTrue(remoteSftpDirectory.IsDirectory);
                    Assert.IsFalse(remoteSftpDirectory.IsRegularFile);
                    Assert.AreEqual(Directory.GetLastWriteTimeUtc(localSubDirectory1), remoteSftpDirectory.LastWriteTimeUtc);

                    Assert.IsTrue(client.Exists(remotePathSubFile1));
                    Assert.AreEqual(CreateFileHash(localPathSubFile1), CreateRemoteFileHash(client, remotePathSubFile1));

                    Assert.IsTrue(client.Exists(remotePathSubFile2));
                    Assert.AreEqual(CreateFileHash(localPathSubFile2), CreateRemoteFileHash(client, remotePathSubFile2));

                    Assert.IsTrue(client.Exists(remoteSubDirectory2));
                    remoteSftpDirectory = client.Get(remoteSubDirectory2);
                    Assert.IsNotNull(remoteSftpDirectory);
                    Assert.AreEqual(absoluteremoteSubDirectory2, remoteSftpDirectory.FullName);
                    Assert.IsTrue(remoteSftpDirectory.IsDirectory);
                    Assert.IsFalse(remoteSftpDirectory.IsRegularFile);
                    Assert.AreEqual(Directory.GetLastWriteTimeUtc(localSubDirectory2), remoteSftpDirectory.LastWriteTimeUtc);
                }
            }
            finally
            {
                Directory.Delete(localDirectory, true);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(remotePathSubFile1))
                    {
                        client.DeleteFile(remotePathSubFile1);
                    }
                    if (client.Exists(remotePathSubFile2))
                    {
                        client.DeleteFile(remotePathSubFile2);
                    }
                    if (client.Exists(remoteSubDirectory1))
                    {
                        client.DeleteDirectory(remoteSubDirectory1);
                    }
                    if (client.Exists(remoteSubDirectory2))
                    {
                        client.DeleteDirectory(remoteSubDirectory2);
                    }
                    if (client.Exists(remotePathFile1))
                    {
                        client.DeleteFile(remotePathFile1);
                    }
                    if (client.Exists(remotePathFile2))
                    {
                        client.DeleteFile(remotePathFile2);
                    }

                    if (remoteDirectory.Length > 0 && remoteDirectory != "." && remoteDirectory != client.WorkingDirectory)
                    {
                        if (client.Exists(remoteDirectory))
                        {
                            client.DeleteDirectory(remoteDirectory);
                        }
                    }
                }
            }
        }

#if FEATURE_MSTEST_DATATEST
        [DataTestMethod]
        [DynamicData(nameof(GetScpUploadDirectoryInfoExistingFileData), DynamicDataSourceType.Method)]
#else
        [TestMethod]
        public void Scp_Upload_DirectoryInfo_ExistingFile()
        {
            foreach (var data in GetScpUploadDirectoryInfoExistingFileData())
            {
                Scp_Upload_DirectoryInfo_ExistingFile((IRemotePathTransformation)data[0],
                                                      (string)data[1]);
            }
        }
#endif
        public void Scp_Upload_DirectoryInfo_ExistingFile(IRemotePathTransformation remotePathTransformation,
                                                          string remoteDirectory)
        {
            var remotePathFile1 = CombinePaths(remoteDirectory, "file1");
            var remotePathFile2 = CombinePaths(remoteDirectory, "file2");

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                Console.WriteLine(client.ConnectionInfo.CurrentKeyExchangeAlgorithm);

                using (var command = client.CreateCommand("rm -Rf " + _remotePathTransformation.Transform(remoteDirectory)))
                {
                    command.Execute();
                }
            }

            var localDirectory = Path.GetTempFileName();
            File.Delete(localDirectory);
            Directory.CreateDirectory(localDirectory);

            var localPathFile1 = Path.Combine(localDirectory, "file1");
            var localPathFile2 = Path.Combine(localDirectory, "file2");

            try
            {
                CreateFile(localPathFile1, 50);
                CreateFile(localPathFile2, 50);

                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    if (remotePathTransformation != null)
                    {
                        client.RemotePathTransformation = remotePathTransformation;
                    }

                    client.Connect();

                    CreateRemoteFile(client, remoteDirectory, 10);

                    try
                    {
                        client.Upload(new DirectoryInfo(localDirectory), remoteDirectory);
                        Assert.Fail();
                    }
                    catch (ScpException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual($"scp: {remoteDirectory}: Not a directory", ex.Message);
                    }
                }
            }
            finally
            {
                Directory.Delete(localDirectory, true);

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    if (client.Exists(remotePathFile1))
                    {
                        client.DeleteFile(remotePathFile1);
                    }
                    if (client.Exists(remotePathFile2))
                    {
                        client.DeleteFile(remotePathFile2);
                    }
                    if (client.Exists((remoteDirectory)))
                    {
                        client.DeleteFile(remoteDirectory);
                    }
                }
            }
        }

        private static IEnumerable<object[]> GetScpDownloadStreamDirectoryDoesNotExistData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-directorydoesnotexist", "scp-file" };
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-directorydoesnotexist", "scp-file" };
        }

        private static IEnumerable<object[]> GetScpUploadFileInfoFileDoesNotExistData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet", "test123", 0 };
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet", "test123", 5 * 1024 * 1024 };
            yield return new object[] { RemotePathTransformation.ShellQuote, "/home/sshnet/dir|&;<>()$`\"'sp\u0100ce \\tab\tlf\n*?[#~=%", "file123", 1024 };
            yield return new object[] { null, "/home/sshnet/scp test", "file 123", 1024 };
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-test", "file|&;<>()$`\"'sp\u0100ce \\tab\tlf*?[#~=%", 1024 };
            yield return new object[] { null, "", "scp-issue280", 1024 };
        }

        private static IEnumerable<object[]> GetScpUploadFileStreamFileDoesNotExistData()
        {
            yield return new object[] { RemotePathTransformation.ShellQuote, "/home/sshnet/dir|&;<>()$`\"'sp\u0100ce \\tab\tlf\n*?[#~=%", "file123", 0 };
            yield return new object[] { RemotePathTransformation.ShellQuote, "/home/sshnet/dir|&;<>()$`\"'sp\u0100ce \\tab\tlf\n*?[#~=%", "file123", 1024 };
            yield return new object[] { null, "/home/sshnet/scp test", "file 123", 1024 };
            yield return new object[] { RemotePathTransformation.ShellQuote, "/home/sshnet/scp-test", "file|&;<>()$`\"'sp\u0100ce \\tab\tlf*?[#~=%", 1024 };
            yield return new object[] { RemotePathTransformation.None, "", "scp-issue280", 1024 };
        }

        private static IEnumerable<object[]> GetScpUploadDirectoryInfoExistingDirectoryData()
        {
            yield return new object[] { RemotePathTransformation.None, "scp-directorydoesnotexist" };
            yield return new object[] { RemotePathTransformation.None, "." };
            yield return new object[] { RemotePathTransformation.ShellQuote, "/home/sshnet/dir|&;<>()$`\"'sp\u0100ce \\tab\tlf*?[#~=%" };
        }

        private static IEnumerable<object[]> GetScpUploadDirectoryInfoExistingFileData()
        {
            yield return new object[] { RemotePathTransformation.None, "scp-upload-file" };
        }

        private static IEnumerable<object[]> ScpUploadFileStreamExistingFileData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-upload-file" };
        }

        private static IEnumerable<object[]> GetScpDownloadStreamFileDoesNotExistData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet", "scp-filedoesnotexist" };
        }

        private static IEnumerable<object[]> GetScpDownloadDirectoryInfoDirectoryDoesNotExistData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-download" };
        }

        private static IEnumerable<object[]> GetScpDownloadDirectoryInfoExistingFileData()
        {
            yield return new object[] { RemotePathTransformation.None, "scp-download" };
        }

        private static IEnumerable<object[]> GetScpDownloadDirectoryInfoExistingDirectoryData()
        {
            yield return new object[] { RemotePathTransformation.None, "scp-download" };
            yield return new object[] { RemotePathTransformation.ShellQuote, "/home/sshnet/dir|&;<>()$`\"'space \\tab\tlf*?[#~=%" };
        }

        private static IEnumerable<object[]> GetScpDownloadFileInfoDirectoryDoesNotExistData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-directorydoesnotexist", "scp-file" };
        }

        private static IEnumerable<object[]> GetScpDownloadFileInfoFileDoesNotExistData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet", "scp-filedoesnotexist" };
        }

        private static IEnumerable<object[]> GetScpDownloadFileInfoExistingDirectoryData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-test" };
        }

        private static IEnumerable<object[]> GetScpDownloadFileInfoExistingFileData()
        {
            yield return new object[] { null, "", "file 123", 0 };
            yield return new object[] { null, "", "file 123", 1024 };
            yield return new object[] { RemotePathTransformation.ShellQuote, "", "file|&;<>()$`\"'sp\u0100ce \\tab\tlf*?[#~=%", 1024 };
            yield return new object[] { null, "/home/sshnet/scp test", "file 123", 1024 };
            yield return new object[] { RemotePathTransformation.ShellQuote, "/home/sshnet/dir|&;<>()$`\"'sp\u0100ce \\tab\tlf\n*?[#~=%", "file123", 1024 };
            yield return new object[] { RemotePathTransformation.ShellQuote, "/home/sshnet/scp-test", "file|&;<>()$`\"'sp\u0100ce \\tab\tlf*?[#~=%", 1024 };
        }

        private static IEnumerable<object[]> GetScpDownloadStreamExistingDirectoryData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-test" };
        }

        private static IEnumerable<object[]> GetScpDownloadStreamExistingFileData()
        {
            yield return new object[] { null, "", "file 123", 0 };
            yield return new object[] { null, "", "file 123", 1024 };
            yield return new object[] { RemotePathTransformation.ShellQuote, "", "file|&;<>()$`\"'sp\u0100ce \\tab\tlf*?[#~=%", 1024 };
            yield return new object[] { null, "/home/sshnet/scp test", "file 123", 1024 };
            yield return new object[] { RemotePathTransformation.ShellQuote, "/home/sshnet/dir|&;<>()$`\"'sp\u0100ce \\tab\tlf\n*?[#~=%", "file123", 1024 };
            yield return new object[] { RemotePathTransformation.ShellQuote, "/home/sshnet/scp-test", "file|&;<>()$`\"'sp\u0100ce \\tab\tlf*?[#~=%", 1024 };
        }

        private static IEnumerable<object[]> GetScpUploadFileStreamDirectoryDoesNotExistData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-issue289", "file123" };
        }

        private static IEnumerable<object[]> GetScpUploadFileStreamExistingDirectoryData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-issue286" };
        }

        private static IEnumerable<object[]> GetScpUploadFileInfoDirectoryDoesNotExistData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-issue289", "file123" };
        }

        private static IEnumerable<object[]> GetScpUploadFileInfoExistingDirectoryData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-issue286" };
        }

        private static IEnumerable<object[]> GetScpUploadFileInfoExistingFileData()
        {
            yield return new object[] { RemotePathTransformation.None, "/home/sshnet/scp-upload-file" };
        }

        private static IEnumerable<object[]> GetScpUploadDirectoryInfoDirectoryDoesNotExistData()
        {
            yield return new object[] { RemotePathTransformation.None, "scp-directorydoesnotexist" };
        }

        private static void CreateRemoteFile(ScpClient client, string remoteFile, int size)
        {
            var file = CreateTempFile(size);

            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    client.Upload(fs, remoteFile);
                }
            }
            finally
            {
                File.Delete(file);
            }
        }

        private static string GetAbsoluteRemotePath(SftpClient client, string directoryName, string fileName)
        {
            var absolutePath = string.Empty;

            if (directoryName.Length == 0)
            {
                absolutePath += client.WorkingDirectory;
            }
            else
            {
                if (directoryName[0] != '/')
                {
                    absolutePath += client.WorkingDirectory + "/" + directoryName;
                }
                else
                {
                    absolutePath = directoryName;
                }
            }

            return absolutePath + "/" + fileName;
        }

        private static string GetAbsoluteRemotePath(IConnectionInfoFactory connectionInfoFactory, string directoryName)
        {
            var absolutePath = string.Empty;

            if (directoryName.Length == 0 || directoryName == ".")
            {
                using (var client = new SftpClient(connectionInfoFactory.Create()))
                {
                    client.Connect();

                    absolutePath += client.WorkingDirectory;
                }
            }
            else
            {
                if (directoryName[0] != '/')
                {
                    using (var client = new SftpClient(connectionInfoFactory.Create()))
                    {
                        client.Connect();

                        absolutePath += client.WorkingDirectory + "/" + directoryName;
                    }
                }
                else
                {
                    absolutePath = directoryName;
                }
            }

            return absolutePath;
        }

        private static string CreateRemoteFileHash(SftpClient client, string remotePath)
        {
            using (var fs = client.OpenRead(remotePath))
            {
                return CreateHash(fs);
            }
        }

        private static string CombinePaths(string path1, string path2)
        {
            if (path1.Length == 0)
            {
                return path2;
            }

            if (path2.Length == 0)
            {
                return path1;
            }

            return path1 + "/" + path2;
        }
    }
}

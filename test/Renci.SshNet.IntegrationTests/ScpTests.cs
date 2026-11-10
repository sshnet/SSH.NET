using Renci.SshNet.Common;

#pragma warning disable CS0618 // These SCP tests use the obsolete default-transformation constructors.

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

        [TestMethod]
        [DynamicData(nameof(GetScpDownloadStreamDirectoryDoesNotExistData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpDownloadStreamFileDoesNotExistData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpDownloadDirectoryInfoDirectoryDoesNotExistData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpDownloadDirectoryInfoExistingFileData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpDownloadDirectoryInfoExistingDirectoryData))]
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
                Assert.HasCount(2, localFiles);
                Assert.IsTrue(localFiles.Contains(localPathFile1));
                Assert.IsTrue(localFiles.Contains(localPathFile2));

                var localSubDirecties = Directory.GetDirectories(localDirectory);
                Assert.HasCount(1, localSubDirecties);
                Assert.AreEqual(localPathSubDirectory, localSubDirecties[0]);

                var localFilesSubDirectory = Directory.GetFiles(localPathSubDirectory);
                Assert.HasCount(1, localFilesSubDirectory);
                Assert.AreEqual(localPathFile3, localFilesSubDirectory[0]);

                Assert.IsEmpty(Directory.GetDirectories(localPathSubDirectory));
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

        [TestMethod]
        [DynamicData(nameof(GetScpDownloadFileInfoDirectoryDoesNotExistData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpDownloadFileInfoFileDoesNotExistData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpDownloadFileInfoExistingDirectoryData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpDownloadFileInfoExistingFileData))]
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

            // Create a local file larger than the remote file in order to test truncation.
            var fileInfo = new FileInfo(CreateTempFile(size + 64));

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

        [TestMethod]
        [DynamicData(nameof(GetScpDownloadStreamExistingDirectoryData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpDownloadStreamExistingFileData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpUploadFileStreamDirectoryDoesNotExistData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpUploadFileStreamExistingDirectoryData))]
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

        [TestMethod]
        [DynamicData(nameof(ScpUploadFileStreamExistingFileData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpUploadFileStreamFileDoesNotExistData))]
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
        [TestMethod]
        [DynamicData(nameof(GetScpUploadFileInfoDirectoryDoesNotExistData))]
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
        [TestMethod]
        [DynamicData(nameof(GetScpUploadFileInfoExistingDirectoryData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpUploadFileInfoExistingFileData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpUploadFileInfoFileDoesNotExistData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpUploadDirectoryInfoDirectoryDoesNotExistData))]
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

        [TestMethod]
        [DynamicData(nameof(GetScpUploadDirectoryInfoExistingDirectoryData))]
        public void Scp_Upload_DirectoryInfo_ExistingDirectory(IRemotePathTransformation remotePathTransformation,
                                                               string remoteDirectory)
        {
            string absoluteRemoteDirectory = GetAbsoluteRemotePath(_connectionInfoFactory, remoteDirectory);

            var remotePathFile1 = CombinePaths(remoteDirectory, "file1");
            var remotePathFile2 = CombinePaths(remoteDirectory, "file2");

            var absoluteRemoteSubDirectory1 = CombinePaths(absoluteRemoteDirectory, "sub1");
            var remoteSubDirectory1 = CombinePaths(remoteDirectory, "sub1");
            var remotePathSubFile1 = CombinePaths(remoteSubDirectory1, "file1");
            var remotePathSubFile2 = CombinePaths(remoteSubDirectory1, "file2");
            var absoluteRemoteSubDirectory2 = CombinePaths(absoluteRemoteDirectory, "sub2");
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
                    Assert.AreEqual(absoluteRemoteSubDirectory1, remoteSftpDirectory.FullName);
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
                    Assert.AreEqual(absoluteRemoteSubDirectory2, remoteSftpDirectory.FullName);
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

        [TestMethod]
        [DynamicData(nameof(GetScpUploadDirectoryInfoExistingFileData))]
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

        /// <summary>
        /// A recursive download must never write outside of the destination directory, even
        /// when the server returns a file name containing a path separator. Backslash is a
        /// legal byte in a Unix file name, so a stock OpenSSH server transmits it verbatim;
        /// on a Windows client it would otherwise be interpreted as a directory separator.
        /// </summary>
        [TestMethod]
        public void Scp_Download_DirectoryInfo_ServerNameWithSeparator_StaysInsideDestination()
        {
            var remoteDirectory = "/tmp/sshnet-scp-guard-" + Guid.NewGuid().ToString("N");

            // Set up a remote directory containing a normal file and a file whose name
            // contains a backslash (a single, valid Unix file name).
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                _ = client.RunCommand("mkdir -p '" + remoteDirectory + "'");
                _ = client.RunCommand("printf '%s' good > '" + remoteDirectory + "/good.txt'");
                _ = client.RunCommand("printf '%s' ESCAPED > '" + remoteDirectory + "/..\\owned.txt'");
            }

            var localRoot = Path.GetTempFileName();
            File.Delete(localRoot);
            _ = Directory.CreateDirectory(localRoot);

            var destination = Path.Combine(localRoot, "download");
            _ = Directory.CreateDirectory(destination);

            // Where "..\owned.txt", combined with the destination, would land on a Windows client.
            var escapedFile = Path.Combine(localRoot, "owned.txt");

            try
            {
                using (var client = new ScpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    try
                    {
                        client.Download(remoteDirectory, new DirectoryInfo(destination));
                    }
                    catch (ScpException)
                    {
                        // Expected on platforms where '\' is a directory separator: the
                        // download is aborted rather than allowed to escape.
                    }
                }

                // The security invariant, asserted on every platform: nothing is written
                // outside of the caller-supplied destination directory.
                Assert.IsFalse(File.Exists(escapedFile),
                    "A file was written outside of the destination directory: " + escapedFile);
            }
            finally
            {
                using (var client = new SshClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();
                    _ = client.RunCommand("rm -rf '" + remoteDirectory + "'");
                }

                Directory.Delete(localRoot, recursive: true);
            }
        }

        /// <summary>
        /// A recursive download must never write outside of the destination directory when the
        /// remote tree contains a directory named "...". It is an ordinary name on the (Linux)
        /// server, but Windows path normalization resolves it to the current directory.
        /// </summary>
        [TestMethod]
        public void Scp_Download_DirectoryInfo_DirectoryNamedThreeDots_StaysInsideDestination()
        {
            var remoteDirectory = "/tmp/sshnet-scp-dots-" + Guid.NewGuid().ToString("N");

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.CreateDirectory(remoteDirectory);
                client.CreateDirectory(remoteDirectory + "/...");
                client.WriteAllText(remoteDirectory + "/.../inner.txt", "inner");
                client.WriteAllText(remoteDirectory + "/outer.txt", "outer");
            }

            var localRoot = Path.Combine(Path.GetTempPath(), "sshnet-scp-dots-" + Guid.NewGuid().ToString("N"));
            var destination = Directory.CreateDirectory(Path.Combine(localRoot, "download"));

            try
            {
                using (var client = new ScpClient(_connectionInfoFactory.Create(), _remotePathTransformation))
                {
                    client.Connect();

                    if (Path.DirectorySeparatorChar == '\\')
                    {
                        _ = Assert.ThrowsExactly<ScpException>(() => client.Download(remoteDirectory, destination));
                    }
                    else
                    {
                        client.Download(remoteDirectory, destination);
                        Assert.AreEqual("inner", File.ReadAllText(Path.Combine(destination.FullName, "...", "inner.txt")));
                        Assert.AreEqual("outer", File.ReadAllText(Path.Combine(destination.FullName, "outer.txt")));
                    }
                }

                // Nothing should be written outside of the destination directory on any platform.
                CollectionAssert.AreEqual(new[] { destination.FullName }, Directory.GetFileSystemEntries(localRoot));
            }
            finally
            {
                using (var client = new SshClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();
                    _ = client.RunCommand("rm -rf '" + remoteDirectory + "'");
                }

                Directory.Delete(localRoot, recursive: true);
            }
        }

        /// <summary>
        /// A file whose name contains the text of a D record must be downloaded as a file, with its
        /// content written verbatim rather than interpreted as SCP records.
        /// </summary>
        [TestMethod]
        public void Scp_Download_DirectoryInfo_FileNameContainingRecordText_IsDownloadedAsFile()
        {
            const string FileName = "zD0755 0 x";
            const string Content = "E\nC0644 8 injected.txt\ninjected";

            var remoteDirectory = "/tmp/sshnet-scp-records-" + Guid.NewGuid().ToString("N");

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.CreateDirectory(remoteDirectory);
                client.WriteAllText(remoteDirectory + "/" + FileName, Content);
            }

            var localRoot = Path.Combine(Path.GetTempPath(), "sshnet-scp-records-" + Guid.NewGuid().ToString("N"));
            var destination = Directory.CreateDirectory(Path.Combine(localRoot, "download"));

            try
            {
                using (var client = new ScpClient(_connectionInfoFactory.Create(), _remotePathTransformation))
                {
                    client.Connect();
                    client.Download(remoteDirectory, destination);
                }

                var filePath = Path.Combine(destination.FullName, FileName);

                CollectionAssert.AreEquivalent(
                    new[] { destination.FullName, filePath },
                    Directory.GetFileSystemEntries(localRoot, "*", SearchOption.AllDirectories));
                Assert.AreEqual(Content, File.ReadAllText(filePath));
            }
            finally
            {
                using (var client = new SshClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();
                    _ = client.RunCommand("rm -rf '" + remoteDirectory + "'");
                }

                Directory.Delete(localRoot, recursive: true);
            }
        }

        /// <summary>
        /// A line feed in the file name part of the upload path must not be sent in the C record. It would
        /// end the record early: the server reads the rest of the name as the start of the file content,
        /// and content crafted to match is then read as further records, creating another file.
        /// </summary>
        [TestMethod]
        public void Scp_Upload_Stream_FileNameWithLineFeed_DoesNotCreateFiles()
        {
            // The server reads the rest of the name (22 x's) and the line feed that ends the record as the
            // start of the 29 bytes of content, followed by "report". It then reads the "\0" as the status
            // byte that ends the file, and the remaining 22 bytes as a C record for extra.txt and its content.
            var fileName = "report.txt\n" + new string('x', 22);
            var content = "report\0C0644 4 extra.txt\ndata"u8.ToArray();

            AssertNameWithLineFeedDoesNotCreateFiles<ArgumentException>(
                (client, remoteDirectory) =>
                {
                    using (var source = new MemoryStream(content))
                    {
                        client.Upload(source, remoteDirectory + "/" + fileName);
                    }
                });
        }

        /// <inheritdoc cref="Scp_Upload_Stream_FileNameWithLineFeed_DoesNotCreateFiles"/>
        [TestMethod]
        public void Scp_Upload_FileInfo_FileNameWithLineFeed_DoesNotCreateFiles()
        {
            var localFile = Path.GetTempFileName();
            File.WriteAllBytes(localFile, "report\0C0644 4 extra.txt\ndata"u8.ToArray());

            try
            {
                AssertNameWithLineFeedDoesNotCreateFiles<ArgumentException>((client, remoteDirectory)
                    => client.Upload(new FileInfo(localFile), remoteDirectory + "/report.txt\n" + new string('x', 22)));
            }
            finally
            {
                File.Delete(localFile);
            }
        }

        /// <summary>
        /// A local file whose name contains a line feed must not be sent in a C record when uploading a
        /// directory; see <see cref="Scp_Upload_Stream_FileNameWithLineFeed_DoesNotCreateFiles"/>.
        /// </summary>
        [TestMethod]
        [OSCondition(ConditionMode.Exclude, OperatingSystems.Windows,
            IgnoreMessage = "A line feed is not valid in a file name on Windows.")]
        public void Scp_Upload_DirectoryInfo_LocalFileNameWithLineFeed_DoesNotCreateFiles()
        {
            var localDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "sshnet-scp-lf-" + Guid.NewGuid().ToString("N")));
            File.WriteAllBytes(Path.Combine(localDirectory.FullName, "report.txt\n" + new string('x', 22)),
                               "report\0C0644 4 extra.txt\ndata"u8.ToArray());

            try
            {
                AssertNameWithLineFeedDoesNotCreateFiles<ScpException>((client, remoteDirectory)
                    => client.Upload(localDirectory, remoteDirectory));
            }
            finally
            {
                localDirectory.Delete(recursive: true);
            }
        }

        /// <summary>
        /// Runs <paramref name="upload"/> into a new, empty remote directory, and asserts that it throws
        /// <typeparamref name="TException"/> without creating anything in that directory.
        /// </summary>
        private void AssertNameWithLineFeedDoesNotCreateFiles<TException>(Action<ScpClient, string> upload)
            where TException : Exception
        {
            var remoteDirectory = "/tmp/sshnet-scp-lf-" + Guid.NewGuid().ToString("N");

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                _ = client.RunCommand("mkdir -p '" + remoteDirectory + "'");
            }

            try
            {
                Exception exception = null;

                using (var client = new ScpClient(_connectionInfoFactory.Create(), _remotePathTransformation))
                {
                    client.Connect();

                    try
                    {
                        upload(client, remoteDirectory);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                }

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    var entries = client.ListDirectory(remoteDirectory)
                                        .Where(f => f.Name is not "." and not "..")
                                        .Select(f => f.Name)
                                        .ToArray();

                    Assert.IsEmpty(entries, "Files were created on the server: " + string.Join(", ", entries));
                }

                Assert.IsInstanceOfType<TException>(exception);
            }
            finally
            {
                using (var client = new SshClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();
                    _ = client.RunCommand("rm -rf '" + remoteDirectory + "'");
                }
            }
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

        [TestMethod]
        public async Task UploadWithUseDirectoryFlagFalse()
        {
            string remoteDirectory = "/home/sshnet/usedirectoryflagfalsetest";

            // remote cleanup
            using (var sftpClient = new SftpClient(_connectionInfoFactory.Create()))
            {
                await sftpClient.ConnectAsync(CancellationToken.None);

                if (await sftpClient.ExistsAsync(remoteDirectory))
                {
                    await sftpClient.DeleteDirectoryAsync(remoteDirectory);
                }

                await sftpClient.CreateDirectoryAsync(remoteDirectory);
            }

            using (var client = new ScpClient(_connectionInfoFactory.Create()))
            {
                client.UseDirectoryFlag = false;

                await client.ConnectAsync(CancellationToken.None);
                int tempFileSize = 1024;
                string tempFilePath = CreateTempFile(tempFileSize);
                MemoryStream downloadedStream = new();

                // FileInfo overload
                client.Upload(new FileInfo(tempFilePath), $"{remoteDirectory}/file1");
                client.Download($"{remoteDirectory}/file1", downloadedStream);
                Assert.AreEqual(tempFileSize, downloadedStream.Length);

                // Stream overload
                downloadedStream = new();
                using (Stream stream = File.OpenRead(tempFilePath))
                {
                    client.Upload(stream, $"{remoteDirectory}/file2");
                    client.Download($"{remoteDirectory}/file2", downloadedStream);
                    Assert.AreEqual(tempFileSize, downloadedStream.Length);
                }

                // DirectoryInfo overload
                downloadedStream = new();
                string tempDir = Path.Combine(Path.GetTempPath(), "SSH.NET_UploadWithUseDirectoryFlagFalseTest");

                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }

                Directory.CreateDirectory(tempDir);
                File.Move(tempFilePath, $"{tempDir}/file3");

                client.Upload(new DirectoryInfo(tempDir), remoteDirectory);
                client.Download($"{remoteDirectory}/file3", downloadedStream);
                Assert.AreEqual(tempFileSize, downloadedStream.Length);
            }
        }
    }
}

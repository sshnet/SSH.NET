using System.Security.Cryptography;

namespace Renci.SshNet.IntegrationTests
{
    public abstract class TestBase : IntegrationTestBase
    {
        protected static MemoryStream CreateMemoryStream(int size)
        {
            var memoryStream = new MemoryStream();
            FillStream(memoryStream, size);
            return memoryStream;
        }

        protected static void FillStream(Stream stream, int size)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var randomContent = new byte[50];
            var random = new Random();

            var numberOfBytesToWrite = size;

            while (numberOfBytesToWrite > 0)
            {
                random.NextBytes(randomContent);

                var numberOfCharsToWrite = Math.Min(numberOfBytesToWrite, randomContent.Length);
                stream.Write(randomContent, 0, numberOfCharsToWrite);
                numberOfBytesToWrite -= numberOfCharsToWrite;
            }
        }

        protected static string CreateHash(Stream stream)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(stream);
                return Encoding.ASCII.GetString(hash);
            }
        }

        protected static string CreateHash(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                return CreateHash(ms);
            }
        }

        protected static string CreateFileHash(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                return CreateHash(fs);
            }
        }

        protected static string CreateTempFile(int size)
        {
            var file = Path.GetTempFileName();
            CreateFile(file, size);
            return file;
        }

        protected static void CreateFile(string fileName, int size)
        {
            using (var fs = File.OpenWrite(fileName))
            {
                FillStream(fs, size);
            }
        }

        protected Stream GetManifestResourceStream(string resourceName)
        {
            var type = GetType();
            var resourceStream = type.Assembly.GetManifestResourceStream(resourceName);

            if (resourceStream is null)
            {
                throw new ArgumentException($"Resource '{resourceName}' not found in assembly '{type.Assembly.FullName}'.", nameof(resourceName));
            }

            return resourceStream;
        }
    }
}

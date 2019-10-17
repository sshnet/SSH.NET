using Renci.SshNet.Common;
using Renci.SshNet.Security;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Renci.SshNet
{
    /// <summary>
    /// 
    /// </summary>
    public class PublicKeyCertFile : IDisposable
    {
        private static readonly Regex CertificateRegex = new Regex(@"(?<type>(^ssh-rsa-cert-v01))@openssh\.com\s(?<data>([a-zA-Z0-9\/+=]*))\s+(?<comment>(.*))",
#if FEATURE_REGEX_COMPILE
            RegexOptions.Compiled | RegexOptions.Multiline);
#else
            RegexOptions.Multiline);
#endif

        /// <summary>
        /// 
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets the host key.
        /// </summary>
        public HostAlgorithm HostCertificate { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicKeyCertFile"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <c>null</c> or empty.</exception>
        /// <remarks>This method calls <see cref="System.IO.File.Open(string, System.IO.FileMode)"/> internally, this method does not catch exceptions from <see cref="System.IO.File.Open(string, System.IO.FileMode)"/>.</remarks>
        public PublicKeyCertFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            using (var certFile = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Open(certFile);
            }
        }


        /// <summary>
        /// Opens the specified private key.
        /// </summary>
        /// <param name="certFile">The private key.</param>
        private void Open(Stream certFile)
        {
            if (certFile == null)
                throw new ArgumentNullException("certFile");

            Match certificateMatch;

            using (var sr = new StreamReader(certFile))
            {
                var text = sr.ReadToEnd();
                certificateMatch = CertificateRegex.Match(text);
            }

            if (!certificateMatch.Success)
            {
                throw new SshException("Invalid certificate file.");
            }

            var certType = certificateMatch.Result("${type}");
            var data = certificateMatch.Result("${data}");
            var comment = certificateMatch.Result("${comment}");

            var binaryData = Convert.FromBase64String(data);
            Data = binaryData;

            switch (certType)
            {
                case "ssh-rsa-cert-v01":
                    HostCertificate = new CertificateHostAlgorithm("ssh-rsa-cert-v01@openssh.com", binaryData);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Key '{0}' is not supported.", certType));
            }

        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PublicKeyCertFile"/> is reclaimed by garbage collection.
        /// </summary>
        ~PublicKeyCertFile()
        {
            Dispose(false);
        }

        #endregion

    }
}

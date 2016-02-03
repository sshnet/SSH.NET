using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
{
    public class ConnectingEventArgs : EventArgs
    {
        public IDictionary<string, string> KeyExchangeAlgorithms { get; private set; }

        public IDictionary<string, string> Encryptions { get; private set; }

        public IDictionary<string, string> HmacAlgorithms { get; private set; }

        public IDictionary<string, string> HostKeyAlgorithms { get; private set; }

        public IDictionary<string, string> SupportedAuthenticationMethods { get; private set; }

        public IDictionary<string, string> CompressionAlgorithms { get; private set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public ICollection<PrivateKeyFile> KeyFiles { get; set; }

        public TimeSpan Timeout { get; set; }

        public int RetryAttempts { get; set; }

        public int MaxSessions { get; set; }

        public ConnectingEventArgs(
            IDictionary<string, string> keyExchangeAlgorithms,
            IDictionary<string, string> encryptions,
            IDictionary<string, string> hmacAlgorithms,
            IDictionary<string, string> hostKeyAlgorithms,
            IDictionary<string, string> supportedAuthenticationMethods,
            IDictionary<string, string> compressionAlgorithms)
        {
            this.KeyExchangeAlgorithms = keyExchangeAlgorithms;
            this.Encryptions = encryptions;
            this.HmacAlgorithms = hmacAlgorithms;
            this.HostKeyAlgorithms = hostKeyAlgorithms;
            this.SupportedAuthenticationMethods = supportedAuthenticationMethods;
            this.CompressionAlgorithms = compressionAlgorithms;
        }
    }
}

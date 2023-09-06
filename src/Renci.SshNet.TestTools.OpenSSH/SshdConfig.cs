using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Renci.SshNet.TestTools.OpenSSH.Formatters;

namespace Renci.SshNet.TestTools.OpenSSH
{
    public class SshdConfig
    {
        private static readonly Regex MatchRegex = new Regex($@"\s*Match\s+(User\s+(?<users>[\S]+))?\s*(Address\s+(?<addresses>[\S]+))?\s*", RegexOptions.Compiled);

        private readonly SubsystemFormatter _subsystemFormatter;
        private readonly Int32Formatter _int32Formatter;
        private readonly BooleanFormatter _booleanFormatter;
        private readonly MatchFormatter _matchFormatter;

        private SshdConfig()
        {
            AcceptedEnvironmentVariables = new List<string>();
            Ciphers = new List<Cipher>();
            HostKeyFiles = new List<string>();
            HostKeyAlgorithms = new List<HostKeyAlgorithm>();
            KeyExchangeAlgorithms = new List<KeyExchangeAlgorithm>();
            PublicKeyAcceptedAlgorithms = new List<PublicKeyAlgorithm>();
            MessageAuthenticationCodeAlgorithms = new List<MessageAuthenticationCodeAlgorithm>();
            Subsystems = new List<Subsystem>();
            Matches = new List<Match>();
            LogLevel = LogLevel.Info;
            Port = 22;
            Protocol = "2,1";

            _booleanFormatter = new BooleanFormatter();
            _int32Formatter = new Int32Formatter();
            _matchFormatter = new MatchFormatter();
            _subsystemFormatter = new SubsystemFormatter();
        }

        /// <summary>
        /// Gets or sets the port number that sshd listens on.
        /// </summary>
        /// <value>
        /// The port number that sshd listens on. The default is 22.
        /// </value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the list of private host key files used by sshd.
        /// </summary>
        /// <value>
        /// A list of private host key files used by sshd.
        /// </value>
        public List<string> HostKeyFiles { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether challenge-response authentication is allowed.
        /// </summary>
        /// <value>
        /// A value specifying whether challenge-response authentication is allowed, or <see langword="null"/>
        /// if this option is not configured.
        /// </value>
        public bool? ChallengeResponseAuthentication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow keyboard-interactive authentication.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to allow and <see langword="false"/> to disallow keyboard-interactive
        /// authentication, or <see langword="null"/> if this option is not configured.
        /// </value>
        public bool? KeyboardInteractiveAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the verbosity when logging messages from sshd.
        /// </summary>
        /// <value>
        /// The verbosity when logging messages from sshd. The default is <see cref="LogLevel.Info"/>.
        /// </value>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets a sets a value indicating whether the Pluggable Authentication Module interface is enabled.
        /// </summary>
        /// <value>
        /// A value indicating whether the Pluggable Authentication Module interface is enabled.
        /// </value>
        public bool? UsePAM { get; set; }

        public List<Subsystem> Subsystems { get; }

        /// <summary>
        /// Gets a list of conditional blocks.
        /// </summary>
        public List<Match> Matches { get; }

        public bool X11Forwarding { get; private set; }
        public List<string> AcceptedEnvironmentVariables { get; private set; }
        public List<Cipher> Ciphers { get; private set; }

        /// <summary>
        /// Gets the host key signature algorithms that the server offers.
        /// </summary>
        public List<HostKeyAlgorithm> HostKeyAlgorithms { get; private set; }

        /// <summary>
        /// Gets the available KEX (Key Exchange) algorithms.
        /// </summary>
        public List<KeyExchangeAlgorithm> KeyExchangeAlgorithms { get; private set; }

        /// <summary>
        /// Gets the signature algorithms that will be accepted for public key authentication.
        /// </summary>
        public List<PublicKeyAlgorithm> PublicKeyAcceptedAlgorithms { get; private set; }

        /// <summary>
        /// Gets the available MAC (message authentication code) algorithms.
        /// </summary>
        public List<MessageAuthenticationCodeAlgorithm> MessageAuthenticationCodeAlgorithms { get; private set; }

        /// <summary>
        /// Gets a value indicating whether <c>sshd</c> should print <c>/etc/motd</c> when a user logs in interactively.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if <c>sshd</c> should print <c>/etc/motd</c> when a user logs in interactively
        /// and <see langword="false"/> if it should not; <see langword="null"/> if this option is not configured.
        /// </value>
        public bool? PrintMotd { get; set; }

        /// <summary>
        /// Gets or sets the protocol versions sshd supported.
        /// </summary>
        /// <value>
        /// The protocol versions sshd supported. The default is <c>2,1</c>.
        /// </value>
        public string Protocol { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether TCP forwarding is allowed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to allow and <see langword="false"/> to disallow TCP forwarding,
        /// or <see langword="null"/> if this option is not configured.
        /// </value>
        public bool? AllowTcpForwarding { get; set; }

        public void SaveTo(TextWriter writer)
        {
            writer.WriteLine("Protocol " + Protocol);
            writer.WriteLine("Port " + _int32Formatter.Format(Port));
            if (HostKeyFiles.Count > 0)
            {
                writer.WriteLine("HostKey " + string.Join(",", HostKeyFiles.ToArray()));
            }

            if (ChallengeResponseAuthentication is not null)
            {
                writer.WriteLine("ChallengeResponseAuthentication " + _booleanFormatter.Format(ChallengeResponseAuthentication.Value));
            }

            if (KeyboardInteractiveAuthentication is not null)
            {
                writer.WriteLine("KbdInteractiveAuthentication " + _booleanFormatter.Format(KeyboardInteractiveAuthentication.Value));
            }

            if (AllowTcpForwarding is not null)
            {
                writer.WriteLine("AllowTcpForwarding " + _booleanFormatter.Format(AllowTcpForwarding.Value));
            }

            if (PrintMotd is not null)
            {
                writer.WriteLine("PrintMotd " + _booleanFormatter.Format(PrintMotd.Value));
            }

            writer.WriteLine("LogLevel " + new LogLevelFormatter().Format(LogLevel));

            foreach (var subsystem in Subsystems)
            {
                writer.WriteLine("Subsystem " + _subsystemFormatter.Format(subsystem));
            }

            if (UsePAM is not null)
            {
                writer.WriteLine("UsePAM " + _booleanFormatter.Format(UsePAM.Value));
            }

            writer.WriteLine("X11Forwarding " + _booleanFormatter.Format(X11Forwarding));

            foreach (var acceptedEnvVar in AcceptedEnvironmentVariables)
            {
                writer.WriteLine("AcceptEnv " + acceptedEnvVar);
            }

            if (Ciphers.Count > 0)
            {
                writer.WriteLine("Ciphers " + string.Join(",", Ciphers.Select(c => c.Name).ToArray()));
            }

            if (HostKeyAlgorithms.Count > 0)
            {
                writer.WriteLine("HostKeyAlgorithms " + string.Join(",", HostKeyAlgorithms.Select(c => c.Name).ToArray()));
            }

            if (KeyExchangeAlgorithms.Count > 0)
            {
                writer.WriteLine("KexAlgorithms " + string.Join(",", KeyExchangeAlgorithms.Select(c => c.Name).ToArray()));
            }

            if (MessageAuthenticationCodeAlgorithms.Count > 0)
            {
                writer.WriteLine("MACs " + string.Join(",", MessageAuthenticationCodeAlgorithms.Select(c => c.Name).ToArray()));
            }

            writer.WriteLine("PubkeyAcceptedAlgorithms " + string.Join(",", PublicKeyAcceptedAlgorithms.Select(c => c.Name).ToArray()));

            foreach (var match in Matches)
            {
                _matchFormatter.Format(match, writer);
            }
        }

        public static SshdConfig LoadFrom(Stream stream, Encoding encoding)
        {
            using (var sr = new StreamReader(stream, encoding))
            {
                var sshdConfig = new SshdConfig();

                Match? currentMatchConfiguration = null;

                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    // Skip empty lines
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    // Skip comments
                    if (line[0] == '#')
                    {
                        continue;
                    }

                    var match = MatchRegex.Match(line);
                    if (match.Success)
                    {
                        var usersGroup = match.Groups["users"];
                        var addressesGroup = match.Groups["addresses"];
                        var users = usersGroup.Success ? usersGroup.Value.Split(',') : Array.Empty<string>();
                        var addresses = addressesGroup.Success ? addressesGroup.Value.Split(',') : Array.Empty<string>();

                        currentMatchConfiguration = new Match(users, addresses);
                        sshdConfig.Matches.Add(currentMatchConfiguration);
                        continue;
                    }

                    if (currentMatchConfiguration != null)
                    {
                        ProcessMatchOption(currentMatchConfiguration, line);
                    }
                    else
                    {
                        ProcessGlobalOption(sshdConfig, line);
                    }
                }

                if (sshdConfig.Ciphers == null)
                {
                    // Obtain supported ciphers using ssh -Q cipher
                }

                if (sshdConfig.KeyExchangeAlgorithms == null)
                {
                    // Obtain supports key exchange algorithms using ssh -Q kex
                }

                if (sshdConfig.HostKeyAlgorithms == null)
                {
                    // Obtain supports host key algorithms using ssh -Q key
                }

                if (sshdConfig.MessageAuthenticationCodeAlgorithms == null)
                {
                    // Obtain supported MACs using ssh -Q mac 
                }


                return sshdConfig;
            }
        }

        private static void ProcessGlobalOption(SshdConfig sshdConfig, string line)
        {
            var matchOptionRegex = new Regex(@"^\s*(?<name>[\S]+)\s+(?<value>.+?){1}\s*$");

            var optionsMatch = matchOptionRegex.Match(line);
            if (!optionsMatch.Success)
            {
                return;
            }

            var nameGroup = optionsMatch.Groups["name"];
            var valueGroup = optionsMatch.Groups["value"];

            var name = nameGroup.Value;
            var value = valueGroup.Value;

            switch (name)
            {
                case "Port":
                    sshdConfig.Port = ToInt(value);
                    break;
                case "HostKey":
                    sshdConfig.HostKeyFiles = ParseCommaSeparatedValue(value);
                    break;
                case "ChallengeResponseAuthentication":
                    sshdConfig.ChallengeResponseAuthentication = ToBool(value);
                    break;
                case "KbdInteractiveAuthentication":
                    sshdConfig.KeyboardInteractiveAuthentication = ToBool(value);
                    break;
                case "LogLevel":
                    sshdConfig.LogLevel = (LogLevel) Enum.Parse(typeof(LogLevel), value, true);
                    break;
                case "Subsystem":
                    sshdConfig.Subsystems.Add(Subsystem.FromConfig(value));
                    break;
                case "UsePAM":
                    sshdConfig.UsePAM = ToBool(value);
                    break;
                case "X11Forwarding":
                    sshdConfig.X11Forwarding = ToBool(value);
                    break;
                case "Ciphers":
                    sshdConfig.Ciphers = ParseCiphers(value);
                    break;
                case "KexAlgorithms":
                    sshdConfig.KeyExchangeAlgorithms = ParseKeyExchangeAlgorithms(value);
                    break;
                case "PubkeyAcceptedAlgorithms":
                    sshdConfig.PublicKeyAcceptedAlgorithms = ParsePublicKeyAcceptedAlgorithms(value);
                    break;
                case "HostKeyAlgorithms":
                    sshdConfig.HostKeyAlgorithms = ParseHostKeyAlgorithms(value);
                    break;
                case "MACs":
                    sshdConfig.MessageAuthenticationCodeAlgorithms = ParseMacs(value);
                    break;
                case "PrintMotd":
                    sshdConfig.PrintMotd = ToBool(value);
                    break;
                case "AcceptEnv":
                    ParseAcceptedEnvironmentVariable(sshdConfig, value);
                    break;
                case "Protocol":
                    sshdConfig.Protocol = value;
                    break;
                case "AllowTcpForwarding":
                    sshdConfig.AllowTcpForwarding = ToBool(value);
                    break;
                case "KeyRegenerationInterval":
                case "HostbasedAuthentication":
                case "ServerKeyBits":
                case "SyslogFacility":
                case "LoginGraceTime":
                case "PermitRootLogin":
                case "StrictModes":
                case "RSAAuthentication":
                case "PubkeyAuthentication":
                case "IgnoreRhosts":
                case "RhostsRSAAuthentication":
                case "PermitEmptyPasswords":
                case "X11DisplayOffset":
                case "PrintLastLog":
                case "TCPKeepAlive":
                case "AuthorizedKeysFile":
                case "PasswordAuthentication":
                case "GatewayPorts":
                    break;
                default:
                    throw new Exception($"Global option '{name}' is not implemented.");
            }
        }

        private static void ParseAcceptedEnvironmentVariable(SshdConfig sshdConfig, string value)
        {
            var acceptedEnvironmentVariables = value.Split(' ');
            foreach (var acceptedEnvironmentVariable in acceptedEnvironmentVariables)
            {
                sshdConfig.AcceptedEnvironmentVariables.Add(acceptedEnvironmentVariable);
            }
        }

        private static List<Cipher> ParseCiphers(string value)
        {
            var cipherNames = value.Split(',');
            var ciphers = new List<Cipher>(cipherNames.Length);
            foreach (var cipherName in cipherNames)
            {
                ciphers.Add(new Cipher(cipherName.Trim()));
            }
            return ciphers;
        }

        private static List<KeyExchangeAlgorithm> ParseKeyExchangeAlgorithms(string value)
        {
            var kexNames = value.Split(',');
            var keyExchangeAlgorithms = new List<KeyExchangeAlgorithm>(kexNames.Length);
            foreach (var kexName in kexNames)
            {
                keyExchangeAlgorithms.Add(new KeyExchangeAlgorithm(kexName.Trim()));
            }
            return keyExchangeAlgorithms;
        }

        public static List<PublicKeyAlgorithm> ParsePublicKeyAcceptedAlgorithms(string value)
        {
            var publicKeyAlgorithmNames = value.Split(',');
            var publicKeyAlgorithms = new List<PublicKeyAlgorithm>(publicKeyAlgorithmNames.Length);
            foreach (var publicKeyAlgorithmName in publicKeyAlgorithmNames)
            {
                publicKeyAlgorithms.Add(new PublicKeyAlgorithm(publicKeyAlgorithmName.Trim()));
            }
            return publicKeyAlgorithms;
        }

        private static List<HostKeyAlgorithm> ParseHostKeyAlgorithms(string value)
        {
            var algorithmNames = value.Split(',');
            var hostKeyAlgorithms = new List<HostKeyAlgorithm>(algorithmNames.Length);
            foreach (var algorithmName in algorithmNames)
            {
                hostKeyAlgorithms.Add(new HostKeyAlgorithm(algorithmName.Trim()));
            }
            return hostKeyAlgorithms;
        }

        private static List<MessageAuthenticationCodeAlgorithm> ParseMacs(string value)
        {
            var macNames = value.Split(',');
            var macAlgorithms = new List<MessageAuthenticationCodeAlgorithm>(macNames.Length);
            foreach (var algorithmName in macNames)
            {
                macAlgorithms.Add(new MessageAuthenticationCodeAlgorithm(algorithmName.Trim()));
            }
            return macAlgorithms;
        }

        private static void ProcessMatchOption(Match matchConfiguration, string line)
        {
            var matchOptionRegex = new Regex(@"^\s+(?<name>[\S]+)\s+(?<value>.+?){1}\s*$");

            var optionsMatch = matchOptionRegex.Match(line);
            if (!optionsMatch.Success)
            {
                return;
            }

            var nameGroup = optionsMatch.Groups["name"];
            var valueGroup = optionsMatch.Groups["value"];

            var name = nameGroup.Value;
            var value = valueGroup.Value;

            switch (name)
            {
                case "AuthenticationMethods":
                    matchConfiguration.AuthenticationMethods = value;
                    break;
                default:
                    throw new Exception($"Match option '{name}' is not implemented.");
            }
        }


        private static List<string> ParseCommaSeparatedValue(string value)
        {
            var values = value.Split(',');
            return new List<string>(values);
        }

        private static bool ToBool(string value)
        {
            switch (value)
            {
                case "yes":
                    return true;
                case "no":
                    return false;
                default:
                    throw new Exception($"Value '{value}' cannot be mapped to a boolean.");
            }
        }

        private static int ToInt(string value)
        {
            return int.Parse(value, NumberFormatInfo.InvariantInfo);
        }
    }
}

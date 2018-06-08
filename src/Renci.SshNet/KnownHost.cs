using Renci.SshNet.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents a host with a known public key
    /// </summary>
    internal class KnownHost
    {
        //If present at the start of a known_hosts entry, signifies that the hostname is sha1 hashed
        private const string HashedHostSignifier = "|1|";

        private const UInt16 DefaultSshPortNumber = 22;
        private const string DefaultSshPortString = "22";

        private const string KeyRevokeMarker = "@revoked";
        private const string KeyCertAuthMarker = "@cert-authority";

        private readonly bool _isHashed;
        private readonly List<Tuple<string, Regex>> _plaintextHostPatterns;
        private readonly byte[] _hashedHostName;
        private readonly byte[] _hashSalt;
        private readonly byte[] _pubKey;
        private readonly string _keyType;
        private readonly HostPubkeyMarker _hostMarker;

        public enum HostValidationResponse
        {
            Matches,
            DoesNoMatch,
            KeyRevoked,
            InvalidSignature,
            ValidSignature
        }

        private enum HostPubkeyMarker
        {
            None,
            Revoke,
            CertAuthority,
            Error
        }

        private static HostPubkeyMarker ParseSpecialOperand(string toParse)
        {
            switch (toParse)
            {
                case KeyRevokeMarker:
                    return HostPubkeyMarker.Revoke;
                case KeyCertAuthMarker:
                    return HostPubkeyMarker.CertAuthority;
                default:
                    return HostPubkeyMarker.Error;
            }
        }

        private KnownHost(string hostName, string keyType, string base64Pubkey, HostPubkeyMarker marker)
        {
            _isHashed = hostName.StartsWith("|");

            if (_isHashed)
            {
                if (!hostName.StartsWith(HashedHostSignifier))
                    throw new FormatException("The hashed section was not properly composed");

                string[] splitHashedSection = hostName.Split('|');
                if (splitHashedSection.Length != 4)
                    throw new FormatException("The hashed section was not properly composed");

                _hashSalt = Convert.FromBase64String(splitHashedSection[2]);

                if (_hashSalt.Length != 20)
                    throw new ArgumentException("The salt must be exacly 20 bytes");

                _hashedHostName = Convert.FromBase64String(splitHashedSection[3]);
            }
            else
            {
                _plaintextHostPatterns = GetPatternList(hostName);
                if (_plaintextHostPatterns.Count == 0)
                    throw new FormatException("No hostname patterns given");
            }

            _keyType = keyType;
            _pubKey = Convert.FromBase64String(base64Pubkey);
            _hostMarker = marker;
        }

        /// <summary>
        /// Attempts to parse a line from an openssh known_hosts file
        /// </summary>
        /// <param name="hostLine"></param>
        /// <param name="host"></param>
        /// <returns><c>true</c> if the host line could be parsed.  <c>false</c> if an error occurred</returns>
        public static bool TryParse(string hostLine, out KnownHost host)
        {
            try
            {
                return UnsafeTryParse(hostLine, out host);
            }
            catch (FormatException)
            {
                host = null;
                return false;
            }
            catch (ArgumentException)
            {
                host = null;
                return false;
            }
        }

        private static bool UnsafeTryParse(string hostLine, out KnownHost host)
        {
            host = null;
            if (string.IsNullOrEmpty(hostLine) || hostLine.StartsWith("#"))
                return false;

            bool hasSpecialPrefix = hostLine.StartsWith("@");

            string[] sectionedLine = hostLine.Split(' ');
            if (hasSpecialPrefix && sectionedLine.Length < 4)
                return false;
            else if (sectionedLine.Length < 3)
                return false;

            HostPubkeyMarker marker =
                hasSpecialPrefix ? ParseSpecialOperand(sectionedLine[0]) : HostPubkeyMarker.None;
            if (marker == HostPubkeyMarker.Error)
                return false;

            int sectionOffset = hasSpecialPrefix ? 1 : 0;
            string hostNameSecion = sectionedLine[sectionOffset];
            string keyTypeSection = sectionedLine[1 + sectionOffset];
            string pubKeySection = sectionedLine[2 + sectionOffset];

            host = new KnownHost(hostNameSecion, keyTypeSection, pubKeySection, marker);

            return true;
        }

        /// <summary>
        /// Compare the given host information with this KnownHost
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="keyType"></param>
        /// <param name="pubKey"></param>
        /// <param name="portNumber"></param>
        /// <returns>Whether or not the given hostname-keytype-pubkey combination is a match to this KnownHost</returns>
        public HostValidationResponse MatchesPubKey(string hostname, string keyType, byte[] pubKey, UInt16 portNumber)
        {
            if (!ValidateHostName(hostname, portNumber))
                return HostValidationResponse.DoesNoMatch;

            if (_hostMarker == HostPubkeyMarker.CertAuthority)
                return ValidateKeySignature(hostname, keyType, pubKey);

            if (_keyType != keyType || !_pubKey.SequenceEqual(pubKey))
                return HostValidationResponse.DoesNoMatch;

            switch (_hostMarker)
            {
                case HostPubkeyMarker.Revoke:
                    return HostValidationResponse.KeyRevoked;
                default:
                    return HostValidationResponse.Matches;
            }
        }

        private HostValidationResponse ValidateKeySignature(string hostname, string keyType, byte[] keyToCheck)
        {
            //TODO: Return HostValidationResponse.ValidSignature if the key is signed by this Certificate Authority
            return HostValidationResponse.InvalidSignature;
        }

        /// <summary>
        /// Writes this KnownHost as an openssh known_hosts formatted line
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string hostnameSection = _isHashed
                ? string.Format("{0}{1}|{2}", HashedHostSignifier, Convert.ToBase64String(_hashSalt), Convert.ToBase64String(_hashedHostName))
                : string.Join("," , _plaintextHostPatterns.Select(x => x.Item1));

            string specialPrefix = string.Empty;
            switch (_hostMarker)
            {
                case HostPubkeyMarker.CertAuthority:
                    specialPrefix = KeyCertAuthMarker + " ";
                    break;
                case HostPubkeyMarker.Revoke:
                    specialPrefix = KeyRevokeMarker + " ";
                    break;
            }

            return string.Format("{0}{1} {2} {3}",specialPrefix, hostnameSection, _keyType, Convert.ToBase64String(_pubKey));
        }

        private bool ValidateHostName(string host, UInt16 port)
        {
            if (_isHashed)
            {
                //Corner case with hashed hosts:  If the default port is specified, then the hash could either be just the hostname,
                //or hostname with port
                if (port == DefaultSshPortNumber && ValidateHashedHostName(host))
                    return true;
                return ValidateHashedHostName(string.Format("[{0}:{1}]", host, port));
            }
            return ValidatePlaintextHostName(string.Format("[{0}:{1}]", host, port));
        }

        private bool ValidateHashedHostName(string hostAndPort)
        {
            HMACSHA1 hmac = new HMACSHA1(_hashSalt);
            byte[] hashToCompare = hmac.ComputeHash(Encoding.ASCII.GetBytes(hostAndPort));

            return _hashedHostName.SequenceEqual(hashToCompare);
        }

        private bool ValidatePlaintextHostName(string hostAndPort)
        {
            bool foundAtLeastOneMatch = false;
            foreach (Tuple<string, Regex> possibleMatch in _plaintextHostPatterns)
            {
                bool negateMatch = possibleMatch.Item1.StartsWith("!");
                bool foundMatch = possibleMatch.Item2.IsMatch(hostAndPort);
                if (foundMatch && negateMatch)
                    return false;

                foundAtLeastOneMatch = foundAtLeastOneMatch || foundMatch;
            }

            return foundAtLeastOneMatch;
        }

        private List<Tuple<string, Regex>> GetPatternList(string unsplitHostPatterns)
        {
            List<Tuple<string, Regex>> patternList = new List<Tuple<string, Regex>>();
            foreach (string s in unsplitHostPatterns.Split(','))
            {
                Regex toAdd;
                if(!TryGetRegexFromPlaintextHostPattern(s, out toAdd))
                    continue;
                patternList.Add(new Tuple<string, Regex>(s, toAdd));
            }

            return patternList;
        }

        private static bool TryGetRegexFromPlaintextHostPattern(string pattern, out Regex matchingExpression)
        {
            matchingExpression = null;

            StringBuilder regexBuilder = new StringBuilder();

            string strippedHostPattern;
            if(!TryValidateAndFormatHostPattern(pattern, out strippedHostPattern))
                return false;

            //split by *, regex escape the split sections, replace * with regex equivalent (.*)
            string[] sectionedPattern = strippedHostPattern.Split('*');
            regexBuilder.Append(string.Join(".*", sectionedPattern.Select(Regex.Escape)));


            matchingExpression = new Regex(regexBuilder.ToString());
            return true;
        }

        private static bool TryValidateAndFormatHostPattern(string input, out string hostPattern)
        {
            hostPattern = null;

            bool squareBraceOpens = input.StartsWith("[");
            bool squareBraceCloses = input.EndsWith("]");

            //Opening/Closing braces must be matched
            if (squareBraceOpens ^ squareBraceCloses)
                return false;

            if(squareBraceOpens)
            {
                //Patterns surrounded by braces must specify port
                if (!input.Contains(":"))
                    return false;
                hostPattern = input;
            }
            else
            {
                hostPattern = string.Format("[{0}:{1}]", input, DefaultSshPortString);
            }

            //Strip the negation operand if present
            if (hostPattern.StartsWith("[!"))
                hostPattern = hostPattern.Remove(1, 1);

            return true;
        }
    }
}
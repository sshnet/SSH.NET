using System;
using Renci.SshNet.Abstractions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet
{
    /// <summary>
    /// A class to save and store the public keys of connected hosts.  Capable
    /// of reading and creating files compatible with openssh's known_hosts
    /// format.  For more information on known_hosts files, read the
    /// <c>man 8 sshd</c> section on known_hosts
    /// </summary>
    public class KnownHostStore
    {
        private readonly List<KnownHost> _knownHosts;

        /// <summary>
        /// Construct an empty KnownHostStore
        /// </summary>
        public KnownHostStore()
        {
            _knownHosts = new List<KnownHost>();
        }

        /// <summary>
        /// Construct a KnownHostStore from an openssh known_hosts file
        /// </summary>
        /// <param name="knownHostsFile">The path of the file to read</param>
        public KnownHostStore(string knownHostsFile)
        {
            _knownHosts = new List<KnownHost>();
            ImportKnownHostsFromFile(knownHostsFile);
        }

        /// <summary>
        /// Fill this store with elements from an openssh known_hosts file
        /// </summary>
        /// <param name="filePath">The path of the file to read</param>
        public void ImportKnownHostsFromFile(string filePath)
        {
            using (var fStream = File.OpenText(filePath))
            {
                while (!fStream.EndOfStream)
                {
                    KnownHost createdHost;
                    if (KnownHost.TryParse(fStream.ReadLine(), out createdHost))
                        _knownHosts.Add(createdHost);
                }
            }
        }

        /// <summary>
        /// Add a host to this store.
        /// </summary>
        /// <param name="hostname">The name of the host to add</param>
        /// <param name="portNumber"></param>
        /// <param name="keyType">The algorithm of the public key</param>
        /// <param name="pubKey">The public key of the host to add</param>
        /// <param name="storeHostnameHashed">Whether the hostname should be stored as a SHA1 hash</param>
        /// <param name="marker">An optional <c>@</c> prefixed string that corresponds to a marker for this host.  See the <c>man 8 sshd</c> section about known_hosts for valid markers</param>
        /// <exception cref="FormatException">Thrown if the given arguments cannor be parsed into a valid host entry</exception>
        public void AddHost(string hostname, UInt16 portNumber, string keyType, byte[] pubKey, bool storeHostnameHashed, string marker = "")
        {
            string hostNameWithPort = string.Format("[{0}:{1}]", hostname, portNumber);
            string hostnameSection;

            if (!string.IsNullOrEmpty(marker))
            {
                if(!marker.StartsWith("@"))
                    throw new FormatException("The given host marker must be prefixed with \'@\'.  See the \'man 8 sshd\' section about known_hosts for more details on markers");
                marker = string.Format("{0} ", marker);
            }

            if (storeHostnameHashed)
            {
                byte[] salt = new byte[20];
                CryptoAbstraction.GenerateRandom(salt);

                HMACSHA1 hmac = new HMACSHA1(salt);
                byte[] hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(hostNameWithPort));

                hostnameSection = string.Format("|1|{0}|{1}", Convert.ToBase64String(salt), Convert.ToBase64String(hash));
            }
            else
            {
                hostnameSection = hostNameWithPort;
            }

            string hostToParse = string.Format("{3}{0} {1} {2}", hostnameSection, keyType, Convert.ToBase64String(pubKey), marker);

            KnownHost newHost;
            if(!KnownHost.TryParse(hostToParse, out newHost))
                throw new FormatException("Malformed input: Failed to create entry.  If you specified a marker, ensure it is valid (see the \'man 8 sshd\' section on known_hosts for more details on markers)");

            _knownHosts.Add(newHost);
        }

        /// <summary>
        /// Removes all hosts from this store
        /// </summary>
        public void ClearHosts()
        {
            _knownHosts.Clear();
        }

        /// <summary>
        /// Writes all hosts in this store to an openssh known_hosts formatted file
        /// </summary>
        /// <param name="outFile">The path of the file to be written</param>
        public void ExportKnownHostsToFile(string outFile)
        {
            using (var fStream = File.OpenWrite(outFile))
            {
                foreach (KnownHost host in _knownHosts)
                {
                    byte[] hostLine = Encoding.ASCII.GetBytes(host.ToString() + Environment.NewLine);
                    fStream.Write(hostLine, 0, hostLine.Length);
                }
            }
        }

        /// <summary>
        /// Look for a host in this store with the given hostname and public key
        /// </summary>
        /// <param name="hostname">The name of the host to search for</param>
        /// <param name="keyType">The algorithm of the public key</param>
        /// <param name="pubKey">The public key of the host to search for</param>
        /// <param name="port">The port number</param>
        /// <exception cref="RevokedKeyException">If the given key is marked as revoked</exception>
        /// <returns>Whether a host corresponding to the given parameters exists in this store</returns>
        public bool Knows(string hostname, string keyType, byte[] pubKey, UInt16 port)
        {
            bool foundMatch = false;

            foreach (KnownHost host in _knownHosts)
            {
                KnownHost.HostValidationResponse typeOfMatch = host.MatchesPubKey(hostname, keyType, pubKey, port);

                if(typeOfMatch == KnownHost.HostValidationResponse.KeyRevoked)
                    throw new RevokedKeyException("The given host-pubkey pair is marked as revoked");

                foundMatch = foundMatch || (typeOfMatch == KnownHost.HostValidationResponse.Matches);
            }

            return foundMatch;
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography;
using System;
using System.Text;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class KnownHostTest
    {
        private const string ExampleHost1 = "example.com";
        private const string ExampleHost2 = "test.web.net";
        private const string ExampleAlgo = "some-key-algorithm";

        private static readonly byte[] ExampleKey = new byte[]
        {
            0x00, 0x11, 0x22, 0x33, 0x44,
            0x55, 0x66, 0x77, 0x88, 0x99,
            0xaa, 0xbb, 0xcc, 0xdd, 0xee,
            0xff
        };
        private static readonly string Base64ExampleKey = Convert.ToBase64String(ExampleKey);

        private static readonly byte[] ExampleSalt = new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04,
            0x05, 0x06, 0x07, 0x08, 0x09,
            0x0a, 0x0b, 0x0c, 0x0d, 0x0e,
            0x0f, 0x10, 0x11, 0x12, 0x13
        };
        private static readonly string Base64ExampleSalt = Convert.ToBase64String(ExampleSalt);
        private const string ExampleHostHashed = "nnUK16ANsXd3hL31YfAkGOluSjU=";

        [TestMethod]
        public void KnownHostCanParsePlaintextHostEntry()
        {
            string expectedPlaintextHostEntry =
                string.Format("{0} {1} {2}", ExampleHost1, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedPlaintextHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostCanParseHashedHostEntry()
        {
            string expectedHashedHostEntry =
                string.Format("|1|{0}|{1} {2} {3}", Base64ExampleSalt, ExampleHostHashed, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsTrue(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedHashedHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostCanParsePlaintextHostEntryWithTrailingComments()
        {
            string expectedPlaintextHostEntry =
                string.Format("{0} {1} {2} nonagon infinity opens the door", ExampleHost1, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
        }

        [TestMethod]
        public void KnownHostCanParseHashedHostEntryWithTrailingComments()
        {
            string expectedHashedHostEntry =
                string.Format("|1|{0}|{1} {2} {3} wait for the answer to open the door", Base64ExampleSalt, ExampleHostHashed, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsTrue(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
        }

        [TestMethod]
        public void KnownHostRejectsInvalidHost()
        {
            string expectedPlaintextHostEntry =
                string.Format("{0} {1} {2}", ExampleHost1, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.DoesNoMatch, parsedHost.MatchesPubKey("MaliciousHost.evil", ExampleAlgo, ExampleKey, 22));
        }

        [TestMethod]
        public void KnownHostRejectsInvalidKeyAlgo()
        {
            string expectedPlaintextHostEntry =
                string.Format("{0} {1} {2}", ExampleHost1, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.DoesNoMatch, parsedHost.MatchesPubKey(ExampleHost1, "failingAlgo", ExampleKey, 22));
        }

        [TestMethod]
        public void KnownHostRejectsInvalidKey()
        {
            string expectedPlaintextHostEntry =
                string.Format("{0} {1} {2}", ExampleHost1, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.DoesNoMatch, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, new byte[]{0x00}, 22));
        }

        [TestMethod]
        public void KnownHostRejectsInvalidPort()
        {
            string expectedPlaintextHostEntry =
                string.Format("{0} {1} {2}", ExampleHost1, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.DoesNoMatch, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 9001));
        }

        [TestMethod]
        public void KnownHostCanParsePlaintextHostEntryWithNonstandardPort()
        {
            string expectedPlaintextHostEntry =
                string.Format("[{0}:9001] {1} {2}", ExampleHost1, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.DoesNoMatch, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 9001));
            Assert.AreEqual(expectedPlaintextHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostCanParsePlaintextHostEntryWithBeginningGlob()
        {
            string expectedPlaintextHostEntry =
                string.Format("*.com {0} {1}", ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedPlaintextHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostCanParsePlaintextHostEntryWithEndingGlob()
        {
            string expectedPlaintextHostEntry =
                string.Format("example.* {0} {1}", ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedPlaintextHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostCanParsePlaintextHostEntryWithMultipleGlobs()
        {
            string expectedPlaintextHostEntry =
                string.Format("ex*le.* {0} {1}", ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedPlaintextHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostCanParseMultipleHostPatterns()
        {

            string expectedPlaintextHostEntry =
                string.Format("{0},{1} {2} {3}", ExampleHost1, ExampleHost2, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost2, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedPlaintextHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostSupportsExcludedPatterns()
        {

            string expectedPlaintextHostEntry =
                string.Format("example.*,!*.com {0} {1}", ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.DoesNoMatch, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey("example.net", ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedPlaintextHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostSupportsKeyRevocationForPlaintextHost()
        {
            string expectedPlaintextHostEntry =
                string.Format("@revoked {0} {1} {2}", ExampleHost1, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.KeyRevoked, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedPlaintextHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostSupportsKeyRevocationForHashedHost()
        {
            string expectedHashedHostEntry =
                string.Format("@revoked |1|{0}|{1} {2} {3}", Base64ExampleSalt, ExampleHostHashed, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsTrue(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.KeyRevoked, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedHashedHostEntry, parsedHost.ToString());
        }


        [TestMethod]
        public void KnownHostRecognizesCertAuthForPlaintextHost()
        {
            string expectedPlaintextHostEntry =
                string.Format("@cert-authority {0} {1} {2}", ExampleHost1, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsTrue(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.InvalidSignature, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedPlaintextHostEntry, parsedHost.ToString());
        }


        [TestMethod]
        public void KnownHostRecognizesCertAuthForHashedHost()
        {
            string expectedHashedHostEntry =
                string.Format("@cert-authority |1|{0}|{1} {2} {3}", Base64ExampleSalt, ExampleHostHashed, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsTrue(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.InvalidSignature, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedHashedHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostcanParseHasedHostEntryWithNonstandardPort()
        {
            string hostWithNonStandardPort = string.Format("[{0}:9001]", ExampleHost1);
            HMACSHA1 hmac = new HMACSHA1(ExampleSalt);
            byte[] hashedHost  = hmac.ComputeHash(Encoding.ASCII.GetBytes(hostWithNonStandardPort));

            string expectedHashedHostEntry =
                string.Format("|1|{0}|{1} {2} {3}", Base64ExampleSalt, Convert.ToBase64String(hashedHost), ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsTrue(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 9001));
            Assert.AreEqual(expectedHashedHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostcanParseHasedHostEntryWithRedundantPort()
        {
            string hostWithNonStandardPort = string.Format("[{0}:22]", ExampleHost1);
            HMACSHA1 hmac = new HMACSHA1(ExampleSalt);
            byte[] hashedHost = hmac.ComputeHash(Encoding.ASCII.GetBytes(hostWithNonStandardPort));

            string expectedHashedHostEntry =
                string.Format("|1|{0}|{1} {2} {3}", Base64ExampleSalt, Convert.ToBase64String(hashedHost), ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsTrue(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
            Assert.AreEqual(KnownHost.HostValidationResponse.Matches, parsedHost.MatchesPubKey(ExampleHost1, ExampleAlgo, ExampleKey, 22));
            Assert.AreEqual(expectedHashedHostEntry, parsedHost.ToString());
        }

        [TestMethod]
        public void KnownHostFailsToParseMalformedKey()
        {
            string expectedPlaintextHostEntry =
                string.Format("{0} {1} {2}", ExampleHost1, ExampleAlgo, "This_Key_Not_Base_64");

            KnownHost parsedHost;

            Assert.IsFalse(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
        }

        [TestMethod]
        public void KnownHostFailsIfSectionMissing()
        {
            string expectedPlaintextHostEntry =
                string.Format("{0} {1}", ExampleHost1, ExampleAlgo);

            KnownHost parsedHost;

            Assert.IsFalse(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
        }

        [TestMethod]
        public void KnownHostFailsToParseEmptyLine()
        {
            string expectedPlaintextHostEntry = String.Empty;

            KnownHost parsedHost;

            Assert.IsFalse(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
        }

        [TestMethod]
        public void KnownHostFailsToParseCommentLine()
        {
            string expectedPlaintextHostEntry =
                string.Format("#{0} {1} {2}", ExampleHost1, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsFalse(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
        }

        [TestMethod]
        public void KnownHostFailsToParseLineWithOperandAndMissingSecion()
        {
            string expectedPlaintextHostEntry =
                string.Format("@cert-authority {0} {1}", ExampleHost1, ExampleAlgo);

            KnownHost parsedHost;

            Assert.IsFalse(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
        }

        [TestMethod]
        public void KnownHostFailsToParseInvalidOperand()
        {
            string expectedPlaintextHostEntry =
                string.Format("@fail-parsing {0} {1} {2}", ExampleHost1, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;

            Assert.IsFalse(KnownHost.TryParse(expectedPlaintextHostEntry, out parsedHost));
        }

        [TestMethod]
        public void KnownHostFailsToParseInvalidHashType()
        {
            string expectedHashedHostEntry =
                string.Format("|2|{0}|{1} {2} {3}", Base64ExampleSalt, ExampleHostHashed, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsFalse(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
        }

        [TestMethod]
        public void KnownHostFailsToParseMalformedSalt()
        {
            string expectedHashedHostEntry =
                string.Format("|1|{0}|{1} {2} {3}", "not_base_64", ExampleHostHashed, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsFalse(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
        }

        [TestMethod]
        public void KnownHostFailsToParseMalformedHash()
        {
            string expectedHashedHostEntry =
                string.Format("|1|{0}|{1} {2} {3}", Base64ExampleSalt, "not_base_64", ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsFalse(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
        }


        [TestMethod]
        public void KnownHostFailsToParseIfHashSectionMissing()
        {
            string expectedHashedHostEntry =
                string.Format("|1|{0} {1} {2}", Base64ExampleSalt, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsFalse(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
        }


        [TestMethod]
        public void KnownHostFailsToParseSaltTooShort()
        {
            byte[] shortSalt = new byte[19];

            string expectedHashedHostEntry =
                string.Format("|1|{0}|{1} {2} {3}", Convert.ToBase64String(shortSalt), ExampleHostHashed, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsFalse(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
        }

        [TestMethod]
        public void KnownHostFailsToParseSaltTooLong()
        {
            byte[] longSalt = new byte[21];

            string expectedHashedHostEntry =
                string.Format("|1|{0}|{1} {2} {3}", Convert.ToBase64String(longSalt), ExampleHostHashed, ExampleAlgo, Base64ExampleKey);

            KnownHost parsedHost;
            Assert.IsFalse(KnownHost.TryParse(expectedHashedHostEntry, out parsedHost));
        }
    }
}

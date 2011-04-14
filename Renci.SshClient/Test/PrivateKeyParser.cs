using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Test
{
    public class opensslkey
    {

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern IntPtr CertCreateSelfSignCertificate(
        IntPtr hProv,
        ref CERT_NAME_BLOB pSubjectIssuerBlob,
        uint dwFlagsm,
        ref CRYPT_KEY_PROV_INFO pKeyProvInfo,
        IntPtr pSignatureAlgorithm,
        IntPtr pStartTime,
        IntPtr pEndTime,
        IntPtr other);


        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CertStrToName(
         uint dwCertEncodingType,
         String pszX500,
         uint dwStrType,
         IntPtr pvReserved,
         [In, Out] byte[] pbEncoded,
         ref uint pcbEncoded,
         IntPtr other);

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CertFreeCertificateContext(
        IntPtr hCertStore);

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_KEY_PROV_INFO
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public String pwszContainerName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public String pwszProvName;
            public uint dwProvType;
            public uint dwFlags;
            public uint cProvParam;
            public IntPtr rgProvParam;
            public uint dwKeySpec;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CERT_NAME_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        const String pemprivheader = "-----BEGIN RSA PRIVATE KEY-----";
        const String pemprivfooter = "-----END RSA PRIVATE KEY-----";
        const String pempubheader = "-----BEGIN PUBLIC KEY-----";
        const String pempubfooter = "-----END PUBLIC KEY-----";
        const String pemp8header = "-----BEGIN PRIVATE KEY-----";
        const String pemp8footer = "-----END PRIVATE KEY-----";
        const String pemp8encheader = "-----BEGIN ENCRYPTED PRIVATE KEY-----";
        const String pemp8encfooter = "-----END ENCRYPTED PRIVATE KEY-----";

        // static byte[] pempublickey;
        // static byte[] pemprivatekey;
        // static byte[] pkcs8privatekey;
        // static byte[] pkcs8encprivatekey;

        static bool verbose = false;

        //public static void Main(String[] args) 
        //{

        // if(args.Length == 1)
        //   if(args[0].ToUpper() == "V")
        //       verbose = true;

        // Console.ForegroundColor = ConsoleColor.Gray;
        // Console.Write("\nRSA public, private or PKCS #8  key file to decode: ");
        // String filename = Console.ReadLine().Trim();
        // if (filename == "")  //exit while(true) loop
        //   return;
        //     if (!File.Exists(filename)) {
        //        Console.WriteLine("File \"{0}\" does not exist!\n", filename);
        //        return;	
        //     }

        //   StreamReader sr = File.OpenText(filename);
        //   String pemstr = sr.ReadToEnd().Trim();
        //   sr.Close();
        //   if(pemstr.StartsWith("-----BEGIN"))
        //       DecodePEMKey(pemstr);
        //   else
        //       DecodeDERKey(filename);
        // }





        // ------- Decode PEM pubic, private or pkcs8 key ----------------

        public static void DecodePEMKey(String pemstr)
        {
            byte[] pempublickey;
            byte[] pemprivatekey;
            byte[] pkcs8privatekey;
            byte[] pkcs8encprivatekey;

            if (pemstr.StartsWith(pempubheader) && pemstr.EndsWith(pempubfooter))
            {
                Console.WriteLine("Trying to decode and parse a PEM public key ..");
                pempublickey = DecodeOpenSSLPublicKey(pemstr);
                if (pempublickey != null)
                {
                    if (verbose)
                        showBytes("\nRSA public key", pempublickey);
                    //PutFileBytes("rsapubkey.pem", pempublickey, pempublickey.Length) ;
                    RSACryptoServiceProvider rsa = DecodeX509PublicKey(pempublickey);
                    Console.WriteLine("\nCreated an RSACryptoServiceProvider instance\n");
                    String xmlpublickey = rsa.ToXmlString(false);
                    Console.WriteLine("\nXML RSA public key:  {0} bits\n{1}\n", rsa.KeySize, xmlpublickey);
                }
            }




            else if (pemstr.StartsWith(pemprivheader) && pemstr.EndsWith(pemprivfooter))
            {
                Console.WriteLine("Trying to decrypt and parse a PEM private key ..");
                pemprivatekey = DecodeOpenSSLPrivateKey(pemstr);
                if (pemprivatekey != null)
                {
                    if (verbose)
                        showBytes("\nRSA private key", pemprivatekey);
                    //PutFileBytes("rsaprivkey.pem", pemprivatekey, pemprivatekey.Length) ;
                    RSACryptoServiceProvider rsa = DecodeRSAPrivateKey(pemprivatekey);
                    Console.WriteLine("\nCreated an RSACryptoServiceProvider instance\n");
                    String xmlprivatekey = rsa.ToXmlString(true);
                    Console.WriteLine("\nXML RSA private key:  {0} bits\n{1}\n", rsa.KeySize, xmlprivatekey);
                    ProcessRSA(rsa);
                }
            }



            else if (pemstr.StartsWith(pemp8header) && pemstr.EndsWith(pemp8footer))
            {
                Console.WriteLine("Trying to decode and parse as PEM PKCS #8 PrivateKeyInfo ..");
                pkcs8privatekey = DecodePkcs8PrivateKey(pemstr);
                if (pkcs8privatekey != null)
                {
                    if (verbose)
                        showBytes("\nPKCS #8 PrivateKeyInfo", pkcs8privatekey);
                    //PutFileBytes("PrivateKeyInfo", pkcs8privatekey, pkcs8privatekey.Length) ;
                    RSACryptoServiceProvider rsa = DecodePrivateKeyInfo(pkcs8privatekey);
                    if (rsa != null)
                    {
                        Console.WriteLine("\nCreated an RSACryptoServiceProvider instance\n");
                        String xmlprivatekey = rsa.ToXmlString(true);
                        Console.WriteLine("\nXML RSA private key:  {0} bits\n{1}\n", rsa.KeySize, xmlprivatekey);
                        ProcessRSA(rsa);
                    }
                    else
                        Console.WriteLine("\nFailed to create an RSACryptoServiceProvider");
                }
            }


            else if (pemstr.StartsWith(pemp8encheader) && pemstr.EndsWith(pemp8encfooter))
            {
                Console.WriteLine("Trying to decode and parse as PEM PKCS #8 EncryptedPrivateKeyInfo ..");
                pkcs8encprivatekey = DecodePkcs8EncPrivateKey(pemstr);
                if (pkcs8encprivatekey != null)
                {
                    if (verbose)
                        showBytes("\nPKCS #8 EncryptedPrivateKeyInfo", pkcs8encprivatekey);
                    //PutFileBytes("EncryptedPrivateKeyInfo", pkcs8encprivatekey, pkcs8encprivatekey.Length) ;
                    RSACryptoServiceProvider rsa = DecodeEncryptedPrivateKeyInfo(pkcs8encprivatekey);
                    if (rsa != null)
                    {
                        Console.WriteLine("\nCreated an RSACryptoServiceProvider instance\n");
                        String xmlprivatekey = rsa.ToXmlString(true);
                        Console.WriteLine("\nXML RSA private key:  {0} bits\n{1}\n", rsa.KeySize, xmlprivatekey);
                        ProcessRSA(rsa);
                    }
                    else
                        Console.WriteLine("\nFailed to create an RSACryptoServiceProvider");
                }
            }


            else
            {
                Console.WriteLine("Not a PEM public, private key or a PKCS #8");
                return;
            }
        }

        // ------- Decode PEM pubic, private or pkcs8 key ----------------
        public static void DecodeDERKey(String filename)
        {
            RSACryptoServiceProvider rsa = null;
            byte[] keyblob = GetFileBytes(filename);
            if (keyblob == null)
                return;

            rsa = DecodeX509PublicKey(keyblob);
            if (rsa != null)
            {
                Console.WriteLine("\nA valid SubjectPublicKeyInfo\n");
                Console.WriteLine("\nCreated an RSACryptoServiceProvider instance\n");
                String xmlpublickey = rsa.ToXmlString(false);
                Console.WriteLine("\nXML RSA public key:  {0} bits\n{1}\n", rsa.KeySize, xmlpublickey);
                return;
            }

            rsa = DecodeRSAPrivateKey(keyblob);
            if (rsa != null)
            {
                Console.WriteLine("\nA valid RSAPrivateKey\n");
                Console.WriteLine("\nCreated an RSACryptoServiceProvider instance\n");
                String xmlprivatekey = rsa.ToXmlString(true);
                Console.WriteLine("\nXML RSA private key:  {0} bits\n{1}\n", rsa.KeySize, xmlprivatekey);
                ProcessRSA(rsa);
                return;
            }

            rsa = DecodePrivateKeyInfo(keyblob);	//PKCS #8 unencrypted
            if (rsa != null)
            {
                Console.WriteLine("\nA valid PKCS #8 PrivateKeyInfo\n");
                Console.WriteLine("\nCreated an RSACryptoServiceProvider instance\n");
                String xmlprivatekey = rsa.ToXmlString(true);
                Console.WriteLine("\nXML RSA private key:  {0} bits\n{1}\n", rsa.KeySize, xmlprivatekey);
                ProcessRSA(rsa);
                return;
            }

            rsa = DecodeEncryptedPrivateKeyInfo(keyblob);	//PKCS #8 encrypted
            if (rsa != null)
            {
                Console.WriteLine("\nA valid PKCS #8 EncryptedPrivateKeyInfo\n");
                Console.WriteLine("\nCreated an RSACryptoServiceProvider instance\n");
                String xmlprivatekey = rsa.ToXmlString(true);
                Console.WriteLine("\nXML RSA private key:  {0} bits\n{1}\n", rsa.KeySize, xmlprivatekey);
                ProcessRSA(rsa);
                return;
            }
            Console.WriteLine("Not a binary DER public, private or PKCS #8 key");
            return;
        }

        public static void ProcessRSA(RSACryptoServiceProvider rsa)
        {
            if (verbose)
                showRSAProps(rsa);
            Console.Write("\n\nExport RSA private key to PKCS #12 file?  (Y or N) ");
            String resp = Console.ReadLine().ToUpper();
            if (resp == "Y" || resp == "YES")
                RSAtoPKCS12(rsa);
        }

        //--------  Generate pkcs #12 from an RSACryptoServiceProvider  ---------
        public static void RSAtoPKCS12(RSACryptoServiceProvider rsa)
        {
            CspKeyContainerInfo keyInfo = rsa.CspKeyContainerInfo;
            String keycontainer = keyInfo.KeyContainerName;
            uint keyspec = (uint)keyInfo.KeyNumber;
            String provider = keyInfo.ProviderName;
            uint cspflags = 0;  //CryptoAPI Current User store;   LM would be CRYPT_MACHINE_KEYSET	= 0x00000020
            String fname = keycontainer + ".p12";
            //---- need to pass in rsa since underlying keycontainer is not persisted and might be deleted too quickly ---
            byte[] pkcs12 = GetPkcs12(rsa, keycontainer, provider, keyspec, cspflags);
            if ((pkcs12 != null) && verbose)
                showBytes("\npkcs #12", pkcs12);
            if (pkcs12 != null)
            {
                PutFileBytes(fname, pkcs12, pkcs12.Length);
                Console.WriteLine("\nWrote pkc #12 file '{0}'\n", fname);
            }
            else
                Console.WriteLine("\nProblem getting pkcs#12");
        }

        //--------   Get the binary PKCS #8 PRIVATE key   --------
        public static byte[] DecodePkcs8PrivateKey(String instr)
        {
            const String pemp8header = "-----BEGIN PRIVATE KEY-----";
            const String pemp8footer = "-----END PRIVATE KEY-----";
            String pemstr = instr.Trim();
            byte[] binkey;
            if (!pemstr.StartsWith(pemp8header) || !pemstr.EndsWith(pemp8footer))
                return null;
            StringBuilder sb = new StringBuilder(pemstr);
            sb.Replace(pemp8header, "");  //remove headers/footers, if present
            sb.Replace(pemp8footer, "");

            String pubstr = sb.ToString().Trim();	//get string after removing leading/trailing whitespace

            try
            {
                binkey = Convert.FromBase64String(pubstr);
            }
            catch (System.FormatException)
            {		//if can't b64 decode, data is not valid
                return null;
            }
            return binkey;
        }

        //------- Parses binary asn.1 PKCS #8 PrivateKeyInfo; returns RSACryptoServiceProvider ---
        public static RSACryptoServiceProvider DecodePrivateKeyInfo(byte[] pkcs8)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            // this byte[] includes the sequence byte and terminal encoded null 
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            MemoryStream mem = new MemoryStream(pkcs8);
            int lenstream = (int)mem.Length;
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;

            try
            {

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;


                bt = binr.ReadByte();
                if (bt != 0x02)
                    return null;

                twobytes = binr.ReadUInt16();

                if (twobytes != 0x0001)
                    return null;

                seq = binr.ReadBytes(15);		//read the Sequence OID
                if (!CompareBytearrays(seq, SeqOID))	//make sure Sequence for OID is correct
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x04)	//expect an Octet string 
                    return null;

                bt = binr.ReadByte();		//read next byte, or next 2 bytes is  0x81 or 0x82; otherwise bt is the byte count
                if (bt == 0x81)
                    binr.ReadByte();
                else
                    if (bt == 0x82)
                        binr.ReadUInt16();
                //------ at this stage, the remaining sequence should be the RSA private key

                byte[] rsaprivkey = binr.ReadBytes((int)(lenstream - mem.Position));
                RSACryptoServiceProvider rsacsp = DecodeRSAPrivateKey(rsaprivkey);
                return rsacsp;
            }

            catch (Exception)
            {
                return null;
            }

            finally { binr.Close(); }

        }

        //--------   Get the binary PKCS #8 Encrypted PRIVATE key   --------
        public static byte[] DecodePkcs8EncPrivateKey(String instr)
        {
            const String pemp8encheader = "-----BEGIN ENCRYPTED PRIVATE KEY-----";
            const String pemp8encfooter = "-----END ENCRYPTED PRIVATE KEY-----";
            String pemstr = instr.Trim();
            byte[] binkey;
            if (!pemstr.StartsWith(pemp8encheader) || !pemstr.EndsWith(pemp8encfooter))
                return null;
            StringBuilder sb = new StringBuilder(pemstr);
            sb.Replace(pemp8encheader, "");  //remove headers/footers, if present
            sb.Replace(pemp8encfooter, "");

            String pubstr = sb.ToString().Trim();	//get string after removing leading/trailing whitespace

            try
            {
                binkey = Convert.FromBase64String(pubstr);
            }
            catch (System.FormatException)
            {		//if can't b64 decode, data is not valid
                return null;
            }
            return binkey;
        }

        //------- Parses binary asn.1 EncryptedPrivateKeyInfo; returns RSACryptoServiceProvider ---
        public static RSACryptoServiceProvider DecodeEncryptedPrivateKeyInfo(byte[] encpkcs8)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            // this byte[] includes the sequence byte and terminal encoded null 
            byte[] OIDpkcs5PBES2 = { 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x05, 0x0D };
            byte[] OIDpkcs5PBKDF2 = { 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x05, 0x0C };
            byte[] OIDdesEDE3CBC = { 0x06, 0x08, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x03, 0x07 };
            byte[] seqdes = new byte[10];
            byte[] seq = new byte[11];
            byte[] salt;
            byte[] IV;
            byte[] encryptedpkcs8;
            byte[] pkcs8;

            int saltsize, ivsize, encblobsize;
            int iterations;

            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            MemoryStream mem = new MemoryStream(encpkcs8);
            int lenstream = (int)mem.Length;
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;

            try
            {

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();	//inner sequence
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();


                seq = binr.ReadBytes(11);		//read the Sequence OID
                if (!CompareBytearrays(seq, OIDpkcs5PBES2))	//is it a OIDpkcs5PBES2 ?
                    return null;

                twobytes = binr.ReadUInt16();	//inner sequence for pswd salt
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();

                twobytes = binr.ReadUInt16();	//inner sequence for pswd salt
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();

                seq = binr.ReadBytes(11);		//read the Sequence OID
                if (!CompareBytearrays(seq, OIDpkcs5PBKDF2))	//is it a OIDpkcs5PBKDF2 ?
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();

                bt = binr.ReadByte();
                if (bt != 0x04)		//expect octet string for salt
                    return null;
                saltsize = binr.ReadByte();
                salt = binr.ReadBytes(saltsize);

                if (verbose)
                    showBytes("Salt for pbkd", salt);
                bt = binr.ReadByte();
                if (bt != 0x02) 	//expect an integer for PBKF2 interation count
                    return null;

                int itbytes = binr.ReadByte();	//PBKD2 iterations should fit in 2 bytes.
                if (itbytes == 1)
                    iterations = binr.ReadByte();
                else if (itbytes == 2)
                    iterations = 256 * binr.ReadByte() + binr.ReadByte();
                else
                    return null;
                if (verbose)
                    Console.WriteLine("PBKD2 iterations {0}", iterations);

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();


                seqdes = binr.ReadBytes(10);		//read the Sequence OID
                if (!CompareBytearrays(seqdes, OIDdesEDE3CBC))	//is it a OIDdes-EDE3-CBC ?
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x04)		//expect octet string for IV
                    return null;
                ivsize = binr.ReadByte();	// IV byte size should fit in one byte (24 expected for 3DES)
                IV = binr.ReadBytes(ivsize);
                if (verbose)
                    showBytes("IV for des-EDE3-CBC", IV);

                bt = binr.ReadByte();
                if (bt != 0x04)		// expect octet string for encrypted PKCS8 data
                    return null;


                bt = binr.ReadByte();

                if (bt == 0x81)
                    encblobsize = binr.ReadByte();	// data size in next byte
                else if (bt == 0x82)
                    encblobsize = 256 * binr.ReadByte() + binr.ReadByte();
                else
                    encblobsize = bt;		// we already have the data size


                encryptedpkcs8 = binr.ReadBytes(encblobsize);
                //if(verbose)
                //	showBytes("Encrypted PKCS8 blob", encryptedpkcs8) ;


                SecureString secpswd = GetSecPswd("Enter password for Encrypted PKCS #8 ==>");
                pkcs8 = DecryptPBDK2(encryptedpkcs8, salt, IV, secpswd, iterations);
                if (pkcs8 == null)	// probably a bad pswd entered.
                    return null;

                //if(verbose)
                //	showBytes("Decrypted PKCS #8", pkcs8) ;
                //----- With a decrypted pkcs #8 PrivateKeyInfo blob, decode it to an RSA ---
                RSACryptoServiceProvider rsa = DecodePrivateKeyInfo(pkcs8);
                return rsa;
            }

            catch (Exception)
            {
                return null;
            }

            finally { binr.Close(); }


        }

        //  ------  Uses PBKD2 to derive a 3DES key and decrypts data --------
        public static byte[] DecryptPBDK2(byte[] edata, byte[] salt, byte[] IV, SecureString secpswd, int iterations)
        {
            CryptoStream decrypt = null;

            IntPtr unmanagedPswd = IntPtr.Zero;
            byte[] psbytes = new byte[secpswd.Length];
            unmanagedPswd = Marshal.SecureStringToGlobalAllocAnsi(secpswd);
            Marshal.Copy(unmanagedPswd, psbytes, 0, psbytes.Length);
            Marshal.ZeroFreeGlobalAllocAnsi(unmanagedPswd);

            try
            {
                Rfc2898DeriveBytes kd = new Rfc2898DeriveBytes(psbytes, salt, iterations);
                TripleDES decAlg = TripleDES.Create();
                decAlg.Key = kd.GetBytes(24);
                decAlg.IV = IV;
                MemoryStream memstr = new MemoryStream();
                decrypt = new CryptoStream(memstr, decAlg.CreateDecryptor(), CryptoStreamMode.Write);
                decrypt.Write(edata, 0, edata.Length);
                decrypt.Flush();
                decrypt.Close();	// this is REQUIRED.
                byte[] cleartext = memstr.ToArray();
                return cleartext;
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem decrypting: {0}", e.Message);
                return null;
            }
        }

        //--------   Get the binary RSA PUBLIC key   --------
        public static byte[] DecodeOpenSSLPublicKey(String instr)
        {
            const String pempubheader = "-----BEGIN PUBLIC KEY-----";
            const String pempubfooter = "-----END PUBLIC KEY-----";
            String pemstr = instr.Trim();
            byte[] binkey;
            if (!pemstr.StartsWith(pempubheader) || !pemstr.EndsWith(pempubfooter))
                return null;
            StringBuilder sb = new StringBuilder(pemstr);
            sb.Replace(pempubheader, "");  //remove headers/footers, if present
            sb.Replace(pempubfooter, "");

            String pubstr = sb.ToString().Trim();	//get string after removing leading/trailing whitespace

            try
            {
                binkey = Convert.FromBase64String(pubstr);
            }
            catch (System.FormatException)
            {		//if can't b64 decode, data is not valid
                return null;
            }
            return binkey;
        }

        //------- Parses binary asn.1 X509 SubjectPublicKeyInfo; returns RSACryptoServiceProvider ---
        public static RSACryptoServiceProvider DecodeX509PublicKey(byte[] x509key)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            MemoryStream mem = new MemoryStream(x509key);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;

            try
            {

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                seq = binr.ReadBytes(15);		//read the Sequence OID
                if (!CompareBytearrays(seq, SeqOID))	//make sure Sequence for OID is correct
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8103)	//data read as little endian order (actual data order for Bit String is 03 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8203)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x00)		//expect null byte next
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                byte lowbyte = 0x00;
                byte highbyte = 0x00;

                if (twobytes == 0x8102)	//data read as little endian order (actual data order for Integer is 02 81)
                    lowbyte = binr.ReadByte();	// read next bytes which is bytes in modulus
                else if (twobytes == 0x8202)
                {
                    highbyte = binr.ReadByte();	//advance 2 bytes
                    lowbyte = binr.ReadByte();
                }
                else
                    return null;
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                int modsize = BitConverter.ToInt32(modint, 0);

                byte firstbyte = binr.ReadByte();
                binr.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstbyte == 0x00)
                {	//if first byte (highest order) of modulus is zero, don't include it
                    binr.ReadByte();	//skip this null byte
                    modsize -= 1;	//reduce modulus buffer size by 1
                }

                byte[] modulus = binr.ReadBytes(modsize);	//read the modulus bytes

                if (binr.ReadByte() != 0x02)			//expect an Integer for the exponent data
                    return null;
                int expbytes = (int)binr.ReadByte();		// should only need one byte for actual exponent data (for all useful values)
                byte[] exponent = binr.ReadBytes(expbytes);


                showBytes("\nExponent", exponent);
                showBytes("\nModulus", modulus);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAKeyInfo = new RSAParameters();
                RSAKeyInfo.Modulus = modulus;
                RSAKeyInfo.Exponent = exponent;
                RSA.ImportParameters(RSAKeyInfo);
                return RSA;
            }
            catch (Exception)
            {
                return null;
            }

            finally { binr.Close(); }

        }

        //------- Parses binary ans.1 RSA private key; returns RSACryptoServiceProvider  ---
        public static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)	//version number
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;


                //------  all private key components are Integer sequences ----
                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                Console.WriteLine("showing components ..");
                if (verbose)
                {
                    showBytes("\nModulus", MODULUS);
                    showBytes("\nExponent", E);
                    showBytes("\nD", D);
                    showBytes("\nP", P);
                    showBytes("\nQ", Q);
                    showBytes("\nDP", DP);
                    showBytes("\nDQ", DQ);
                    showBytes("\nIQ", IQ);
                }

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAparams = new RSAParameters();
                RSAparams.Modulus = MODULUS;
                RSAparams.Exponent = E;
                RSAparams.D = D;
                RSAparams.P = P;
                RSAparams.Q = Q;
                RSAparams.DP = DP;
                RSAparams.DQ = DQ;
                RSAparams.InverseQ = IQ;
                RSA.ImportParameters(RSAparams);
                return RSA;
            }
            catch (Exception)
            {
                return null;
            }
            finally { binr.Close(); }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)		//expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();	// data size in next byte
            else
                if (bt == 0x82)
                {
                    highbyte = binr.ReadByte();	// data size in next 2 bytes
                    lowbyte = binr.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                }
                else
                {
                    count = bt;		// we already have the data size
                }



            while (binr.ReadByte() == 0x00)
            {	//remove high order zeros in data
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);		//last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }

        //-----  Get the binary RSA PRIVATE key, decrypting if necessary ----
        public static byte[] DecodeOpenSSLPrivateKey(String instr)
        {
            const String pemprivheader = "-----BEGIN RSA PRIVATE KEY-----";
            const String pemprivfooter = "-----END RSA PRIVATE KEY-----";
            String pemstr = instr.Trim();
            byte[] binkey;
            if (!pemstr.StartsWith(pemprivheader) || !pemstr.EndsWith(pemprivfooter))
                return null;

            StringBuilder sb = new StringBuilder(pemstr);
            sb.Replace(pemprivheader, "");  //remove headers/footers, if present
            sb.Replace(pemprivfooter, "");

            String pvkstr = sb.ToString().Trim();	//get string after removing leading/trailing whitespace

            try
            {        // if there are no PEM encryption info lines, this is an UNencrypted PEM private key
                binkey = Convert.FromBase64String(pvkstr);
                return binkey;
            }
            catch (System.FormatException)
            {		//if can't b64 decode, it must be an encrypted private key
                //Console.WriteLine("Not an unencrypted OpenSSL PEM private key");  
            }

            StringReader str = new StringReader(pvkstr);

            //-------- read PEM encryption info. lines and extract salt -----
            if (!str.ReadLine().StartsWith("Proc-Type: 4,ENCRYPTED"))
                return null;
            String saltline = str.ReadLine();
            if (!saltline.StartsWith("DEK-Info: DES-EDE3-CBC,"))
                return null;
            String saltstr = saltline.Substring(saltline.IndexOf(",") + 1).Trim();
            byte[] salt = new byte[saltstr.Length / 2];
            for (int i = 0; i < salt.Length; i++)
                salt[i] = Convert.ToByte(saltstr.Substring(i * 2, 2), 16);
            if (!(str.ReadLine() == ""))
                return null;

            //------ remaining b64 data is encrypted RSA key ----
            String encryptedstr = str.ReadToEnd();

            try
            {	//should have b64 encrypted RSA key now
                binkey = Convert.FromBase64String(encryptedstr);
            }
            catch (System.FormatException)
            {  // bad b64 data.
                return null;
            }

            //------ Get the 3DES 24 byte key using PDK used by OpenSSL ----

            SecureString despswd = GetSecPswd("Enter password to derive 3DES key==>");
            //Console.Write("\nEnter password to derive 3DES key: ");
            //String pswd = Console.ReadLine();
            byte[] deskey = GetOpenSSL3deskey(salt, despswd, 1, 2);    // count=1 (for OpenSSL implementation); 2 iterations to get at least 24 bytes

            if (deskey == null)
                return null;
            //showBytes("3DES key", deskey) ;

            //------ Decrypt the encrypted 3des-encrypted RSA private key ------
            byte[] rsakey = DecryptKey(binkey, deskey, salt);	//OpenSSL uses salt value in PEM header also as 3DES IV
            if (rsakey != null)
                return rsakey;	//we have a decrypted RSA private key
            else
            {
                Console.WriteLine("Failed to decrypt RSA private key; probably wrong password.");
                return null;
            }
        }

        // ----- Decrypt the 3DES encrypted RSA private key ----------
        public static byte[] DecryptKey(byte[] cipherData, byte[] desKey, byte[] IV)
        {
            MemoryStream memst = new MemoryStream();
            TripleDES alg = TripleDES.Create();
            alg.Key = desKey;
            alg.IV = IV;
            try
            {
                CryptoStream cs = new CryptoStream(memst, alg.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(cipherData, 0, cipherData.Length);
                cs.Close();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return null;
            }
            byte[] decryptedData = memst.ToArray();
            return decryptedData;
        }

        //-----   OpenSSL PBKD uses only one hash cycle (count); miter is number of iterations required to build sufficient bytes ---
        private static byte[] GetOpenSSL3deskey(byte[] salt, SecureString secpswd, int count, int miter)
        {
            IntPtr unmanagedPswd = IntPtr.Zero;
            int HASHLENGTH = 16;	//MD5 bytes
            byte[] keymaterial = new byte[HASHLENGTH * miter];     //to store contatenated Mi hashed results


            byte[] psbytes = new byte[secpswd.Length];
            unmanagedPswd = Marshal.SecureStringToGlobalAllocAnsi(secpswd);
            Marshal.Copy(unmanagedPswd, psbytes, 0, psbytes.Length);
            Marshal.ZeroFreeGlobalAllocAnsi(unmanagedPswd);

            //UTF8Encoding utf8 = new UTF8Encoding();
            //byte[] psbytes = utf8.GetBytes(pswd);

            // --- contatenate salt and pswd bytes into fixed data array ---
            byte[] data00 = new byte[psbytes.Length + salt.Length];
            Array.Copy(psbytes, data00, psbytes.Length);		//copy the pswd bytes
            Array.Copy(salt, 0, data00, psbytes.Length, salt.Length);	//concatenate the salt bytes

            // ---- do multi-hashing and contatenate results  D1, D2 ...  into keymaterial bytes ----
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = null;
            byte[] hashtarget = new byte[HASHLENGTH + data00.Length];   //fixed length initial hashtarget

            for (int j = 0; j < miter; j++)
            {
                // ----  Now hash consecutively for count times ------
                if (j == 0)
                    result = data00;   	//initialize 
                else
                {
                    Array.Copy(result, hashtarget, result.Length);
                    Array.Copy(data00, 0, hashtarget, result.Length, data00.Length);
                    result = hashtarget;
                    //Console.WriteLine("Updated new initial hash target:") ;
                    //showBytes(result) ;
                }

                for (int i = 0; i < count; i++)
                    result = md5.ComputeHash(result);
                Array.Copy(result, 0, keymaterial, j * HASHLENGTH, result.Length);  //contatenate to keymaterial
            }
            showBytes("Final key material", keymaterial);
            byte[] deskey = new byte[24];
            Array.Copy(keymaterial, deskey, deskey.Length);

            Array.Clear(psbytes, 0, psbytes.Length);
            Array.Clear(data00, 0, data00.Length);
            Array.Clear(result, 0, result.Length);
            Array.Clear(hashtarget, 0, hashtarget.Length);
            Array.Clear(keymaterial, 0, keymaterial.Length);

            return deskey;
        }

        //------   Since we are using an RSA with nonpersisted keycontainer, must pass it in to ensure it isn't colledted  -----
        private static byte[] GetPkcs12(RSA rsa, String keycontainer, String cspprovider, uint KEYSPEC, uint cspflags)
        {
            byte[] pfxblob = null;
            IntPtr hCertCntxt = IntPtr.Zero;

            String DN = "CN=Opensslkey Unsigned Certificate";

            hCertCntxt = CreateUnsignedCertCntxt(keycontainer, cspprovider, KEYSPEC, cspflags, DN);
            if (hCertCntxt == IntPtr.Zero)
            {
                Console.WriteLine("Couldn't create an unsigned-cert\n");
                return null;
            }
            try
            {
                X509Certificate cert = new X509Certificate(hCertCntxt);	//create certificate object from cert context.
                //X509Certificate2UI.DisplayCertificate(new X509Certificate2(cert));	// display it, showing linked private key
                SecureString pswd = GetSecPswd("Set PFX Password ==>");
                pfxblob = cert.Export(X509ContentType.Pkcs12, pswd);
            }

            catch (Exception exc)
            {
                Console.WriteLine("BAD RESULT" + exc.Message);
                pfxblob = null;
            }

            rsa.Clear();
            if (hCertCntxt != IntPtr.Zero)
                CertFreeCertificateContext(hCertCntxt);
            return pfxblob;
        }

        private static IntPtr CreateUnsignedCertCntxt(String keycontainer, String provider, uint KEYSPEC, uint cspflags, String DN)
        {
            const uint AT_KEYEXCHANGE = 0x00000001;
            const uint AT_SIGNATURE = 0x00000002;
            const uint CRYPT_MACHINE_KEYSET = 0x00000020;
            const uint PROV_RSA_FULL = 0x00000001;
            const String MS_DEF_PROV = "Microsoft Base Cryptographic Provider v1.0";
            const String MS_STRONG_PROV = "Microsoft Strong Cryptographic Provider";
            const String MS_ENHANCED_PROV = "Microsoft Enhanced Cryptographic Provider v1.0";
            const uint CERT_CREATE_SELFSIGN_NO_SIGN = 1;
            const uint X509_ASN_ENCODING = 0x00000001;
            const uint CERT_X500_NAME_STR = 3;
            IntPtr hCertCntxt = IntPtr.Zero;
            byte[] encodedName = null;
            uint cbName = 0;

            if (provider != MS_DEF_PROV && provider != MS_STRONG_PROV && provider != MS_ENHANCED_PROV)
                return IntPtr.Zero;
            if (keycontainer == "")
                return IntPtr.Zero;
            if (KEYSPEC != AT_SIGNATURE && KEYSPEC != AT_KEYEXCHANGE)
                return IntPtr.Zero;
            if (cspflags != 0 && cspflags != CRYPT_MACHINE_KEYSET)   //only 0 (Current User) keyset is currently used.
                return IntPtr.Zero;
            if (DN == "")
                return IntPtr.Zero;


            if (CertStrToName(X509_ASN_ENCODING, DN, CERT_X500_NAME_STR, IntPtr.Zero, null, ref cbName, IntPtr.Zero))
            {
                encodedName = new byte[cbName];
                CertStrToName(X509_ASN_ENCODING, DN, CERT_X500_NAME_STR, IntPtr.Zero, encodedName, ref cbName, IntPtr.Zero);
            }

            CERT_NAME_BLOB subjectblob = new CERT_NAME_BLOB();
            subjectblob.pbData = Marshal.AllocHGlobal(encodedName.Length);
            Marshal.Copy(encodedName, 0, subjectblob.pbData, encodedName.Length);
            subjectblob.cbData = encodedName.Length;

            CRYPT_KEY_PROV_INFO pInfo = new CRYPT_KEY_PROV_INFO();
            pInfo.pwszContainerName = keycontainer;
            pInfo.pwszProvName = provider;
            pInfo.dwProvType = PROV_RSA_FULL;
            pInfo.dwFlags = cspflags;
            pInfo.cProvParam = 0;
            pInfo.rgProvParam = IntPtr.Zero;
            pInfo.dwKeySpec = KEYSPEC;

            hCertCntxt = CertCreateSelfSignCertificate(IntPtr.Zero, ref subjectblob, CERT_CREATE_SELFSIGN_NO_SIGN, ref pInfo, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (hCertCntxt == IntPtr.Zero)
                showWin32Error(Marshal.GetLastWin32Error());
            Marshal.FreeHGlobal(subjectblob.pbData);
            return hCertCntxt;
        }

        private static SecureString GetSecPswd(String prompt)
        {
            SecureString password = new SecureString();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(prompt);
            Console.ForegroundColor = ConsoleColor.Magenta;

            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine();
                    return password;
                }
                else if (cki.Key == ConsoleKey.Backspace)
                {
                    // remove the last asterisk from the screen...
                    if (password.Length > 0)
                    {
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        password.RemoveAt(password.Length - 1);
                    }
                }
                else if (cki.Key == ConsoleKey.Escape)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine();
                    return password;
                }
                else if (Char.IsLetterOrDigit(cki.KeyChar) || Char.IsSymbol(cki.KeyChar))
                {
                    if (password.Length < 20)
                    {
                        password.AppendChar(cki.KeyChar);
                        Console.Write("*");
                    }
                    else
                    {
                        Console.Beep();
                    }
                }
                else
                {
                    Console.Beep();
                }
            }
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        private static void showRSAProps(RSACryptoServiceProvider rsa)
        {
            Console.WriteLine("RSA CSP key information:");
            CspKeyContainerInfo keyInfo = rsa.CspKeyContainerInfo;
            Console.WriteLine("Accessible property: " + keyInfo.Accessible);
            Console.WriteLine("Exportable property: " + keyInfo.Exportable);
            Console.WriteLine("HardwareDevice property: " + keyInfo.HardwareDevice);
            Console.WriteLine("KeyContainerName property: " + keyInfo.KeyContainerName);
            Console.WriteLine("KeyNumber property: " + keyInfo.KeyNumber.ToString());
            Console.WriteLine("MachineKeyStore property: " + keyInfo.MachineKeyStore);
            Console.WriteLine("Protected property: " + keyInfo.Protected);
            Console.WriteLine("ProviderName property: " + keyInfo.ProviderName);
            Console.WriteLine("ProviderType property: " + keyInfo.ProviderType);
            Console.WriteLine("RandomlyGenerated property: " + keyInfo.RandomlyGenerated);
            Console.WriteLine("Removable property: " + keyInfo.Removable);
            Console.WriteLine("UniqueKeyContainerName property: " + keyInfo.UniqueKeyContainerName);
        }

        private static void showBytes(String info, byte[] data)
        {
            Console.WriteLine("{0}  [{1} bytes]", info, data.Length);
            for (int i = 1; i <= data.Length; i++)
            {
                Console.Write("{0:X2}  ", data[i - 1]);
                if (i % 16 == 0)
                    Console.WriteLine();
            }
            Console.WriteLine("\n\n");
        }

        private static byte[] GetFileBytes(String filename)
        {
            if (!File.Exists(filename))
                return null;
            Stream stream = new FileStream(filename, FileMode.Open);
            int datalen = (int)stream.Length;
            byte[] filebytes = new byte[datalen];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(filebytes, 0, datalen);
            stream.Close();
            return filebytes;
        }

        private static void PutFileBytes(String outfile, byte[] data, int bytes)
        {
            FileStream fs = null;
            if (bytes > data.Length)
            {
                Console.WriteLine("Too many bytes");
                return;
            }
            try
            {
                fs = new FileStream(outfile, FileMode.Create);
                fs.Write(data, 0, bytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                fs.Close();
            }
        }

        private static void showWin32Error(int errorcode)
        {
            //Win32Exception myEx = new Win32Exception(errorcode);
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine("Error code:\t 0x{0:X}", myEx.ErrorCode);
            //Console.WriteLine("Error message:\t {0}\n", myEx.Message);
            //Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}

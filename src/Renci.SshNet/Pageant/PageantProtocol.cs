#if FEATURE_PAGEANT
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Text;
using Renci.SshNet.Common;

namespace Renci.SshNet.Pageant
{
    /// <summary>
    /// 
    /// </summary>
   public class PageantProtocol: IAgentProtocol
    {

        #region  Constants

        private const int WM_COPYDATA = 0x004A;

        private const int AGENT_COPYDATA_ID = unchecked((int)0x804e50ba);

        private const int AGENT_MAX_MSGLEN = 8192;

        /// <summary>
        /// 
        /// </summary>
        public const byte SSH2_AGENTC_REQUEST_IDENTITIES = 11;

        /// <summary>
        /// 
        /// </summary>
        public const byte SSH2_AGENT_IDENTITIES_ANSWER = 12;

        /// <summary>
        /// 
        /// </summary>
        public const byte SSH2_AGENTC_SIGN_REQUEST = 13;

        /// <summary>
        /// 
        /// </summary>
        public const byte SSH2_AGENT_SIGN_RESPONSE = 14;

        #endregion

        /// <summary>
        /// 
        /// </summary>
       public static bool IsRunning
       {
           get
           {
               var hWnd = NativeMethods.FindWindow("Pageant", "Pageant");

               return hWnd != IntPtr.Zero;
           }
       }


        /// <summary>
        /// 
        /// </summary>
        public PageantProtocol()
        {
            var hWnd = NativeMethods.FindWindow("Pageant", "Pageant");

            if (hWnd == IntPtr.Zero)
            {
                throw new SshException("Pageant not running");
            }
            
        }


        #region Implementation of IAgentProtocol

        IEnumerable<IdentityReference> IAgentProtocol.GetIdentities()
        {
            var hWnd = NativeMethods.FindWindow("Pageant", "Pageant");

            if (hWnd == IntPtr.Zero)
            {
                yield break;
            }

            string mmFileName = Path.GetRandomFileName();

            using (var mmFile = MemoryMappedFile.CreateNew(mmFileName, AGENT_MAX_MSGLEN))
            {
                using (var accessor = mmFile.CreateViewAccessor())
                {
#if NET35 || NET40
                    var security = mmFile.GetAccessControl();
                    security.SetOwner(System.Security.Principal.WindowsIdentity.GetCurrent().User);
                    mmFile.SetAccessControl(security);
#endif

                    accessor.Write(0, IPAddress.NetworkToHostOrder(AGENT_MAX_MSGLEN - 4));
                    accessor.Write(4, SSH2_AGENTC_REQUEST_IDENTITIES);


                    var copy = new COPYDATASTRUCT(AGENT_COPYDATA_ID, mmFileName);

                    if (NativeMethods.SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref copy) == IntPtr.Zero)
                    {
                        yield break;
                    }

                    if (accessor.ReadByte(4) != SSH2_AGENT_IDENTITIES_ANSWER)
                    {
                        yield break;
                    }

                    int numberOfIdentities = IPAddress.HostToNetworkOrder(accessor.ReadInt32(5));


                    if (numberOfIdentities == 0)
                    {
                        yield break;
                    }

                    int position = 9;
                    for (int i = 0; i < numberOfIdentities; i++)
                    {
                        int blobSize = IPAddress.HostToNetworkOrder(accessor.ReadInt32(position));
                        position += 4;

                        var blob = new byte[blobSize];

                        accessor.ReadArray(position, blob, 0, blobSize);
                        position += blobSize;
                        int commnetLenght = IPAddress.HostToNetworkOrder(accessor.ReadInt32(position));
                        position += 4;
                        var commentChars = new byte[commnetLenght];
                        accessor.ReadArray(position, commentChars, 0, commnetLenght);
                        position += commnetLenght;

                        string comment = Encoding.ASCII.GetString(commentChars);
                        string type = Encoding.ASCII.GetString(blob, 4, 7);// needs more testing kind of hack

                        yield return new IdentityReference(type,blob,comment);

                    }
                }

            }
        }

        byte[] IAgentProtocol.SignData(IdentityReference identity, byte[] data)
        {
            var hWnd = NativeMethods.FindWindow("Pageant", "Pageant");

            if (hWnd == IntPtr.Zero)
            {
                return new byte[0];
            }

            string mmFileName = Path.GetRandomFileName();

            using (var mmFile = MemoryMappedFile.CreateNew(mmFileName, AGENT_MAX_MSGLEN))
            {
                using (var accessor = mmFile.CreateViewAccessor())
                {
                    //var security = mmFile.GetAccessControl();
                    //security.SetOwner(System.Security.Principal.WindowsIdentity.GetCurrent().User);
                    //mmFile.SetAccessControl(security);

                    accessor.Write(0, IPAddress.NetworkToHostOrder(AGENT_MAX_MSGLEN - 4));
                    accessor.Write(4, SSH2_AGENTC_SIGN_REQUEST);
                    accessor.Write(5, IPAddress.NetworkToHostOrder(identity.Blob.Length));
                    accessor.WriteArray(9, identity.Blob, 0, identity.Blob.Length);
                    accessor.Write(9 + identity.Blob.Length, IPAddress.NetworkToHostOrder(data.Length));
                    accessor.WriteArray(13 + identity.Blob.Length, data, 0, data.Length);



                    var copy = new COPYDATASTRUCT(AGENT_COPYDATA_ID, mmFileName);

                    if (NativeMethods.SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref copy) == IntPtr.Zero)
                    {
                        return new byte[0];
                    }

                    if (accessor.ReadByte(4) != SSH2_AGENT_SIGN_RESPONSE)
                    {
                        return new byte[0];
                    }

                    int size = IPAddress.HostToNetworkOrder(accessor.ReadInt32(5));
                    var ret = new byte[size];
                    accessor.ReadArray(9, ret, 0, size);
                    return ret;
                }
            }
        }

#endregion
    }
}
#endif
#if NETFRAMEWORK && !NET20 && !NET35
using System;
using System.Runtime.InteropServices;

namespace Renci.SshNet.Pageant {
    [StructLayout (LayoutKind.Sequential)]
    internal struct COPYDATASTRUCT {
        public IntPtr dwData;
        public int cbData;
        public IntPtr lpData;
    }
}
#endif
#if NETFRAMEWORK && !NET20 && !NET35
using System;
using System.Runtime.InteropServices;

namespace Renci.SshNet.Pageant {
    internal class NativeMethods {
        [DllImport ("user32.dll")]
        public static extern IntPtr SendMessage (IntPtr hWnd, uint dwMsg, IntPtr wParam, IntPtr lParam);

        [DllImportAttribute ("user32.dll", EntryPoint = "FindWindowA", CallingConvention = CallingConvention.Winapi,
            ExactSpelling = true)]
        public static extern IntPtr FindWindow ([MarshalAsAttribute (UnmanagedType.LPStr)] string lpClassName, [MarshalAsAttribute (UnmanagedType.LPStr)] string lpWindowName);
    }
}
#endif
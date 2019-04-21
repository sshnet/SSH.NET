using System;
using System.Runtime.InteropServices;

namespace Renci.SshNet.Pageant
{
    internal class NativeMethods
    {
        [DllImport("user32.dll", EntryPoint = "SendMessageA", CallingConvention = CallingConvention.StdCall,
            ExactSpelling = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int dwMsg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [DllImportAttribute("user32.dll", EntryPoint = "FindWindowA", CallingConvention = CallingConvention.Winapi,
            ExactSpelling = true)]
        public static extern IntPtr FindWindow([MarshalAsAttribute(UnmanagedType.LPStr)] string lpClassName,
                                               [MarshalAsAttribute(UnmanagedType.LPStr)] string lpWindowName);
    }
}
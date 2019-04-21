using System.Runtime.InteropServices;

namespace Renci.SshNet.Pageant
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct COPYDATASTRUCT
    {
        public COPYDATASTRUCT(int dwData, string lpData)
        {
            this.dwData = dwData;
            this.lpData = lpData;
            cbData = lpData.Length + 1;
        }


        private readonly int dwData;

        private readonly int cbData;

        [MarshalAs(UnmanagedType.LPStr)] private readonly string lpData;
    }
}
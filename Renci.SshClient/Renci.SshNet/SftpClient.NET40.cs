using System;
using System.Threading.Tasks;

namespace Renci.SshNet
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SftpClient
    {
        partial void ExecuteThread(Action action)
        {
            Task.Factory.StartNew(action);
        }
    }
}
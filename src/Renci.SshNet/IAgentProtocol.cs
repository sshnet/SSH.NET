using System.Collections.Generic;

namespace Renci.SshNet
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAgentProtocol
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerable<IdentityReference> GetIdentities();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] SignData(IdentityReference identity, byte[] data);
    }
}

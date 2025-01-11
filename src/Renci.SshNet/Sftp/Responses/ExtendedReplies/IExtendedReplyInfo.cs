using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp.Responses
{
    /// <summary>
    /// Extended Reply Info.
    /// </summary>
    internal interface IExtendedReplyInfo
    {
        /// <summary>
        /// Loads the data from the stream into the instance.
        /// </summary>
        /// <param name="stream">The stream.</param>
        void LoadData(SshDataStream stream);
    }
}

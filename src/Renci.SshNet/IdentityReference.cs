namespace Renci.SshNet
{
    /// <summary>
    /// 
    /// </summary>
    public class IdentityReference
    {
        /// <summary>
        /// 
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public byte[] Blob { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Comment { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="blob"></param>
        /// <param name="comment"></param>
        public IdentityReference(string type,byte[] blob,string comment )
        {
           this.Type = type;
           this.Blob = blob;
           this.Comment = comment;
        }

    }
}

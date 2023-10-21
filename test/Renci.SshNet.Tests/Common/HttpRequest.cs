using System.Collections.Generic;

namespace Renci.SshNet.Tests.Common
{
    public class HttpRequest
    {
        public HttpRequest()
        {
            Headers = new List<string>();
            MessageBody = System.Array.Empty<byte>();
        }

        public string RequestLine { get; set; }
        public IList<string> Headers { get; set; }
        public byte[] MessageBody { get; set; }
    }
}

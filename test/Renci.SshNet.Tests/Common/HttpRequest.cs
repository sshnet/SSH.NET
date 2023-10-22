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
        public List<string> Headers { get; }
        public byte[] MessageBody { get; set; }
    }
}

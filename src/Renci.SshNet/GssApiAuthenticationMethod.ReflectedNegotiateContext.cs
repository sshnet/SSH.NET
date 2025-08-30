#if !NET
#nullable enable
using System;
using System.Net;

namespace Renci.SshNet
{
    public partial class GssApiAuthenticationMethod
    {
        private sealed class ReflectedNegotiateContext : IAuthenticationContext
        {

#pragma warning disable IDE0060 // Remove unused parameter
            public ReflectedNegotiateContext(bool delegateCredential, NetworkCredential credential, string targetName)
#pragma warning restore IDE0060 // Remove unused parameter
            {
            }

            public bool IsSigned
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public byte[] ComputeIntegrityCheck(ReadOnlySpan<byte> message)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
            }

            public byte[] GetOutgoingBlob(ReadOnlySpan<byte> incomingBlob, out NegotiateStatusCode statusCode)
            {
                throw new NotImplementedException();
            }
        }
    }
}
#endif

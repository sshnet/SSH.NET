#if NET
using System;
using System.Buffers;
using System.Net;
using System.Net.Security;
#if NET8_0
using System.Runtime.CompilerServices;
#endif
using System.Security.Principal;

namespace Renci.SshNet
{
    public partial class GssApiAuthenticationMethod
    {
        private sealed class NegotiateContext : IAuthenticationContext
        {
#if NET8_0
            // This API was made public in .NET 9 through ComputeIntegrityCheck.
            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetMIC")]
            private static extern void GetMICMethod(NegotiateAuthentication context, ReadOnlySpan<byte> data, IBufferWriter<byte> writer);
#endif
            private readonly NegotiateAuthentication _negotiateAuthentication;
            public NegotiateContext(bool delegateCredential, NetworkCredential credential, string targetName)
            {
                var negotiateOptions = new NegotiateAuthenticationClientOptions()
                {
                    AllowedImpersonationLevel = delegateCredential ? TokenImpersonationLevel.Delegation : TokenImpersonationLevel.Impersonation,
                    Credential = credential,
                    Package = "Kerberos",

#if NET10_0_OR_GREATER
                    RequiredProtectionLevel = ProtectionLevel.Sign,
#else
                    // While only Sign is needed we need to set EncryptAndSign for
                    // Windows client support. Sign only will pass in SECQOP_WRAP_NO_ENCRYPT
                    // to MakeSignature which fails.
                    // https://github.com/dotnet/runtime/issues/103461
                    RequiredProtectionLevel = ProtectionLevel.EncryptAndSign,
#endif

                    // While RFC states this should be set to "false", Win32-OpenSSH
                    // fails if it's not true. I'm unsure if openssh-portable on Linux
                    // will fail in the same way or not.
                    RequireMutualAuthentication = true,
                    TargetName = targetName
                };

                _negotiateAuthentication = new NegotiateAuthentication(negotiateOptions);
            }

            public bool IsSigned
            {
                get
                {
                    return _negotiateAuthentication.IsSigned;
                }
            }

            public byte[] ComputeIntegrityCheck(ReadOnlySpan<byte> message)
            {
                var signatureWriter = new ArrayBufferWriter<byte>();
#if NET8_0
                GetMICMethod(
                    _negotiateAuthentication,
                    message,
                    signatureWriter);
#else
                _negotiateAuthentication.ComputeIntegrityCheck(
                    message,
                    signatureWriter);
#endif

                return signatureWriter.WrittenSpan.ToArray();
            }

            public void Dispose()
            {
                _negotiateAuthentication.Dispose();
            }

            public byte[] GetOutgoingBlob(ReadOnlySpan<byte> incomingBlob, out NegotiateStatusCode statusCode)
            {
                var outgoingBlob = _negotiateAuthentication.GetOutgoingBlob(incomingBlob, out var code);

#pragma warning disable IDE0010 // Add missing cases
                switch (code)
                {
                    case NegotiateAuthenticationStatusCode.ContinueNeeded:
                        statusCode = NegotiateStatusCode.ContinueNeeded;
                        break;
                    case NegotiateAuthenticationStatusCode.Completed:
                        statusCode = NegotiateStatusCode.Completed;
                        break;
                    default:
                        statusCode = NegotiateStatusCode.Other;
                        break;
                }
#pragma warning restore IDE0010 // Add missing cases

                return outgoingBlob;
            }
        }
    }
}
#endif

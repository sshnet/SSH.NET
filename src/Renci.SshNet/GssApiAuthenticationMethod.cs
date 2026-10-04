using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to perform GSS API authentication.
    /// </summary>
    public partial class GssApiAuthenticationMethod : AuthenticationMethod
    {
        // Kerberos - 1.2.840.113554.1.2.2 - This is DER encoding of the OID.
        private static readonly byte[] KRB5OID = [0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x12, 0x01, 0x02, 0x02];

        private readonly KerberosCredential _credential;
        private readonly RequestMessageGssApi _requestMessage;
        private readonly EventWaitHandle _mechanismSelectionCompleted = new AutoResetEvent(initialState: false);
        private readonly EventWaitHandle _tokenExchangeCompleted = new AutoResetEvent(initialState: false);
        private readonly EventWaitHandle _authenticationCompleted = new AutoResetEvent(initialState: false);

        private AuthenticationResult? _authenticationResult;
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        private IAuthenticationContext _authenticationContext;
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
        private Session _session;
        private bool _isDisposed;

        /// <summary>
        /// Gets the name of the authentication method.
        /// </summary>
        public override string Name
        {
            get { return _requestMessage.MethodName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GssApiAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="credential">The <see cref="KerberosCredential"/>.</param>
        public GssApiAuthenticationMethod(string username, KerberosCredential credential)
            : base(username)
        {
            ThrowHelper.ThrowIfNull(credential);

            _credential = credential;
            _requestMessage = new RequestMessageGssApi(ServiceName.Connection, Username, KRB5OID);
        }

        /// <inheritdoc />
        public override AuthenticationResult Authenticate(Session session)
        {
            ThrowHelper.ThrowIfNull(session);
            _session = session;

            session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;
            session.UserAuthenticationGssApiResponseReceived += Session_UserAuthenticationGssApiResponseReceived;
            session.RegisterMessage("SSH_MSG_USERAUTH_GSSAPI_RESPONSE");

            try
            {
                session.SendMessage(_requestMessage);
                session.WaitOnHandle(_mechanismSelectionCompleted);
            }
            finally
            {
                session.UnRegisterMessage("SSH_MSG_USERAUTH_GSSAPI_RESPONSE");
                session.UserAuthenticationGssApiResponseReceived -= Session_UserAuthenticationGssApiResponseReceived;
                session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
            }

            if (_authenticationResult.HasValue)
            {
                return _authenticationResult.Value;
            }

            // RFC uses hostbased SPN format "service@host" but Windows SSPI needs the service/host format.
            // .NET converts this format to the hostbased format expected by GSSAPI for us.
            var targetName = !string.IsNullOrEmpty(_credential.TargetName) ? _credential.TargetName : $"host/{_session.ConnectionInfo.Host}";
            var networkCredential = _credential.NetworkCredential ?? CredentialCache.DefaultNetworkCredentials;
#if NET
            _authenticationContext = new NegotiateContext(_credential.DelegateCredential, networkCredential, targetName);
#else
            _authenticationContext = new ReflectedNegotiateContext(_credential.DelegateCredential, networkCredential, targetName);
#endif
            var outgoingBlob = _authenticationContext.GetOutgoingBlob(Array.Empty<byte>(), out var statusCode);

            if (outgoingBlob == null)
            {
                return AuthenticationResult.Failure;
            }

            var tokenMessage = new GssApiTokenMessage { Token = outgoingBlob };

            session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;
            session.UserAuthenticationGssApiTokenReceived += Session_UserAuthenticationGssApiTokenReceived;
            session.RegisterMessage("SSH_MSG_USERAUTH_GSSAPI_TOKEN");

            try
            {
                session.SendMessage(tokenMessage);
                session.WaitOnHandle(_tokenExchangeCompleted);
            }
            finally
            {
                session.UnRegisterMessage("SSH_MSG_USERAUTH_GSSAPI_TOKEN");
                session.UserAuthenticationGssApiTokenReceived -= Session_UserAuthenticationGssApiTokenReceived;
                session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
            }

            if (_authenticationResult.HasValue)
            {
                return _authenticationResult.Value;
            }

            // While we request signing, the server may not so we need to check to see if we need to send a MIC.
            Message finalExchangeMessage;
            if (_authenticationContext.IsSigned)
            {
                var micDataStream = new SshDataStream(256);
                micDataStream.WriteBinary(_session.SessionId);
                micDataStream.WriteByte(RequestMessage.AuthenticationMessageCode);
                micDataStream.Write(session.ConnectionInfo.Username, Encoding.UTF8);
                micDataStream.Write("ssh-connection", Encoding.UTF8);
                micDataStream.Write("gssapi-with-mic", Encoding.UTF8);

                var mic = _authenticationContext.ComputeIntegrityCheck(micDataStream.ToArray());

                finalExchangeMessage = new GssApiMicMessage(mic);
            }
            else
            {
                finalExchangeMessage = new GssApiExchangeCompleteMessage();
            }

            session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;
            session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;

            try
            {
                session.SendMessage(finalExchangeMessage);
                session.WaitOnHandle(_authenticationCompleted);
            }
            finally
            {
                session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
                session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
            }

            return _authenticationResult.Value;
        }

        private void Session_UserAuthenticationGssApiResponseReceived(object sender, MessageEventArgs<GssApiResponseMessage> e)
        {
            if (!KRB5OID.SequenceEqual(e.Message.SelectedMechanismOid))
            {
                throw new SshConnectionException("The packet contains an unexpected value.", DisconnectReason.ProtocolError);
            }

            _ = _mechanismSelectionCompleted.Set();
        }

        private void Session_UserAuthenticationGssApiTokenReceived(object sender, MessageEventArgs<GssApiTokenMessage> e)
        {
            var incomingBlob = e.Message.Token;
            var outgoingBlob = _authenticationContext.GetOutgoingBlob(incomingBlob, out var statusCode);

            if (statusCode == NegotiateStatusCode.ContinueNeeded)
            {
                var tokenMessage = new GssApiTokenMessage { Token = outgoingBlob };
                _session.SendMessage(tokenMessage);
            }
            else if (statusCode == NegotiateStatusCode.Completed)
            {
                _ = _tokenExchangeCompleted.Set();
            }
            else
            {
                throw new SshException($"Failed to generate the token. Status code: {statusCode}");
            }
        }

        private void Session_UserAuthenticationSuccessReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            _authenticationResult = AuthenticationResult.Success;
            _ = _authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            if (e.Message.PartialSuccess)
            {
                _authenticationResult = AuthenticationResult.PartialSuccess;
            }
            else
            {
                _authenticationResult = AuthenticationResult.Failure;
            }

            // Copy allowed authentication methods
            AllowedAuthentications = e.Message.AllowedAuthentications;

            _ = _mechanismSelectionCompleted.Set();
            _ = _tokenExchangeCompleted.Set();
            _ = _authenticationCompleted.Set();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _authenticationContext?.Dispose();
                _mechanismSelectionCompleted.Dispose();
                _tokenExchangeCompleted.Dispose();
                _authenticationCompleted.Dispose();

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Represents a stateful authentication context.
        /// </summary>
        private interface IAuthenticationContext : IDisposable
        {
            /// <summary>
            /// Gets a value indicating whether data signing was negotiated.
            /// </summary>
            bool IsSigned { get; }

            /// <summary>
            /// Evaluates an authentication token sent by the other party and returns a token in response.
            /// </summary>
            /// <param name="incomingBlob">Incoming authentication token, or empty value when initiating the authentication exchange.</param>
            /// <param name="statusCode">Status code returned by the authentication provider.</param>
            /// <returns>An outgoing authentication token to be sent to the other party.</returns>
            byte[] GetOutgoingBlob(ReadOnlySpan<byte> incomingBlob, out NegotiateStatusCode statusCode);

            /// <summary>
            /// Computes the integrity check of a given message.
            /// </summary>
            /// <param name="message">Input message for MIC calculation.</param>
            /// <returns>The MIC.</returns>
            byte[] ComputeIntegrityCheck(ReadOnlySpan<byte> message);
        }

        private enum NegotiateStatusCode
        {
            ContinueNeeded,
            Completed,
            Other
        }
    }
}

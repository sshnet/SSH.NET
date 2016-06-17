namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Provides list of disconnect reason as specified by the protocol.
    /// </summary>
    public enum DisconnectReason
    {
        /// <summary>
        /// Disconnect reason is not provided.
        /// </summary>
        None = 0,
        /// <summary>
        /// SSH_DISCONNECT_HOST_NOT_ALLOWED_TO_CONNECT
        /// </summary>
        HostNotAllowedToConnect = 1,
        /// <summary>
        /// SSH_DISCONNECT_PROTOCOL_ERROR
        /// </summary>
        ProtocolError = 2,
        /// <summary>
        /// SSH_DISCONNECT_KEY_EXCHANGE_FAILED
        /// </summary>
        KeyExchangeFailed = 3,
        /// <summary>
        /// SSH_DISCONNECT_RESERVED
        /// </summary>
        Reserved = 4,
        /// <summary>
        /// SSH_DISCONNECT_MAC_ERROR
        /// </summary>
        MacError = 5,
        /// <summary>
        /// SSH_DISCONNECT_COMPRESSION_ERROR
        /// </summary>
        CompressionError = 6,
        /// <summary>
        /// SSH_DISCONNECT_SERVICE_NOT_AVAILABLE
        /// </summary>
        ServiceNotAvailable = 7,
        /// <summary>
        /// SSH_DISCONNECT_PROTOCOL_VERSION_NOT_SUPPORTED
        /// </summary>
        ProtocolVersionNotSupported = 8,
        /// <summary>
        /// SSH_DISCONNECT_HOST_KEY_NOT_VERIFIABLE
        /// </summary>
        HostKeyNotVerifiable = 9,
        /// <summary>
        /// SSH_DISCONNECT_CONNECTION_LOST
        /// </summary>
        ConnectionLost = 10,
        /// <summary>
        /// SSH_DISCONNECT_BY_APPLICATION
        /// </summary>
        ByApplication = 11,
        /// <summary>
        /// SSH_DISCONNECT_TOO_MANY_CONNECTIONS
        /// </summary>
        TooManyConnections = 12,
        /// <summary>
        /// SSH_DISCONNECT_AUTH_CANCELLED_BY_USER
        /// </summary>
        AuthenticationCanceledByUser = 13,
        /// <summary>
        /// SSH_DISCONNECT_NO_MORE_AUTH_METHODS_AVAILABLE
        /// </summary>
        NoMoreAuthenticationMethodsAvailable = 14,
        /// <summary>
        /// SSH_DISCONNECT_ILLEGAL_USER_NAME
        /// </summary>
        IllegalUserName = 15,
    }
}

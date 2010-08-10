namespace Renci.SshClient.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public enum MessageTypes : byte
    {
        /// <summary>
        /// {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        None = 0,
        /// <summary>
        /// SSH_MSG_DISCONNECT
        /// </summary>
        Disconnect = 1,
        /// <summary>
        /// SSH_MSG_IGNORE
        /// </summary>
        Ignore = 2,
        /// <summary>
        /// SSH_MSG_UNIMPLEMENTED
        /// </summary>
        Unimplemented = 3,
        /// <summary>
        /// SSH_MSG_DEBUG
        /// </summary>
        Debug = 4,
        /// <summary>
        /// SSH_MSG_SERVICE_REQUEST
        /// </summary>
        ServiceRequest = 5,
        /// <summary>
        /// SSH_MSG_SERVICE_ACCEPT
        /// </summary>
        ServiceAcceptRequest = 6,

        /// <summary>
        /// SSH_MSG_KEXINIT
        /// </summary>
        KeyExchangeInit = 20,
        /// <summary>
        /// SSH_MSG_NEWKEYS
        /// </summary>
        NewKeys = 21,
        /// <summary>
        /// SSH_MSG_KEXDH_INIT
        /// </summary>
        DiffieHellmanKeyExchangeInit = 30,
        /// <summary>
        /// SSH_MSG_KEXDH_REPLY
        /// </summary>
        KeyExchangeDhReply = 31,

        SSH_MSG_KEX_DH_GEX_GROUP = 31,
        SSH_MSG_KEX_DH_GEX_INIT = 32,
        SSH_MSG_KEX_DH_GEX_REPLY = 33,
        SSH_MSG_KEX_DH_GEX_REQUEST = 34,

        /// <summary>
        /// SSH_MSG_USERAUTH_REQUEST
        /// </summary>
        UserAuthenticationRequest = 50,
        /// <summary>
        /// SSH_MSG_USERAUTH_FAILURE
        /// </summary>
        UserAuthenticationFailure = 51,
        /// <summary>
        /// SSH_MSG_USERAUTH_SUCCESS
        /// </summary>
        UserAuthenticationSuccess = 52,
        /// <summary>
        /// SSH_MSG_USERAUTH_BANNER
        /// </summary>
        UserAuthenticationBanner = 53,
        /// <summary>
        /// SSH_MSG_USERAUTH_INFO_REQUEST
        /// </summary>
        UserAuthenticationInformationRequest = 60,
        /// <summary>
        /// SSH_MSG_USERAUTH_INFO_RESPONSE
        /// </summary>
        UserAuthenticationInformationResponse = 61,
        /// <summary>
        /// SSH_MSG_USERAUTH_PK_OK
        /// </summary>
        UserAuthenticationPublicKey = 60,
        /// <summary>
        /// SSH_MSG_USERAUTH_PASSWD_CHANGEREQ
        /// </summary>
        UserAuthenticationPasswordChangeRequired = 60,


        /// <summary>
        /// SSH_MSG_GLOBAL_REQUEST
        /// </summary>
        GlobalRequest = 80,
        /// <summary>
        /// SSH_MSG_REQUEST_SUCCESS
        /// </summary>
        RequestSuccess = 81,
        /// <summary>
        /// SSH_MSG_REQUEST_FAILURE
        /// </summary>
        RequestFailure = 82,
        /// <summary>
        /// SSH_MSG_CHANNEL_OPEN
        /// </summary>
        ChannelOpen = 90,
        /// <summary>
        /// SSH_MSG_CHANNEL_OPEN_CONFIRMATION
        /// </summary>
        ChannelOpenConfirmation = 91,
        /// <summary>
        /// SSH_MSG_CHANNEL_OPEN_FAILURE
        /// </summary>
        ChannelOpenFailure = 92,
        /// <summary>
        /// SSH_MSG_CHANNEL_WINDOW_ADJUST
        /// </summary>
        ChannelWindowAdjust = 93,
        /// <summary>
        /// SSH_MSG_CHANNEL_DATA
        /// </summary>
        ChannelData = 94,
        /// <summary>
        /// SSH_MSG_CHANNEL_EXTENDED_DATA
        /// </summary>
        ChannelExtendedData = 95,
        /// <summary>
        /// SSH_MSG_CHANNEL_EOF
        /// </summary>
        ChannelEof = 96,
        /// <summary>
        /// SSH_MSG_CHANNEL_CLOSE
        /// </summary>
        ChannelClose = 97,
        /// <summary>
        /// SSH_MSG_CHANNEL_REQUEST
        /// </summary>
        ChannelRequest = 98,
        /// <summary>
        /// SSH_MSG_CHANNEL_SUCCESS
        /// </summary>
        ChannelSuccess = 99,
        /// <summary>
        /// SSH_MSG_CHANNEL_FAILURE
        /// </summary>
        ChannelFailure = 100,
    }
}

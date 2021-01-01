/// <summary>
/// Trace event identifiers enumeration
/// </summary>
public enum TraceEventId
{
    /// <summary>
    /// Failure closing handle event identifier
    /// </summary>
    FailureClosingHandle = 0,

    /// <summary>
    /// Disconnecting client event identifier
    /// </summary>
    DisconnectingClient = 1,

    /// <summary>
    /// Disposing client event identifier
    /// </summary>
    DisposingClient = 2,

    /// <summary>
    /// Unsuccessful channel close wait event identifier
    /// </summary>
    UnsuccessfulChannelCloseWait = 3,

    /// <summary>
    /// Socket shutdown failure event identifier
    /// </summary>
    SocketShutdownFailure = 4,

    /// <summary>
    /// Pending channels close timeout event identifier
    /// </summary>
    PendingChannelsCloseTimeout = 5,

    /// <summary>
    /// Excess data event identifier
    /// </summary>
    ExcessData = 6,

    /// <summary>
    /// Server cipher creation event identifier
    /// </summary>
    ServerCipherCreation = 7,

    /// <summary>
    /// File size fetch failure event identifier
    /// </summary>
    FileSizeFetchFailure = 8,

    /// <summary>
    /// Server version event identifier
    /// </summary>
    ServerVersion = 9,

    /// <summary>
    /// Disconnecting session event identifier
    /// </summary>
    DisconnectingSession = 10,

    /// <summary>
    /// Sending message event identifier
    /// </summary>
    SendingMessage = 11,

    /// <summary>
    /// Failure sending message event identifier
    /// </summary>
    FailureSendingMessage = 12,

    /// <summary>
    /// Disconnect received event identifier
    /// </summary>
    DisconnectReceived = 13,

    /// <summary>
    /// Message received event identifier
    /// </summary>
    MessageReceived = 14,

    /// <summary>
    /// Initiating connection event identifier
    /// </summary>
    InitiatingConnection = 15,

    /// <summary>
    /// Shutting down socket event identifier
    /// </summary>
    ShuttingDownSocket = 16,

    /// <summary>
    /// Disposing socket event identifier
    /// </summary>
    DisposingSocket = 17,

    /// <summary>
    /// Disposed socket event identifier
    /// </summary>
    DisposedSocket = 18,

    /// <summary>
    /// Raised exception event identifier
    /// </summary>
    RaisedException = 19,

    /// <summary>
    /// Disconnecting after exception event identifier
    /// </summary>
    DisconnectingAfterException = 20,

    /// <summary>
    /// Disposing session event identifier
    /// </summary>
    DisposingSession = 21,
}
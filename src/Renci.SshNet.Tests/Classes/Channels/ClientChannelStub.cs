using System;
using System.Collections.Generic;
using Renci.SshNet.Channels;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    internal class ClientChannelStub : ClientChannel
    {
        /// <summary>
        /// Initializes a new <see cref="ClientChannelStub"/> instance.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="localWindowSize">Size of the window.</param>
        /// <param name="localPacketSize">Size of the packet.</param>
        public ClientChannelStub(ISession session, uint localChannelNumber, uint localWindowSize, uint localPacketSize)
            : base(session, localChannelNumber, localWindowSize, localPacketSize)
        {
            OnErrorOccurredInvocations = new List<Exception>();
        }

        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.X11; }
        }

        public IList<Exception> OnErrorOccurredInvocations { get; private set; }

        public Exception OnCloseException { get; set; }

        public Exception OnDataException { get; set; }

        public Exception OnDisconnectedException { get; set; }

        public Exception OnEofException{ get; set; }

        public Exception OnErrorOccurredException { get; set; }

        public Exception OnExtendedDataException { get; set; }

        public Exception OnFailureException { get; set; }

        public Exception OnRequestException { get; set; }

        public Exception OnSuccessException { get; set; }

        public Exception OnWindowAdjustException { get; set; }

        public Exception OnOpenConfirmationException { get; set; }

        public Exception OnOpenFailureException { get; set; }

        public void SetIsOpen(bool value)
        {
            IsOpen = value;
        }

        public void InitializeRemoteChannelInfo(uint remoteChannelNumber, uint remoteWindowSize, uint remotePacketSize)
        {
            base.InitializeRemoteInfo(remoteChannelNumber, remoteWindowSize, remotePacketSize);
        }

        protected override void OnClose()
        {
            base.OnClose();

            if (OnCloseException != null)
                throw OnCloseException;
        }

        protected override void OnData(byte[] data)
        {
            base.OnData(data);

            if (OnDataException != null)
                throw OnDataException;
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();

            if (OnDisconnectedException != null)
                throw OnDisconnectedException;
        }

        protected override void OnEof()
        {
            base.OnEof();

            if (OnEofException != null)
                throw OnEofException;
        }

        protected override void OnExtendedData(byte[] data, uint dataTypeCode)
        {
            base.OnExtendedData(data, dataTypeCode);

            if (OnExtendedDataException != null)
                throw OnExtendedDataException;
        }

        protected override void OnErrorOccured(Exception exp)
        {
            OnErrorOccurredInvocations.Add(exp);

            if (OnErrorOccurredException != null)
                throw OnErrorOccurredException;
        }

        protected override void OnFailure()
        {
            base.OnFailure();

            if (OnFailureException != null)
                throw OnFailureException;
        }

        protected override void OnRequest(RequestInfo info)
        {
            base.OnRequest(info);

            if (OnRequestException != null)
                throw OnRequestException;
        }

        protected override void OnSuccess()
        {
            base.OnSuccess();

            if (OnSuccessException != null)
                throw OnSuccessException;
        }

        protected override void OnWindowAdjust(uint bytesToAdd)
        {
            base.OnWindowAdjust(bytesToAdd);

            if (OnWindowAdjustException != null)
                throw OnWindowAdjustException;
        }

        protected override void OnOpenConfirmation(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
        {
            base.OnOpenConfirmation(remoteChannelNumber, initialWindowSize, maximumPacketSize);

            if (OnOpenConfirmationException != null)
                throw OnOpenConfirmationException;
        }

        protected override void OnOpenFailure(uint reasonCode, string description, string language)
        {
            base.OnOpenFailure(reasonCode, description, language);

            if (OnOpenFailureException != null)
                throw OnOpenFailureException;
        }
    }
}

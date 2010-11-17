using System.IO;
using System.Linq;
using System.Threading;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal class ChannelSessionShell : ChannelSession
    {

        private EventWaitHandle _success = new AutoResetEvent(false);

        /// <summary>
        /// Holds channel data stream
        /// </summary>
        private Stream _channelData;

        /// <summary>
        /// Holds channel extended data stream
        /// </summary>
        private Stream _channelExtendedData;

        public void Start(Stream output, Stream extendedOutput)
        {
            this.Open();

            this._channelData = output;
            this._channelExtendedData = extendedOutput;

            this.SendPseudoTerminalRequest("xterm", 80, 24, 640, 240, "");

            _success.WaitOne();

            this.SendShellRequest();
        }

        public void Stop()
        {
            //  Close channel
            this.Close();
        }

        protected override void OnSuccess()
        {
            base.OnSuccess();

            _success.Set();
        }

        protected override void OnFailure()
        {
            base.OnFailure();
        }

        protected override void OnData(string data)
        {
            base.OnData(data);

            this._channelData.Write(data.GetSshBytes().ToArray(), 0, data.Length);
            this._channelData.Flush();
        }

        protected override void OnExtendedData(string data, uint dataTypeCode)
        {
            base.OnExtendedData(data, dataTypeCode);

            //  TODO:   dataTypeCode is not handled
            this._channelExtendedData.Write(data.GetSshBytes().ToArray(), 0, data.Length);
            this._channelExtendedData.Flush();
        }

        public void Send(string data)
        {
            this.SendMessage(new ChannelDataMessage
            {
                LocalChannelNumber = this.RemoteChannelNumber,
                Data = data,
            });
        }

        public class ShellStream : MemoryStream
        {
            public ShellStream()
            {

            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                base.Write(buffer, offset, count);
            }

            public override void WriteByte(byte value)
            {
                base.WriteByte(value);
            }
        }
    }
}

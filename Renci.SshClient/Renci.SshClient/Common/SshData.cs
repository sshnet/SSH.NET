using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Renci.SshClient.Common
{
    public abstract class SshData
    {
        /// <summary>
        /// Data byte array that hold message unencrypted data
        /// </summary>
        private IList<byte> _data;

        private int _readerIndex;

        public bool IsEndOfData
        {
            get
            {
                return this._readerIndex >= this._data.Count();
            }
        }

        public virtual IEnumerable<byte> GetBytes()
        {
            this._data = new List<byte>();

            this.SaveData();

            return this._data;
        }

        protected abstract void LoadData();

        protected abstract void SaveData();

        protected void LoadBytes(IEnumerable<byte> bytes)
        {
            this.ResetReader();
            this._data = new List<byte>(bytes);
        }

        protected void ResetReader()
        {
            this._readerIndex = 1;  //  Set to 1 to skip first byte which specifies message type
        }

        protected IEnumerable<byte> ReadBytes(int length)
        {
            var result = this._data.Skip(this._readerIndex).Take(length);
            this._readerIndex += length;
            return result;
        }

        protected byte ReadByte()
        {
            return this.ReadBytes(1).FirstOrDefault();
        }

        protected bool ReadBoolean()
        {
            return this.ReadByte() == 0 ? false : true;
        }

        protected UInt16 ReadUInt16()
        {
            return BitConverter.ToUInt16(this.ReadBytes(2).Reverse().ToArray(), 0);
        }

        protected UInt32 ReadUInt32()
        {
            return BitConverter.ToUInt32(this.ReadBytes(4).Reverse().ToArray(), 0);
        }

        protected UInt64 ReadUInt64()
        {
            return BitConverter.ToUInt64(this.ReadBytes(8).Reverse().ToArray(), 0);
        }

        protected Int64 ReadInt64()
        {
            return BitConverter.ToInt64(this.ReadBytes(8).Reverse().ToArray(), 0);

        }

        protected string ReadString()
        {
            var length = this.ReadUInt32();

            if (length > (UInt32)int.MaxValue)
            {
                throw new NotSupportedException(string.Format("String that longer that {0} are not supported.", int.MaxValue));
            }

            var result = this._data.Skip(this._readerIndex).Take((int)length).GetSshString();
            this._readerIndex += (int)length;

            return result;
        }

        protected BigInteger ReadBigInteger()
        {
            var length = this.ReadUInt32();

            var data = this.ReadBytes((int)length);

            return new BigInteger(data.Reverse().ToArray());
        }

        protected IEnumerable<string> ReadNamesList()
        {
            var namesList = this.ReadString();
            return namesList.Split(',');
        }

        protected IDictionary<string, string> ReadExtensionPair()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            while (this._readerIndex < this._data.Count)
            {
                var extensionName = this.ReadString();
                var extensionData = this.ReadString();
                result.Add(extensionName, extensionData);
            }
            return result;
        }

        protected void Write(IEnumerable<byte> data)
        {
            foreach (var b in data)
                this.Write(b);
        }

        protected void Write(byte data)
        {
            this._data.Add(data);
        }

        protected void Write(bool data)
        {
            if (data)
            {
                this.Write(1);
            }
            else
            {
                this.Write(0);
            }
        }

        protected void Write(UInt16 data)
        {
            this.Write(BitConverter.GetBytes(data).Reverse());
        }

        protected void Write(UInt32 data)
        {
            this.Write(BitConverter.GetBytes(data).Reverse());
        }

        protected void Write(UInt64 data)
        {
            this.Write(BitConverter.GetBytes(data).Reverse());
        }

        protected void Write(Int64 data)
        {
            this.Write(BitConverter.GetBytes(data).Reverse());
        }

        protected void Write(string data, Encoding encoding)
        {
            this.Write((uint)data.Length);
            this.Write(encoding.GetBytes(data));
        }

        protected void Write(string data)
        {
            this.Write((uint)data.Length);
            this.Write(data.GetSshBytes());
        }

        protected void Write(BigInteger data)
        {
            var bytes = data.ToByteArray().Reverse().ToList();
            this.Write((uint)bytes.Count);
            this.Write(bytes);
        }

        protected void Write(IEnumerable<string> data)
        {
            this.Write(string.Join(",", data));
        }

        protected void Write(IDictionary<string, string> data)
        {
            foreach (var item in data)
            {
                this.Write(item.Key);
                this.Write(item.Value);
            }
        }
    }
}

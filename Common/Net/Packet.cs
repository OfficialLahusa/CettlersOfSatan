namespace Common.Net
{
    public class Packet
    {
        private List<byte> data;
        public int ReadPos
        {
            get;
            set;
        }

        public Packet()
        {
            data = new List<byte>();
            ReadPos = 0;
        }

        public Packet(byte[] data)
        {
            this.data = new List<byte>(data);
            ReadPos = 0;
        }

        public void Clear()
        {
            data.Clear();
            ReadPos = 0;
        }

        public byte[] GetData()
        {
            return data.ToArray();
        }

        public int GetSize()
        {
            return data.Count;
        }

        public bool IsEmpty()
        {
            return data.Count == 0;
        }

        public Packet ResetReadPos()
        {
            ReadPos = 0;
            return this;
        }

        private bool CheckSize(int size)
        {
            return data.Count - ReadPos >= size;
        }

        public Packet Write(byte[] data)
        {
            this.data.AddRange(data);

            return this;
        }

        public Packet Read(ref byte[] data)
        {
            if (CheckSize(data.Length))
            {
                this.data.CopyTo(ReadPos, data, 0, data.Length);
                ReadPos += data.Length;
            }

            return this;
        }

        public Packet Write(Guid data)
        {
            Write(data.ToByteArray());

            return this;
        }

        public Packet Read(ref Guid data)
        {
            if (CheckSize(16))
            {
                byte[] bytes = new byte[16];
                this.data.CopyTo(ReadPos, bytes, 0, bytes.Length);
                data = new Guid(bytes);

                ReadPos += 16;
            }

            return this;
        }

        public Packet Write(Int16 data)
        {
            Write(BitConverter.GetBytes(data));

            return this;
        }

        public Packet Read(ref Int16 data)
        {
            if (CheckSize(sizeof(Int16)))
            {
                byte[] bytes = new byte[sizeof(Int16)];
                this.data.CopyTo(ReadPos, bytes, 0, bytes.Length);
                data = BitConverter.ToInt16(bytes);

                ReadPos += sizeof(Int16);
            }

            return this;
        }

        public Packet Write(Int32 data)
        {
            Write(BitConverter.GetBytes(data));

            return this;
        }

        public Packet Read(ref Int32 data)
        {
            if (CheckSize(sizeof(Int32)))
            {
                byte[] bytes = new byte[sizeof(Int32)];
                this.data.CopyTo(ReadPos, bytes, 0, bytes.Length);
                data = BitConverter.ToInt32(bytes);

                ReadPos += sizeof(Int32);
            }

            return this;
        }

        public Packet Write(Int64 data)
        {
            Write(BitConverter.GetBytes(data));

            return this;
        }

        public Packet Read(ref Int64 data)
        {
            if (CheckSize(sizeof(Int64)))
            {
                byte[] bytes = new byte[sizeof(Int64)];
                this.data.CopyTo(ReadPos, bytes, 0, bytes.Length);
                data = BitConverter.ToInt64(bytes);

                ReadPos += sizeof(Int64);
            }

            return this;
        }

        public Packet Write(UInt16 data)
        {
            Write(BitConverter.GetBytes(data));

            return this;
        }

        public Packet Read(ref UInt16 data)
        {
            if (CheckSize(sizeof(UInt16)))
            {
                byte[] bytes = new byte[sizeof(UInt16)];
                this.data.CopyTo(ReadPos, bytes, 0, bytes.Length);
                data = BitConverter.ToUInt16(bytes);

                ReadPos += sizeof(UInt16);
            }

            return this;
        }

        public Packet Write(UInt32 data)
        {
            Write(BitConverter.GetBytes(data));

            return this;
        }

        public Packet Read(ref UInt32 data)
        {
            if (CheckSize(sizeof(UInt32)))
            {
                byte[] bytes = new byte[sizeof(UInt32)];
                this.data.CopyTo(ReadPos, bytes, 0, bytes.Length);
                data = BitConverter.ToUInt32(bytes);

                ReadPos += sizeof(UInt32);
            }

            return this;
        }

        public Packet Write(UInt64 data)
        {
            Write(BitConverter.GetBytes(data));

            return this;
        }

        public Packet Read(ref UInt64 data)
        {
            if (CheckSize(sizeof(UInt64)))
            {
                byte[] bytes = new byte[sizeof(UInt64)];
                this.data.CopyTo(ReadPos, bytes, 0, bytes.Length);
                data = BitConverter.ToUInt64(bytes);

                ReadPos += sizeof(UInt64);
            }

            return this;
        }

        public Packet Write(float data)
        {
            Write(BitConverter.GetBytes(data));

            return this;
        }

        public Packet Read(ref float data)
        {
            if (CheckSize(sizeof(float)))
            {
                byte[] bytes = new byte[sizeof(float)];
                this.data.CopyTo(ReadPos, bytes, 0, bytes.Length);
                data = BitConverter.ToSingle(bytes);

                ReadPos += sizeof(float);
            }

            return this;
        }

        public Packet Write(double data)
        {
            Write(BitConverter.GetBytes(data));

            return this;
        }

        public Packet Read(ref double data)
        {
            if (CheckSize(sizeof(double)))
            {
                byte[] bytes = new byte[sizeof(double)];
                this.data.CopyTo(ReadPos, bytes, 0, bytes.Length);
                data = BitConverter.ToDouble(bytes);

                ReadPos += sizeof(double);
            }

            return this;
        }

        public Packet Write(bool data)
        {
            Write(BitConverter.GetBytes(data));

            return this;
        }

        public Packet Read(ref bool data)
        {
            if (CheckSize(sizeof(bool)))
            {
                byte[] bytes = new byte[sizeof(bool)];
                this.data.CopyTo(ReadPos, bytes, 0, bytes.Length);
                data = BitConverter.ToBoolean(bytes);

                ReadPos += sizeof(bool);
            }

            return this;
        }

        public Packet Write(char data)
        {
            Write(BitConverter.GetBytes(data));

            return this;
        }

        public Packet Read(ref char data)
        {
            if (CheckSize(sizeof(char)))
            {
                byte[] bytes = new byte[sizeof(char)];
                this.data.CopyTo(ReadPos, bytes, 0, bytes.Length);
                data = BitConverter.ToChar(bytes);

                ReadPos += sizeof(char);
            }

            return this;
        }

        public Packet Write(string data)
        {
            // Convert to UTF-16 Char Array
            char[] charArray = data.ToCharArray();
            Write(BitConverter.GetBytes(charArray.Length * sizeof(char)));
            byte[] bytes = new byte[charArray.Length * sizeof(char)];
            Buffer.BlockCopy(charArray, 0, bytes, 0, charArray.Length * sizeof(char));
            Write(bytes);

            return this;
        }

        public Packet Read(ref string data)
        {
            uint length = 0;
            Read(ref length);

            if (CheckSize((int)length))
            {
                byte[] bytes = new byte[length];
                this.data.CopyTo(ReadPos, bytes, 0, (int)length);
                char[] charArray = new char[length / 2];
                Buffer.BlockCopy(bytes, 0, charArray, 0, (int)length);
                data = new string(charArray);

                ReadPos += (int)length;
            }

            return this;
        }
    }
}


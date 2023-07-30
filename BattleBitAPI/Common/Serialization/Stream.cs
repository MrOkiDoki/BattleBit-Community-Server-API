using BattleBitAPI.Common.Extentions;
using System.Net;

namespace BattleBitAPI.Common.Serialization
{
    public class Stream : IDisposable
    {
        public const int DefaultBufferSize = 1024 * 512;

#if BIGENDIAN
        public static readonly bool IsLittleEndian = false;
#else
        public static readonly bool IsLittleEndian = true;
#endif

        public byte[] Buffer;
        public int WritePosition;
        public int ReadPosition;
        public bool InPool;

        public bool CanRead(int size)
        {
            int readableLenght = WritePosition - ReadPosition;
            return (readableLenght >= size);
        }
        public void EnsureWriteBufferSize(int requiredSize)
        {
            int bufferLenght = Buffer.Length;

            int leftSpace = bufferLenght - WritePosition;
            if (leftSpace < requiredSize)
            {
                int newSize = bufferLenght + Math.Max(requiredSize, 1024);
                System.Array.Resize(ref Buffer, newSize);
            }
        }

        // -------- Write ------
        public void Write(byte value)
        {
            EnsureWriteBufferSize(1);
            Buffer[WritePosition] = value;
            WritePosition += 1;
        }
        public void Write(bool value)
        {
            EnsureWriteBufferSize(1);
            Buffer[WritePosition] = value ? (byte)1 : (byte)0;
            WritePosition += 1;
        }
        public unsafe void Write(short value)
        {
            EnsureWriteBufferSize(2);
            fixed (byte* ptr = &Buffer[WritePosition])
                *((short*)ptr) = value;
            WritePosition += 2;
        }
        public unsafe void Write(ushort value)
        {
            EnsureWriteBufferSize(2);
            fixed (byte* ptr = &Buffer[WritePosition])
                *((ushort*)ptr) = value;
            WritePosition += 2;
        }
        public unsafe void Write(int value)
        {
            EnsureWriteBufferSize(4);
            fixed (byte* ptr = &Buffer[WritePosition])
                *((int*)ptr) = value;
            WritePosition += 4;
        }
        public unsafe void Write(uint value)
        {
            EnsureWriteBufferSize(4);
            fixed (byte* ptr = &Buffer[WritePosition])
                *((uint*)ptr) = value;
            WritePosition += 4;
        }
        public unsafe void Write(long value)
        {
            EnsureWriteBufferSize(8);
            fixed (byte* ptr = &Buffer[WritePosition])
                *((long*)ptr) = value;
            WritePosition += 8;
        }
        public unsafe void Write(ulong value)
        {
            EnsureWriteBufferSize(8);
            fixed (byte* ptr = &Buffer[WritePosition])
                *((ulong*)ptr) = value;
            WritePosition += 8;
        }
        public unsafe void Write(decimal value)
        {
            EnsureWriteBufferSize(16);
            fixed (byte* ptr = &Buffer[WritePosition])
                *((decimal*)ptr) = value;
            WritePosition += 16;
        }
        public unsafe void Write(double value)
        {
            EnsureWriteBufferSize(16);
            fixed (byte* ptr = &Buffer[WritePosition])
                *((double*)ptr) = value;
            WritePosition += 16;
        }
        public unsafe void Write(float value)
        {
            int intValue = *((int*)&value);
            EnsureWriteBufferSize(4);
            fixed (byte* ptr = &Buffer[WritePosition])
                *((int*)ptr) = intValue;
            WritePosition += 4;
        }
        public unsafe void Write(string value)
        {
            int charCount = value.Length;
            fixed (char* strPtr = value)
            {
                int size = System.Text.Encoding.UTF8.GetByteCount(strPtr, charCount);
                EnsureWriteBufferSize(size + 2);


                fixed (byte* buffPtr = &Buffer[WritePosition])
                {
                    *((ushort*)buffPtr) = (ushort)size;
                    System.Text.Encoding.UTF8.GetBytes(strPtr, charCount, buffPtr + 2, size);
                }
                WritePosition += size + 2;
            }
        }
        public unsafe void Write(DateTime value)
        {
            var utc = value.ToUniversalTime();

            EnsureWriteBufferSize(8);
            fixed (byte* ptr = &Buffer[WritePosition])
                *((long*)ptr) = utc.Ticks;
            WritePosition += 8;
        }
        public unsafe void Write(IPAddress value)
        {
            uint ip = value.ToUInt();
            Write(ip);
        }
        public unsafe void Write(IPEndPoint value)
        {
            uint ip = value.Address.ToUInt();

            Write(ip);
            Write((ushort)value.Port);
        }
        public unsafe void WriteRaw(string value)
        {
            int charCount = value.Length;
            fixed (char* strPtr = value)
            {
                int size = System.Text.Encoding.UTF8.GetByteCount(strPtr, charCount);
                EnsureWriteBufferSize(size);

                fixed (byte* buffPtr = &Buffer[WritePosition])
                    System.Text.Encoding.UTF8.GetBytes(strPtr, charCount, buffPtr, size);
                WritePosition += size;
            }
        }
        public unsafe void Write<T>(T value) where T : IStreamSerializable
        {
            value.Write(this);
        }
        public void Write(byte[] source, int sourceIndex, int length)
        {
            if (length == 0)
                return;

            EnsureWriteBufferSize(length);
            System.Array.Copy(source, sourceIndex, this.Buffer, this.WritePosition, length);
            this.WritePosition += length;
        }
        public void Write(Stream source)
        {
            EnsureWriteBufferSize(source.WritePosition);
            System.Array.Copy(source.Buffer, 0, this.Buffer, this.WritePosition, source.WritePosition);
            this.WritePosition += source.WritePosition;
        }
        public void WriteStringItem(string value)
        {
            if (value == null)
                this.Write("none");
            else
                this.Write(value);
        }

        public unsafe void WriteAt(byte value, int position)
        {
            Buffer[position] = value;
        }
        public unsafe void WriteAt(short value, int position)
        {
            fixed (byte* ptr = &Buffer[position])
                *((short*)ptr) = value;
        }
        public unsafe void WriteAt(ushort value, int position)
        {
            fixed (byte* ptr = &Buffer[position])
                *((ushort*)ptr) = value;
        }
        public unsafe void WriteAt(int value, int position)
        {
            fixed (byte* ptr = &Buffer[position])
                *((int*)ptr) = value;
        }
        public unsafe void WriteAt(uint value, int position)
        {
            fixed (byte* ptr = &Buffer[position])
                *((uint*)ptr) = value;
        }
        public unsafe void WriteAt(long value, int position)
        {
            fixed (byte* ptr = &Buffer[position])
                *((long*)ptr) = value;
        }
        public unsafe void WriteAt(ulong value, int position)
        {
            fixed (byte* ptr = &Buffer[position])
                *((ulong*)ptr) = value;
        }

        // -------- Read ------
        public byte ReadInt8()
        {

            var value = Buffer[ReadPosition];
            ReadPosition++;

            return value;
        }
        public bool ReadBool()
        {


            var value = Buffer[ReadPosition];
            ReadPosition++;

            return value == 1;
        }
        public unsafe short ReadInt16()
        {
            short value = 0;

            fixed (byte* pbyte = &Buffer[ReadPosition])
            {
                if (ReadPosition % 2 == 0)
                {
                    value = *((short*)pbyte);
                }
                else
                {
                    if (IsLittleEndian)
                    {
                        value = (short)((*pbyte) | (*(pbyte + 1) << 8));
                    }
                    else
                    {
                        value = (short)((*pbyte << 8) | (*(pbyte + 1)));
                    }
                }
            }

            ReadPosition += 2;
            return value;
        }
        public unsafe ushort ReadUInt16()
        {
            ushort value = 0;

            fixed (byte* pbyte = &Buffer[ReadPosition])
            {
                if (ReadPosition % 2 == 0)
                {
                    value = *((ushort*)pbyte);
                }
                else
                {
                    if (IsLittleEndian)
                    {
                        value = (ushort)((*pbyte) | (*(pbyte + 1) << 8));
                    }
                    else
                    {
                        value = (ushort)((*pbyte << 8) | (*(pbyte + 1)));
                    }
                }
            }

            ReadPosition += 2;
            return value;
        }
        public unsafe int ReadInt32()
        {
            int value = 0;
            fixed (byte* pbyte = &Buffer[ReadPosition])
            {
                if (ReadPosition % 4 == 0)
                {
                    value = *((int*)pbyte);
                }
                else
                {
                    if (IsLittleEndian)
                    {
                        value = (int)((*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24));
                    }
                    else
                    {
                        value = (int)((*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | (*(pbyte + 3)));
                    }
                }
            }
            ReadPosition += 4;

            return value;
        }
        public unsafe uint ReadUInt32()
        {
            uint value = 0;
            fixed (byte* pbyte = &Buffer[ReadPosition])
            {
                if (ReadPosition % 4 == 0)
                {
                    value = *((uint*)pbyte);
                }
                else
                {
                    if (IsLittleEndian)
                    {
                        value = (uint)((*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24));
                    }
                    else
                    {
                        value = (uint)((*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | (*(pbyte + 3)));
                    }
                }
            }
            ReadPosition += 4;

            return value;
        }
        public unsafe long ReadInt64()
        {
            long value = 0;
            fixed (byte* pbyte = &Buffer[ReadPosition])
            {
                if (ReadPosition % 8 == 0)
                {
                    value = *((long*)pbyte);
                }
                else
                {
                    if (IsLittleEndian)
                    {
                        int i1 = (*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24);
                        int i2 = (*(pbyte + 4)) | (*(pbyte + 5) << 8) | (*(pbyte + 6) << 16) | (*(pbyte + 7) << 24);
                        value = (uint)i1 | ((long)i2 << 32);
                    }
                    else
                    {
                        int i1 = (*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | (*(pbyte + 3));
                        int i2 = (*(pbyte + 4) << 24) | (*(pbyte + 5) << 16) | (*(pbyte + 6) << 8) | (*(pbyte + 7));
                        value = (uint)i2 | ((long)i1 << 32);
                    }
                }
            }
            ReadPosition += 8;

            return value;
        }
        public unsafe ulong ReadUInt64()
        {
            ulong value = 0;
            fixed (byte* pbyte = &Buffer[ReadPosition])
            {
                if (ReadPosition % 8 == 0)
                {
                    value = *((ulong*)pbyte);
                }
                else
                {
                    if (IsLittleEndian)
                    {
                        int i1 = (*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24);
                        int i2 = (*(pbyte + 4)) | (*(pbyte + 5) << 8) | (*(pbyte + 6) << 16) | (*(pbyte + 7) << 24);
                        value = (uint)i1 | ((ulong)i2 << 32);
                    }
                    else
                    {
                        int i1 = (*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | (*(pbyte + 3));
                        int i2 = (*(pbyte + 4) << 24) | (*(pbyte + 5) << 16) | (*(pbyte + 6) << 8) | (*(pbyte + 7));
                        value = (uint)i2 | ((ulong)i1 << 32);
                    }
                }
            }
            ReadPosition += 8;

            return value;
        }
        public unsafe decimal ReadInt128()
        {


            decimal value = 0;
            fixed (byte* ptr = &Buffer[ReadPosition])
            {
                value = *((decimal*)ptr);
            }
            ReadPosition += 16;

            return value;
        }
        public unsafe double ReadDouble()
        {
            double value = 0;
            fixed (byte* ptr = &Buffer[ReadPosition])
            {
                value = *((double*)ptr);
            }
            ReadPosition += 16;

            return value;
        }
        public unsafe float ReadFloat()
        {
            int value = 0;
            fixed (byte* pbyte = &Buffer[ReadPosition])
            {
                if (ReadPosition % 4 == 0)
                {
                    value = *((int*)pbyte);
                }
                else
                {
                    if (IsLittleEndian)
                    {
                        value = (int)((*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24));
                    }
                    else
                    {
                        value = (int)((*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | (*(pbyte + 3)));
                    }
                }
            }
            ReadPosition += 4;

            return *(float*)&value;
        }
        public DateTime ReadDateTime()
        {
            long value = ReadInt64();
            try
            {
                return new DateTime(value, DateTimeKind.Utc);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
        public bool TryReadDateTime(out DateTime time)
        {
            long value = ReadInt64();
            try
            {
                time = new DateTime(value, DateTimeKind.Utc);
                return true;
            }
            catch { }

            time = default;
            return false;
        }
        public unsafe IPAddress ReadIPAddress()
        {
            uint ip = ReadUInt32();
            return new IPAddress(ip);
        }
        public unsafe IPEndPoint ReadIPEndPoint()
        {
            uint ip = ReadUInt32();
            ushort port = ReadUInt16();

            return new IPEndPoint(ip, port);
        }
        public T Read<T>() where T : IStreamSerializable
        {
            T value = default;
            value.Read(this);
            return value;
        }

        public byte[] ReadByteArray(int lenght)
        {
            if (lenght == 0)
                return new byte[0];

            byte[] newBuffer = new byte[lenght];
            System.Array.Copy(this.Buffer, this.ReadPosition, newBuffer, 0, lenght);
            this.ReadPosition += lenght;
            return newBuffer;
        }
        public void ReadTo(byte[] buffer, int offset, int size)
        {
            System.Array.Copy(this.Buffer, this.ReadPosition, buffer, offset, size);
            this.ReadPosition += size;
        }
        public unsafe string ReadString(int size)
        {
            string str;

#if NETCOREAPP
            fixed (byte* ptr = &Buffer[ReadPosition])
                str = System.Text.Encoding.UTF8.GetString(ptr, size);
#else
        str = System.Text.Encoding.UTF8.GetString(Buffer, ReadPosition, size);
#endif
            ReadPosition += size;

            return str;
        }
        public unsafe bool TryReadString(out string str)
        {
            if (!CanRead(2))
            {
                str = null;
                return false;
            }


            int size = 0;
            fixed (byte* ptr = &Buffer[ReadPosition])
                size = *((ushort*)ptr);
            ReadPosition += 2;

            if (!CanRead(size))
            {
                str = null;
                return false;
            }

#if NETCOREAPP
            fixed (byte* ptr = &Buffer[ReadPosition])
                str = System.Text.Encoding.UTF8.GetString(ptr, size);
#else
        str = System.Text.Encoding.UTF8.GetString(Buffer, ReadPosition, size);
#endif

            ReadPosition += size;

            return true;
        }
        public unsafe bool TryReadString(out string str, int maximumSize = ushort.MaxValue)
        {
            if (!CanRead(2))
            {
                str = null;
                return false;
            }


            int size = 0;
            fixed (byte* ptr = &Buffer[ReadPosition])
                size = *((ushort*)ptr);
            ReadPosition += 2;

            if (size > maximumSize)
            {
                str = null;
                return false;
            }

            if (!CanRead(size))
            {
                str = null;
                return false;
            }

#if NETCOREAPP
            fixed (byte* ptr = &Buffer[ReadPosition])
                str = System.Text.Encoding.UTF8.GetString(ptr, size);
#else
        str = System.Text.Encoding.UTF8.GetString(Buffer, ReadPosition, size);
#endif

            ReadPosition += size;

            return true;
        }
        public unsafe bool TrySkipString()
        {
            if (!CanRead(2))
                return false;

            int size = 0;
            fixed (byte* ptr = &Buffer[ReadPosition])
                size = *((ushort*)ptr);
            ReadPosition += 2;

            if (!CanRead(size))
                return false;

            ReadPosition += size;
            return true;
        }

        public int NumberOfBytesReadable
        {
            get => WritePosition - ReadPosition;
        }
        public void SkipWriting(int size)
        {
            EnsureWriteBufferSize(size);
            WritePosition += size;
        }
        public void SkipReading(int size)
        {
            ReadPosition += size;
        }

        // -------- Finalizing ------
        public byte[] AsByteArrayData()
        {
            var data = new byte[this.WritePosition];
            System.Array.Copy(this.Buffer, 0, data, 0, this.WritePosition);
            return data;
        }
        public void Reset()
        {
            this.ReadPosition = 0;
            this.WritePosition = 0;
        }
        public void Dispose()
        {
            if (InPool)
                return;
            InPool = true;

            lock (mPool)
                mPool.Enqueue(this);
        }

        // ------- Pool -----
        private static Queue<Stream> mPool = new Queue<Stream>(1024 * 256);
        public static Stream Get()
        {
            lock (mPool)
            {
                if (mPool.Count > 0)
                {
                    var item = mPool.Dequeue();
                    item.WritePosition = 0;
                    item.ReadPosition = 0;
                    item.InPool = false;

                    return item;
                }
            }

            return new Stream()
            {
                Buffer = new byte[DefaultBufferSize],
                InPool = false,
                ReadPosition = 0,
                WritePosition = 0,
            };
        }
    }
}
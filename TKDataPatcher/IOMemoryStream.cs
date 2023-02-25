using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace TKDataPatcher
{
    public class IOMemoryStream
    {
        public bool IsLittleEndian { get; private set; }

        public long Position { get => stream.Position; set => stream.Position = value; }

        public Stream Stream { get => stream; }

        private Stream stream;
        private bool disposedValue = false;

        public IOMemoryStream(Stream stream, bool isLittleEndian = true)
        {
            IsLittleEndian = isLittleEndian;
            this.stream = stream;
        }

        public void DebugReportPosition(string text)
        {
            Console.WriteLine($"{text}: 0x{Position:X8}");
        }

        ~IOMemoryStream()
        {
            Dispose(false);
        }

        /// <summary>
        /// Read bytes from IO stream.
        /// </summary>
        /// <param name="size">The size of the buffer.</param>
        /// <returns></returns>
        public byte[] ReadBytes(int size)
        {
            byte[] buffer = new byte[size];

            stream.Read(buffer, 0, size);

            if (IsLittleEndian != BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            return buffer;
        }

        public void WriteBytes(byte[] buffer)
        {
            if (IsLittleEndian != BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            stream.Write(buffer, 0, buffer.Length);
        }

        public void WriteInt(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public byte ReadByte()
        {
            return ReadBytes(1)[0];
        }

        public short ReadShort()
        {
            return BitConverter.ToInt16(ReadBytes(2), 0);
        }

        public ushort ReadUShort()
        {
            return BitConverter.ToUInt16(ReadBytes(2), 0);
        }

        public int ReadInt()
        {
            return BitConverter.ToInt32(ReadBytes(4), 0);
        }

        public uint ReadUInt()
        {
            return BitConverter.ToUInt32(ReadBytes(4), 0);
        }

        public long ReadLong()
        {
            return BitConverter.ToInt64(ReadBytes(4), 0);
        }

        public ulong ReadULong()
        {
            byte[] buffer = ReadBytes(8);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public Guid ReadGuid()
        {
            byte[] buffer = ReadBytes(0x10);
            return new Guid(buffer);
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }

        public double ReadDouble()
        {
            return BitConverter.ToDouble(ReadBytes(8), 0);
        }

        public bool ReadByteBool()
        {
            return ReadByte() != 0x00;
        }

        public bool ReadIntBool()
        {
            int data = ReadInt();
            //This is really bad, Wildcard....
            if (data != 0 && data != 1)
                throw new Exception("Expected boolean, got " + data);
            return data == 1;
        }

        public string ReadStringOffset(uint offset)
        {
            long oldPosition = stream.Position;
            stream.Position = offset;
            string x = ReadASCIIString();
            stream.Position = oldPosition;
            return x;
        }

        public T[] ReadTArray<T>(Func<T> Getter)
        {
            int serializeNum = ReadInt();
            T[] A = new T[serializeNum];

            for (int i = 0; i < serializeNum; i++)
            {
                A[i] = Getter();
            }

            return A;
        }

        public string ReadASCIIString()
        {
            List<byte> bytes = new List<byte>();
            byte b = ReadByte();
            if (b == 0x00)
            {
                return "\x00";
            }
            bytes.Add(b);
            while ((b = ReadByte()) != 0x00)
            {
                bytes.Add(b);
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        public long[] FindPatterns(byte[] pattern, int startPos = 0)
        {
            stream.Position = startPos;
            List<long> searchResults = new List<long>();
            int patternPosition = 0; //Track of how much of the array has been matched
            long filePosition = 0;
            long bufferSize = Math.Min(stream.Length, 100_000);

            byte[] buffer = new byte[bufferSize];
            int readCount = 0;

            while ((readCount = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < readCount; i++)
                {
                    byte currentByte = buffer[i];

                    if (currentByte == pattern[0])
                        patternPosition = 0;

                    if (currentByte == pattern[patternPosition])
                    {
                        patternPosition++;
                        if (patternPosition == pattern.Length)
                        {
                            searchResults.Add(filePosition + 1 - pattern.Length);
                            patternPosition = 0;
                        }
                    }
                    filePosition++;
                }
            }

            return searchResults.ToArray();
        }

        public string ReadUnicodeString(int maxLength = 10485760)
        {
            //Read length
            int length = this.ReadInt();
            if (length == 0)
                return "";

            //Validate length
            if (length > maxLength)
                throw new Exception("Failed to read null-terminated string; Length from file exceeded maximum length requested.");

            //My friend's arg broke this reader. Turns out extended characters use TWO bytes. I think if the length is negative, it's two bytes per character
            if (length < 0)
            {
                //Read this many bytes * 2
                byte[] buffer = ReadBytes((-length * 2) - 1);

                //Read null byte, but discard
                byte nullByte1 = ReadByte();
                if (nullByte1 != 0x00)
                    throw new Exception("Failed to read null-terminated string; 1st terminator in 2-bytes-per-character string was not null!");

                //Convert to string
                return Encoding.Unicode.GetString(buffer);
            }
            else
            {
                //Read this many bytes.
                byte[] buffer = ReadBytes(length - 1);
                //Read null byte, but discard
                byte nullByte = ReadByte();
                if (nullByte != 0x00)
                    throw new Exception("Failed to read null-terminated string; Terminator was not null!");
                //Convert to string
                return Encoding.UTF8.GetString(buffer);
            }
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    stream.Dispose();
                }

                stream = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

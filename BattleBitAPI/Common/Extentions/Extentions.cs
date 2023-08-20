using System.Net;
using System.Net.Sockets;

namespace BattleBitAPI.Common.Extentions
{
    public static class Extentions
    {
        public static long TickCount
        {
            get
            {
#if NETCOREAPP
                return System.Environment.TickCount64;
#else
                return (long)Environment.TickCount;
#endif
            }
        }
        public unsafe static uint ToUInt(this IPAddress address)
        {
#if NETCOREAPP
            return BitConverter.ToUInt32(address.GetAddressBytes());
#else
        return BitConverter.ToUInt32(address.GetAddressBytes(), 0);
#endif
        }

        public static void Replace<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            dic.Remove(key);
            dic.Add(key, value);
        }

        public static void SafeClose(this TcpClient client)
        {
            try { client.Close(); } catch { }
            try { client.Dispose(); } catch { }
        }
        public static async Task<int> Read(this NetworkStream networkStream, Serialization.Stream outputStream, int size, CancellationToken token = default)
        {
            int read = 0;
            int readUntil = outputStream.WritePosition + size;

            //Ensure we have space.
            outputStream.EnsureWriteBufferSize(size);

            //Continue reading until we have the package.
            while (outputStream.WritePosition < readUntil)
            {
                int sizeToRead = readUntil - outputStream.WritePosition;
                int received = await networkStream.ReadAsync(outputStream.Buffer, outputStream.WritePosition, sizeToRead, token);
                if (received <= 0)
                    throw new Exception("NetworkStream was closed.");

                read += received;
                outputStream.WritePosition += received;
            }

            return read;
        }
        public static async Task<bool> TryRead(this NetworkStream networkStream, Serialization.Stream outputStream, int size, CancellationToken token = default)
        {
            try
            {
                int readUntil = outputStream.WritePosition + size;

                //Ensure we have space.
                outputStream.EnsureWriteBufferSize(size);

                //Continue reading until we have the package.
                while (outputStream.WritePosition < readUntil)
                {
                    int sizeToRead = readUntil - outputStream.WritePosition;
                    int received = await networkStream.ReadAsync(outputStream.Buffer, outputStream.WritePosition, sizeToRead, token);
                    if (received <= 0)
                        throw new Exception("NetworkStream was closed.");
                    outputStream.WritePosition += received;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

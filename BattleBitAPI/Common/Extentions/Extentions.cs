using System.Net;
using System.Net.Sockets;
using Stream = BattleBitAPI.Common.Serialization.Stream;

namespace BattleBitAPI.Common.Extentions;

public static class Extentions
{
    public static long TickCount
    {
        get
        {
#if NETCOREAPP
            return Environment.TickCount64;
#else
                return (long)Environment.TickCount;
#endif
        }
    }

    public static uint ToUInt(this IPAddress address)
    {
#if NETCOREAPP
        return BitConverter.ToUInt32(address.GetAddressBytes());
#else
        return BitConverter.ToUInt32(address.GetAddressBytes(), 0);
#endif
    }

    public static void SafeClose(this TcpClient client)
    {
        try
        {
            client.Close();
        }
        catch
        {
        }

        try
        {
            client.Dispose();
        }
        catch
        {
        }
    }

    public static async Task<int> Read(this NetworkStream networkStream, Stream outputStream, int size, CancellationToken token = default)
    {
        var read = 0;
        var readUntil = outputStream.WritePosition + size;

        //Ensure we have space.
        outputStream.EnsureWriteBufferSize(size);

        //Continue reading until we have the package.
        while (outputStream.WritePosition < readUntil)
        {
            var sizeToRead = readUntil - outputStream.WritePosition;
            var received = await networkStream.ReadAsync(outputStream.Buffer, outputStream.WritePosition, sizeToRead, token);
            if (received <= 0)
                throw new Exception("NetworkStream was closed.");

            read += received;
            outputStream.WritePosition += received;
        }

        return read;
    }

    public static async Task<bool> TryRead(this NetworkStream networkStream, Stream outputStream, int size, CancellationToken token = default)
    {
        try
        {
            var readUntil = outputStream.WritePosition + size;

            //Ensure we have space.
            outputStream.EnsureWriteBufferSize(size);

            //Continue reading until we have the package.
            while (outputStream.WritePosition < readUntil)
            {
                var sizeToRead = readUntil - outputStream.WritePosition;
                var received = await networkStream.ReadAsync(outputStream.Buffer, outputStream.WritePosition, sizeToRead, token);
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
#region

using System.Net.Sockets;

using CommunityServerAPI.BattleBitAPI.Packets;

using Stream = BattleBitAPI.Common.Serialization.Stream;

#endregion

namespace CommunityServerAPI.BattleBitAPI.Common.Extentions;

public static class NetworkStreamExtensions
{
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

	/// <summary>
	/// Serialize the provided packet and send it down the network stream.
	/// Returns false if an error occured, true otherwise.
	/// </summary>
	/// <param name="self"></param>
	/// <param name="packet"></param>
	/// <returns></returns>
	public static async Task<bool> TryWritePacket(this NetworkStream self, BasePacket packet)
	{
		using (var stream = Stream.Get())
		{
			if (!packet.TryWrite(stream))
				return false;

			self.Write(stream.Buffer, 0, stream.WritePosition);
			self.Flush();
		}

		return true;
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

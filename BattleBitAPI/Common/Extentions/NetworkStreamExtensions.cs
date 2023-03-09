#region

using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

using BattleBitAPI.Common.Enums;
using BattleBitAPI.Networking;

using CommunityServerAPI.BattleBitAPI.Packets;


#endregion

namespace CommunityServerAPI.BattleBitAPI.Common.Extentions;

public static class NetworkStreamExtensions
{
	public static async Task<int> Read(this Stream networkStream, Stream outputStream, int size, CancellationToken token = default)
	{
		var read = 0;
		var readUntil = outputStream.Position + size;

		//Ensure we have space.
		//outputStream.EnsureWriteBufferSize(size);

		//Continue reading until we have the package.
		while (outputStream.Position < readUntil)
		{
			var sizeToRead = readUntil - outputStream.Position;
			var buffer = new byte[sizeToRead];
			var received = await networkStream.ReadAsync(buffer, (int) outputStream.Position, (int) sizeToRead, token);
			if (received <= 0)
				throw new Exception("NetworkStream was closed.");

			outputStream.Write(buffer);

			read += received;
		}

		return read;
	}

	/// <summary>
	///     Serialize the provided packet and send it down the network stream.
	///     Returns false if an error occured, true otherwise.
	/// </summary>
	/// <param name="self"></param>
	/// <param name="packet"></param>
	/// <returns></returns>
	public static bool TryWritePacket(this NetworkStream self, BasePacket packet)
	{
		using (var stream = new BinaryWriter(self))
		{
			if (!packet.TryWrite(stream, CancellationToken.None))
				return false;
		}
		self.Flush();

		return true;
	}

	public static int TryReadSigned(this Stream self, CancellationToken token, int size)
	{
		using (var readStream = new BinaryReader(self))
		{
			switch (size)
			{
				case 1: return readStream.ReadByte();
				case 2: return readStream.ReadInt16();
				case 4: return readStream.ReadInt32();
			}
			throw new Exception($"Invalid int size {size}.");
		}
	}

	/// <summary>
	///     !! This blocks the current thread !!
	///     Await a response code from the other end of the socket.
	///     Returns "None" if the connection timed out.
	/// </summary>
	/// <param name="self"></param>
	/// <returns></returns>
	public static NetworkCommuncation AwaitResponse(this TcpClient self)
	{
		var watch = Stopwatch.StartNew();
		while (watch.ElapsedMilliseconds < Const.HailConnectTimeout)
		{
			if (self.Available > 0)
			{
				var data = self.GetStream().ReadByte();
				if (data >= 0)
				{
					return (NetworkCommuncation)data;
					break;
				}
			}
			Thread.Sleep(1);
		}

		return NetworkCommuncation.None;
	}

	public static byte[] ToByteArray(this Stream self)
	{
		var buffer = new byte[self.Length];
		self.Write(buffer);

		return buffer;
	}

	public static async Task<bool> TryRead(this Stream networkStream, Stream outputStream, int size, CancellationToken token = default)
	{
		try
		{


			var read = 0;
			var readUntil = outputStream.Position + size;

			//Ensure we have space.
			//outputStream.EnsureWriteBufferSize(size);

			//Continue reading until we have the package.
			while (outputStream.Position < readUntil)
			{
				var sizeToRead = readUntil - outputStream.Position;
				var buffer = new byte[sizeToRead];
				var received = await networkStream.ReadAsync(buffer, (int)outputStream.Position, (int)sizeToRead, token);
				if (received <= 0)
					throw new Exception("NetworkStream was closed.");

				outputStream.Write(buffer);

				read += received;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}
}

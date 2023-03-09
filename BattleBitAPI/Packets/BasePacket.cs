#region

#endregion

namespace CommunityServerAPI.BattleBitAPI.Packets;

public abstract class BasePacket
{
	/// <summary>
	///     Attempt to write this packet to the provided stream.
	///     Return true if the write was successful.
	/// </summary>
	/// <param name="destination"></param>
	/// <returns></returns>
	public abstract bool TryWrite(BinaryWriter destination, CancellationToken token);

	/// <summary>
	///     Attempt to read this packet type from the stream.
	///     Returns true if the read was successful.
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public abstract bool TryRead(BinaryReader source, CancellationToken token);
}

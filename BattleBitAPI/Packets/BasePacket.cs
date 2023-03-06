#region

using Stream = BattleBitAPI.Common.Serialization.Stream;

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
	public abstract bool TryWrite(Stream destination);
}

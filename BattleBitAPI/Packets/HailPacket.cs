#region

using BattleBitAPI.Common.Enums;
using BattleBitAPI.Networking;

using Stream = BattleBitAPI.Common.Serialization.Stream;

#endregion

namespace CommunityServerAPI.BattleBitAPI.Packets;

public class HailPacket : BasePacket
{
	public byte CurrentPlayers;
	public MapDayNight DayNight;
	public string Gamemode;

	public ushort GamePort;
	public byte InQueuePlayers;
	public bool IsPasswordProtected;
	public string LoadingScreenText;
	public string Map;
	public MapSize MapSize;
	public byte MaxPlayers;
	public string ServerName;
	public string ServerRulesText;

	public override bool TryWrite(Stream destination)
	{
		destination.Write((byte)NetworkCommuncation.Hail);
		destination.Write((ushort)GamePort);
		destination.Write(IsPasswordProtected);
		destination.Write(ServerName);
		destination.Write(Gamemode);
		destination.Write(Map);
		destination.Write((byte)MapSize);
		destination.Write((byte)DayNight);
		destination.Write((byte)CurrentPlayers);
		destination.Write((byte)InQueuePlayers);
		destination.Write((byte)MaxPlayers);
		destination.Write(LoadingScreenText);
		destination.Write(ServerRulesText);

		return true;
	}
}

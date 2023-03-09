#region

using BattleBitAPI.Common.Enums;
using BattleBitAPI.Networking;

#endregion

namespace CommunityServerAPI.BattleBitAPI.Packets;

public class HailPacket : BasePacket
{
	public int CurrentPlayers;
	public MapDayNight DayNight;
	public string Gamemode;

	public int GamePort;
	public int InQueuePlayers;
	public bool IsPasswordProtected;
	public string LoadingScreenText;
	public string Map;
	public MapSize MapSize;
	public int MaxPlayers;
	public string ServerName;
	public string ServerRulesText;

	public override bool TryWrite(BinaryWriter destination, CancellationToken token)
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

	/// <summary>
	/// TODO: DO we want exceptions here, or do we just want to return false?
	/// Having exceptions goes against general .NET semantics, but it'd be good for debugging.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="token"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public override bool TryRead(BinaryReader source, CancellationToken token)
	{
		//Read port
		GamePort = source.ReadUInt16();

		//Read is Port protected
		IsPasswordProtected = source.ReadBoolean();

		//	TODO: Can this be used as a DOS attack with large strings?
		//	Ensure that the length of the hail packet is capped.
		ServerName = source.ReadString();

		if (ServerName.Length < Const.MinServerNameLength || ServerName.Length > Const.MaxServerNameLength)
			throw new Exception("Invalid server name size");

		//Read the gamemode
		Gamemode = source.ReadString();
		if (Gamemode.Length < Const.MinGamemodeNameLength || Gamemode.Length > Const.MaxGamemodeNameLength)
			throw new Exception("Invalid gamemode size");

		Map = source.ReadString();
		if (Map.Length < Const.MinMapNameLength || Map.Length > Const.MaxMapNameLength)
			throw new Exception("Invalid map size");

		MapSize = (MapSize)source.ReadByte();
		DayNight = (MapDayNight)source.ReadByte();
		CurrentPlayers = source.ReadByte();
		InQueuePlayers = source.ReadByte();
		MaxPlayers = source.ReadByte();

		LoadingScreenText = source.ReadString();
		if (LoadingScreenText.Length < Const.MinLoadingScreenTextLength || LoadingScreenText.Length > Const.MaxLoadingScreenTextLength)
			throw new Exception("Invalid server Loading Screen Text Size");

		ServerRulesText = source.ReadString();
		if (ServerRulesText.Length < Const.MinServerRulesTextLength || ServerRulesText.Length > Const.MaxServerRulesTextLength)
			throw new Exception("Invalid server Server Rules Text Size");

		return true;
	}
}

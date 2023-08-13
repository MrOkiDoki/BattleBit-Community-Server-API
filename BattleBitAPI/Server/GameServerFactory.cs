namespace BattleBitAPI.Server;

public class GameServerFactory<TGameServer, TPlayer>
	where TGameServer: GameServer<TPlayer>
	where TPlayer: Player<TPlayer>
{

	/// <summary>
	/// Create a new gameserver instance.
	/// This will be whatever type you want to handle incoming
	/// events for a connecting gameserver.
	/// </summary>
	/// <returns></returns>
	public virtual TGameServer Create()
	{
		TGameServer gameServer = (TGameServer)Activator.CreateInstance(typeof(TGameServer));

		return gameServer;
	}

}

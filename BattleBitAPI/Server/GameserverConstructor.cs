namespace BattleBitAPI.Server;

public class GameserverConstructor<TGameServer, TPlayer>
	where TGameServer: GameServer<TPlayer>
	where TPlayer: Player<TPlayer>
{

	public virtual TGameServer Create()
	{
		TGameServer gameServer = (TGameServer)Activator.CreateInstance(typeof(TGameServer));

		return gameServer;
	}

}

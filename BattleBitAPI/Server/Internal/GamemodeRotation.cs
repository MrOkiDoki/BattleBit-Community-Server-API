namespace BattleBitAPI.Server
{
    public class GamemodeRotation<TPlayer> where TPlayer : Player<TPlayer>
    {
        private GameServer<TPlayer>.Internal mResources;
        public GamemodeRotation(GameServer<TPlayer>.Internal resources)
        {
            mResources = resources;
        }

        public IEnumerable<string> GetGamemodeRotation()
        {
            lock (mResources._GamemodeRotation)
                return new List<string>(mResources._GamemodeRotation);
        }
        public bool InRotation(string gamemode)
        {
            lock (mResources._GamemodeRotation)
                return mResources._GamemodeRotation.Contains(gamemode);
        }
        public bool RemoveFromRotation(string gamemode)
        {
            lock (mResources._GamemodeRotation)
                if (!mResources._GamemodeRotation.Remove(gamemode))
                    return false;
            mResources.IsDirtyGamemodeRotation = true;
            return true;
        }
        public bool AddToRotation(string gamemode)
        {
            lock (mResources._GamemodeRotation)
                if (!mResources._GamemodeRotation.Add(gamemode))
                    return false;
            mResources.IsDirtyGamemodeRotation = true;
            return true;
        }

        public void Reset()
        {
        }
    }
}

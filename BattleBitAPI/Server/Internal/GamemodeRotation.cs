namespace BattleBitAPI.Server
{
    public class GamemodeRotation
    {
        private GameServer.mInternalResources mResources;
        public GamemodeRotation(GameServer.mInternalResources resources)
        {
            mResources = resources;
        }

        public IEnumerable<string> GetGamemodeRotation()
        {
            lock (mResources.GamemodeRotation)
                return new List<string>(mResources.GamemodeRotation);
        }
        public bool InRotation(string gamemode)
        {
            lock (mResources.GamemodeRotation)
                return mResources.GamemodeRotation.Contains(gamemode);
        }
        public bool RemoveFromRotation(string gamemode)
        {
            lock (mResources.GamemodeRotation)
                if (!mResources.GamemodeRotation.Remove(gamemode))
                    return false;
            mResources.GamemodeRotationDirty = true;
            return true;
        }
        public bool AddToRotation(string gamemode)
        {
            lock (mResources.GamemodeRotation)
                if (!mResources.GamemodeRotation.Add(gamemode))
                    return false;
            mResources.GamemodeRotationDirty = true;
            return true;
        }
    }
}

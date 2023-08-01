namespace BattleBitAPI.Server
{
    public class MapRotation
    {
        private GameServer.mInternalResources mResources;
        public MapRotation(GameServer.mInternalResources resources)
        {
            mResources = resources;
        }

        public IEnumerable<string> GetMapRotation()
        {
            lock (mResources.MapRotation)
                return new List<string>(mResources.MapRotation);
        }
        public bool InRotation(string map)
        {
            map = map.ToUpperInvariant();

            lock (mResources.MapRotation)
                return mResources.MapRotation.Contains(map);
        }
        public bool RemoveFromRotation(string map)
        {
            map = map.ToUpperInvariant();

            lock (mResources.MapRotation)
                if (!mResources.MapRotation.Remove(map))
                    return false;
            mResources.MapRotationDirty = true;
            return true;
        }
        public bool AddToRotation(string map)
        {
            map = map.ToUpperInvariant();

            lock (mResources.MapRotation)
                if (!mResources.MapRotation.Add(map))
                    return false;
            mResources.MapRotationDirty = true;
            return true;
        }
    }
}

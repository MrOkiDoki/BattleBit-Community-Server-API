namespace BattleBitAPI.Server
{
    public class MapRotation<TPlayer> where TPlayer : Player<TPlayer>
    {
        private GameServer<TPlayer>.Internal mResources;
        public MapRotation(GameServer<TPlayer>.Internal resources)
        {
            mResources = resources;
        }

        public IEnumerable<string> GetMapRotation()
        {
            lock (mResources._MapRotation)
                return new List<string>(mResources._MapRotation);
        }
        public bool InRotation(string map)
        {
            map = map.ToUpperInvariant();

            lock (mResources._MapRotation)
                return mResources._MapRotation.Contains(map);
        }
        public bool RemoveFromRotation(string map)
        {
            map = map.ToUpperInvariant();

            lock (mResources._MapRotation)
                if (!mResources._MapRotation.Remove(map))
                    return false;
            mResources.IsDirtyMapRotation = true;
            return true;
        }
        public bool AddToRotation(string map)
        {
            map = map.ToUpperInvariant();

            lock (mResources._MapRotation)
                if (!mResources._MapRotation.Add(map))
                    return false;
            mResources.IsDirtyMapRotation = true;
            return true;
        }
        public void SetRotation(params string[] maps)
        {
            lock (mResources._MapRotation)
            {
                mResources._MapRotation.Clear();
                foreach (var item in maps)
                    mResources._MapRotation.Add(item);
            }
            mResources.IsDirtyMapRotation = true;
        }
        public void ClearRotation()
        {
            lock (mResources._MapRotation)
            {
                if (mResources._MapRotation.Count == 0)
                    return;

                mResources._MapRotation.Clear();
            }
            mResources.IsDirtyMapRotation = true;
        }

        public void Reset()
        {
        }
    }
}

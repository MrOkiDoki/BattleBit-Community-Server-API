namespace BattleBitAPI.Pooling
{
    public class ItemPooling<TItem>
    {
        private Queue<ItemPooling<TItem>.List> mPool;
        private int mDefaultCount;
        public ItemPooling(int defaultCount)
        {
            this.mPool = new Queue<ItemPooling<TItem>.List>(6);
            this.mDefaultCount = defaultCount;
        }

        public ItemPooling<TItem>.List Get()
        {
            lock (mPool)
            {
                if (mPool.Count > 0)
                    return mPool.Dequeue();
            }
            return new ItemPooling<TItem>.List(this, mDefaultCount);
        }
        public void Post(ItemPooling<TItem>.List item)
        {
            lock (mPool)
                mPool.Enqueue(item);
        }

        public class List : IDisposable
        {
            private ItemPooling<TItem> mParent;
            public List<TItem> ListItems;

            public List(ItemPooling<TItem> parent, int count)
            {
                this.mParent = parent;
                this.ListItems = new List<TItem>(count);
            }


            public void Dispose()
            {
                ListItems.Clear();
                mParent.Post(this);
            }
        }
    }
}

namespace BattleBitAPI.Common.Threading;

public class ThreadSafe<T>
{
    private readonly ReaderWriterLockSlim mLock;
    public T Value;

    public ThreadSafe(T value)
    {
        Value = value;
        mLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    }

    public SafeWriteHandle GetWriteHandle()
    {
        return new SafeWriteHandle(mLock);
    }

    public SafeReadHandle GetReadHandle()
    {
        return new SafeReadHandle(mLock);
    }

    /// <summary>
    ///     Swaps current value with new value and returns old one.
    /// </summary>
    /// <param name="newValue"></param>
    /// <returns>Old value</returns>
    public T SwapValue(T newValue)
    {
        using (new SafeWriteHandle(mLock))
        {
            var oldValue = Value;
            Value = newValue;
            return oldValue;
        }
    }
}

public class SafeWriteHandle : IDisposable
{
    private bool mDisposed;
    private readonly ReaderWriterLockSlim mLock;

    public SafeWriteHandle(ReaderWriterLockSlim mLock)
    {
        this.mLock = mLock;
        mLock.EnterWriteLock();
    }

    public void Dispose()
    {
        if (mDisposed)
            return;
        mDisposed = true;
        mLock.ExitWriteLock();
    }
}

public class SafeReadHandle : IDisposable
{
    private bool mDisposed;
    private readonly ReaderWriterLockSlim mLock;

    public SafeReadHandle(ReaderWriterLockSlim mLock)
    {
        this.mLock = mLock;
        mLock.EnterReadLock();
    }

    public void Dispose()
    {
        if (mDisposed)
            return;
        mDisposed = true;

        mLock.ExitReadLock();
    }
}
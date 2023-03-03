using System;
using System.Collections.Generic;

namespace BattleBitAPI.Common.Threading
{
    public class ThreadSafe<T>
    {
        private System.Threading.ReaderWriterLockSlim mLock;
        public T Value;

        public ThreadSafe(T value)
        {
            this.Value = value;
            this.mLock = new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.SupportsRecursion);
        }

        public SafeWriteHandle GetWriteHandle() => new SafeWriteHandle(this.mLock);
        public SafeReadHandle GetReadHandle() => new SafeReadHandle(this.mLock);

        /// <summary>
        /// Swaps current value with new value and returns old one.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns>Old value</returns>
        public T SwapValue(T newValue)
        {
            using (new SafeWriteHandle(this.mLock))
            {
                var oldValue = this.Value;
                this.Value = newValue;
                return oldValue;
            }
        }
    }
    public class SafeWriteHandle : System.IDisposable
    {
        private System.Threading.ReaderWriterLockSlim mLock;
        private bool mDisposed;
        public SafeWriteHandle(System.Threading.ReaderWriterLockSlim mLock)
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
    public class SafeReadHandle : System.IDisposable
    {
        private System.Threading.ReaderWriterLockSlim mLock;
        private bool mDisposed;
        public SafeReadHandle(System.Threading.ReaderWriterLockSlim mLock)
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
}
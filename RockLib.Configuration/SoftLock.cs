using System.Threading;

namespace RockLib.Configuration;

/// <summary>
/// This is a copy of 
/// https://raw.githubusercontent.com/RockLib/RockLib.Threading/main/RockLib.Threading/SoftLock.cs.
/// There are no updates to this code, so it was decided to inline the implementation into RockLib.Configuration
/// </summary>
internal sealed class SoftLock
{
    private const int _lockNotAcquired = 0;
    private const int _lockAcquired = 1;

    private int _lock;

    /// <summary>
    /// Try to acquire the lock. Returns true if the lock is acquired. Returns false if the lock has
    /// already been acquired.
    /// </summary>
    /// <returns>True, if the lock was acquired. False, if another thread currently has the lock</returns>
    public bool TryAcquire()
    {
        return Interlocked.Exchange(ref _lock, _lockAcquired) == _lockNotAcquired;
    }

    /// <summary>
    /// Release the lock. Should only be called after successfully acquiring the lock.
    /// </summary>
    public void Release()
    {
        Interlocked.Exchange(ref _lock, _lockNotAcquired);
    }

    /// <summary>
    /// Gets a value indicating whether the lock has been acquired.
    /// </summary>
    public bool IsLockAcquired => _lock == _lockAcquired;
}
using System.Collections.Concurrent;

namespace BudgetBoard.Service.Helpers;

public static class NetWorthWidgetConfigurationLock
{
    internal sealed class LockEntry
    {
        public LockEntry(SemaphoreSlim semaphore)
        {
            Semaphore = semaphore;
        }

        public SemaphoreSlim Semaphore { get; }
        public int ActiveCount;
    }

    private static readonly ConcurrentDictionary<Guid, LockEntry> Locks = new();

    public static async Task<DisposableLock> AcquireLockAsync(Guid widgetSettingsId)
    {
        var lockEntry = Locks.GetOrAdd(
            widgetSettingsId,
            _ => new LockEntry(new SemaphoreSlim(1, 1))
        );
        Interlocked.Increment(ref lockEntry.ActiveCount);

        try
        {
            await lockEntry.Semaphore.WaitAsync();
            return new DisposableLock(widgetSettingsId, lockEntry);
        }
        catch
        {
            if (Interlocked.Decrement(ref lockEntry.ActiveCount) == 0)
            {
                RemoveIfUnused(widgetSettingsId, lockEntry);
            }

            throw;
        }
    }

    private static void RemoveIfUnused(Guid widgetSettingsId, LockEntry lockEntry)
    {
        while (true)
        {
            if (lockEntry.ActiveCount != 0)
            {
                return;
            }

            if (
                !Locks.TryGetValue(widgetSettingsId, out var currentEntry)
                || !ReferenceEquals(currentEntry, lockEntry)
            )
            {
                return;
            }

            if (
                Locks.TryRemove(widgetSettingsId, out var removedEntry)
                && ReferenceEquals(removedEntry, lockEntry)
            )
            {
                lockEntry.Semaphore.Dispose();
                return;
            }
        }
    }

    public sealed class DisposableLock : IDisposable
    {
        private readonly Guid _widgetSettingsId;
        private readonly LockEntry _lockEntry;
        private bool _disposed;

        internal DisposableLock(Guid widgetSettingsId, LockEntry lockEntry)
        {
            _widgetSettingsId = widgetSettingsId;
            _lockEntry = lockEntry;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                _lockEntry.Semaphore.Release();
            }
            finally
            {
                if (Interlocked.Decrement(ref _lockEntry.ActiveCount) == 0)
                {
                    RemoveIfUnused(_widgetSettingsId, _lockEntry);
                }

                _disposed = true;
            }
        }
    }
}

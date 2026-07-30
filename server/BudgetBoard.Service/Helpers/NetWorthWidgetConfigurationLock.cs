using System.Collections.Concurrent;

namespace BudgetBoard.Service.Helpers;

public static class NetWorthWidgetConfigurationLock
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> Locks = new();

    public static async Task<DisposableLock> AcquireLockAsync(Guid widgetSettingsId)
    {
        var semaphore = Locks.GetOrAdd(widgetSettingsId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        return new DisposableLock(semaphore);
    }

    public sealed class DisposableLock(SemaphoreSlim semaphore) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            semaphore.Release();
            _disposed = true;
        }
    }
}

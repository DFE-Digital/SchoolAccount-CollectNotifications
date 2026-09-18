namespace SchoolAccount.CollectNotifications.Interfaces;

public interface IThreadingService
{
    Task Batch<TSource>(
        IEnumerable<TSource> items,
        CancellationToken cancellationToken,
        Func<TSource, CancellationToken, Task<bool>> func);
}

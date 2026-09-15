using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Interfaces;

public interface ILedgerStore
{
    Task<Result<List<CollectReturnStatus>>> GetWhatHasChangedAsync(
        DateTime lastRunDate,
        List<string> laeStabKeys,
        CancellationToken cancellationToken = default);
}

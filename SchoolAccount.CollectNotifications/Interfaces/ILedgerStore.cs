using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Interfaces;

public interface ILedgerStore
{
    Task<Result<List<ComparableCollectReturnStatus>>> GetWhatHasChangedAsync(
        DateTime lastRunDate,
        List<string> laeStabKeys,
        bool limitToApprovedStatuses = true,
        CancellationToken cancellationToken = default);
}

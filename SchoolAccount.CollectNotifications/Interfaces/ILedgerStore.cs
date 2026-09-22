using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;

namespace SchoolAccount.CollectNotifications.Interfaces;

public interface ILedgerStore
{
    Task<Result<List<CensusStatusChange>>> GetWhatHasChangedAsync(
        DateTime lastRunDate,
        bool limitToApprovedStatuses = true,
        CancellationToken cancellationToken = default);
}

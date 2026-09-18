using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;

namespace SchoolAccount.CollectNotifications.Interfaces;

public interface IEnrollmentStore
{
    Task<Result<List<EnrolledRecipient>>> ListAsync(CancellationToken cancellationToken = default);
}
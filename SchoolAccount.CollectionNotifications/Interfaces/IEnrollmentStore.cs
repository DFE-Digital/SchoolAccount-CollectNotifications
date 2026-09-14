using SchoolAccount.CollectionNotifications.Models;
using SchoolAccount.CollectionNotifications.Models.Dtos;

namespace SchoolAccount.CollectionNotifications.Interfaces;

public interface IEnrollmentStore
{
    Task<Result<List<EnrolledRecipient>>> ListAsync(CancellationToken cancellationToken = default);
}
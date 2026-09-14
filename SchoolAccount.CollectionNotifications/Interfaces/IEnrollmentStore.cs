using SchoolAccount.CollectionNotifications.Models.Dtos;

namespace SchoolAccount.CollectionNotifications.Interfaces;

public interface IEnrollmentStore
{
    Task<List<EnrolledRecipient>> ReadAsync(CancellationToken cancellationToken = default);
}
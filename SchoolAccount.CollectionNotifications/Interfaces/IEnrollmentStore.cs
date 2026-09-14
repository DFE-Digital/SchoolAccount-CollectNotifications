using SchoolAccount.CollectionNotifications.Models.Dtos;

namespace SchoolAccount.CollectionNotifications.Interfaces;

public interface IEnrollmentStore
{
    Task<List<EnrolledRecipient>> ListAsync(CancellationToken cancellationToken = default);
}
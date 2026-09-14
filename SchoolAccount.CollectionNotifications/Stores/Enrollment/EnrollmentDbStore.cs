using Dapper;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectionNotifications.Interfaces;
using SchoolAccount.CollectionNotifications.Models;
using SchoolAccount.CollectionNotifications.Models.Databases;
using SchoolAccount.CollectionNotifications.Models.Dtos;
using SchoolAccount.CollectionNotifications.Models.Options;

namespace SchoolAccount.CollectionNotifications.Stores.Enrollment;

public class EnrollmentDbStore(
    IDbConnectionFactory<EnrollmentDatabase> factory,
    IOptions<EnrollmentDbOptions> options
) : IEnrollmentStore
{
    public async Task<Result<List<EnrolledRecipient>>> ListAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
        
        var sql = """
                  SELECT * FROM Recipients
                  """;
        
        await using var conn = await factory.OpenAsync(cancellationToken);
        var records = (await conn.QueryAsync<EnrolledRecipient>(sql, cancellationToken)).ToList();
        return Result.Success(records);
    }
}
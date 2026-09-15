using Dapper;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Databases;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Stores.Enrollment;

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
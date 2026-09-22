using Dapper;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Databases;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Services;
using SchoolAccount.CollectNotifications.Tests.Integration.Helpers;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Stores;

/// <summary>
/// The last run time is a row in the ledger's JobStatus table, so this is exercised against a real
/// database rather than a mocked store.
/// </summary>
public class LastRanServiceIntegrationTests : IAsyncLifetime
{
    private readonly string _jobName = $"test-{Guid.NewGuid():N}";
    private readonly DbConnectionFactory<LedgerDatabase> _connectionFactory = new(TestDatabaseHelper.ConnectionString);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var conn = await TestDatabaseHelper.OpenConnectionAsync();
        await conn.ExecuteAsync("DELETE FROM JobStatus WHERE Name = @Name;", new { Name = _jobName });
        await _connectionFactory.DisposeAsync();
    }

    private LastRanService CreateService() =>
        new(_connectionFactory, Options.Create(new CensusOptions { JobName = _jobName }));

    [Fact]
    public async Task When_the_job_has_never_run_it_should_return_the_minimum_sql_date()
    {
        // Arrange
        var sut = CreateService();

        // Act
        var result = await sut.GetTimestampAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));
    }

    [Fact]
    public async Task When_setting_the_timestamp_for_the_first_time_it_should_insert_the_row()
    {
        // Arrange
        var sut = CreateService();
        var ranAt = new DateTime(2026, 9, 21, 22, 30, 0, DateTimeKind.Utc);

        // Act
        var saved = await sut.SetTimestampAsync(ranAt);
        var readBack = await sut.GetTimestampAsync();

        // Assert
        saved.IsSuccess.ShouldBeTrue();
        readBack.Value.ShouldBe(ranAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task When_setting_the_timestamp_again_it_should_update_rather_than_add_a_second_row()
    {
        // Arrange
        var sut = CreateService();
        var firstRun = new DateTime(2026, 9, 20, 22, 30, 0, DateTimeKind.Utc);
        var secondRun = new DateTime(2026, 9, 21, 22, 30, 0, DateTimeKind.Utc);

        // Act
        await sut.SetTimestampAsync(firstRun);
        await sut.SetTimestampAsync(secondRun);

        // Assert
        var readBack = await sut.GetTimestampAsync();
        readBack.Value.ShouldBe(secondRun, TimeSpan.FromSeconds(1));

        await using var conn = await TestDatabaseHelper.OpenConnectionAsync();
        var rows = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM JobStatus WHERE Name = @Name;", new { Name = _jobName });
        rows.ShouldBe(1);
    }
}

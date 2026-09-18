using System.Data.SqlTypes;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Services;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Services;

public class LastRanServiceTests
{
    private const string TestBlobName = "custom/collect/lastran.json";
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IOptions<CensusOptions> _censusOptions = Options.Create(new CensusOptions
    {
        LastRunBlobName = TestBlobName
    });
    private readonly LastRanService _sut;

    public LastRanServiceTests()
    {
        _sut = new LastRanService(_blobStorageService, _censusOptions);
    }

    [Fact]
    public async Task Getting_a_timestamp_should_return_minimum_sql_date_time_when_blob_does_not_exist()
    {
        // Arrange
        _blobStorageService
            .GetAsync<LastRanService.LastRanBlobObject>(TestBlobName, Arg.Any<CancellationToken>())
            .Returns(Result.Success<LastRanService.LastRanBlobObject?>(null));

        // Act
        var result = await _sut.GetTimestampAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe((DateTime)SqlDateTime.MinValue);
    }

    [Fact]
    public async Task Getting_a_timestamp_should_return_correct_date_time_when_blob_contains_valid_oa_date()
    {
        // Arrange
        var expectedDateTime = new DateTime(2026, 9, 15, 12, 30, 0, DateTimeKind.Utc);
        var blobObject = new LastRanService.LastRanBlobObject(expectedDateTime.ToOADate());

        _blobStorageService
            .GetAsync<LastRanService.LastRanBlobObject>(TestBlobName, Arg.Any<CancellationToken>())
            .Returns(Result.Success<LastRanService.LastRanBlobObject?>(blobObject));

        // Act
        var result = await _sut.GetTimestampAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedDateTime);
    }

    [Fact]
    public async Task Getting_a_timestamp_should_return_failure_when_blob_storage_retrieval_fails()
    {
        // Arrange
        _blobStorageService
            .GetAsync<LastRanService.LastRanBlobObject>(TestBlobName, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<LastRanService.LastRanBlobObject?>("Storage account unreachable"));

        // Act
        var result = await _sut.GetTimestampAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Storage account unreachable");
    }

    [Fact]
    public async Task Updating_the_timestamp_should_convert_timestamp_to_oa_date_and_save_to_expected_blob_path()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);
        var expectedOaDate = timestamp.ToOADate();

        _blobStorageService
            .SaveAsync(
                TestBlobName,
                Arg.Is<LastRanService.LastRanBlobObject>(x => Math.Abs(x.RunDate - expectedOaDate) < 0.00001),
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _sut.SetTimestampAsync(timestamp);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _blobStorageService
            .Received(1)
            .SaveAsync(
                TestBlobName,
                Arg.Is<LastRanService.LastRanBlobObject>(x => Math.Abs(x.RunDate - expectedOaDate) < 0.00001),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Updating_the_timestamp_should_return_failure_when_blob_storage_save_fails()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);

        _blobStorageService
            .SaveAsync(
                TestBlobName,
                Arg.Any<LastRanService.LastRanBlobObject>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Write permissions denied"));

        // Act
        var result = await _sut.SetTimestampAsync(timestamp);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Write permissions denied");
    }
}

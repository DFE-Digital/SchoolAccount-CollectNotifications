using System.Data.SqlTypes;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Services;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Services;

public class LastRanServiceTests
{
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly LastRanService _sut;

    public LastRanServiceTests()
    {
        _sut = new LastRanService(_blobStorageService);
    }

    [Fact]
    public async Task GetTimestampAsync_should_return_minimum_sql_date_time_when_blob_does_not_exist()
    {
        // Arrange
        _blobStorageService
            .GetAsync<LastRanService.LastRanBlobObject>("schoolaccount/collect/lastran.json", Arg.Any<CancellationToken>())
            .Returns(Result.Success<LastRanService.LastRanBlobObject?>(null));

        // Act
        var result = await _sut.GetTimestampAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe((DateTime)SqlDateTime.MinValue);
    }

    [Fact]
    public async Task GetTimestampAsync_should_return_correct_date_time_when_blob_contains_valid_oa_date()
    {
        // Arrange
        var expectedDateTime = new DateTime(2026, 9, 15, 12, 30, 0, DateTimeKind.Utc);
        var blobObject = new LastRanService.LastRanBlobObject(expectedDateTime.ToOADate());

        _blobStorageService
            .GetAsync<LastRanService.LastRanBlobObject>("schoolaccount/collect/lastran.json", Arg.Any<CancellationToken>())
            .Returns(Result.Success<LastRanService.LastRanBlobObject?>(blobObject));

        // Act
        var result = await _sut.GetTimestampAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedDateTime);
    }

    [Fact]
    public async Task GetTimestampAsync_should_return_failure_when_blob_storage_retrieval_fails()
    {
        // Arrange
        _blobStorageService
            .GetAsync<LastRanService.LastRanBlobObject>("schoolaccount/collect/lastran.json", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<LastRanService.LastRanBlobObject?>("Storage account unreachable"));

        // Act
        var result = await _sut.GetTimestampAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Storage account unreachable");
    }

    [Fact]
    public async Task SetTimestampAsync_should_convert_timestamp_to_oa_date_and_save_to_expected_blob_path()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);
        var expectedOaDate = timestamp.ToOADate();

        _blobStorageService
            .SaveAsync(
                "schoolaccount/collect/lastran.json",
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
                "schoolaccount/collect/lastran.json",
                Arg.Is<LastRanService.LastRanBlobObject>(x => Math.Abs(x.RunDate - expectedOaDate) < 0.00001),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetTimestampAsync_should_return_failure_when_blob_storage_save_fails()
    {
        // Arrange
        var timestamp = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);

        _blobStorageService
            .SaveAsync(
                "schoolaccount/collect/lastran.json",
                Arg.Any<LastRanService.LastRanBlobObject>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Write permissions denied"));

        // Act
        var result = await _sut.SetTimestampAsync(timestamp);

        // Arrange
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Write permissions denied");
    }
}

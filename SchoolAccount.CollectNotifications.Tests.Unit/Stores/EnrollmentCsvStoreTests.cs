using Microsoft.Extensions.Options;
using MiniExcelLibs;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Stores.Enrollment;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Stores;

public class EnrollmentCsvStoreTests : IDisposable
{
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly List<string> _tempFiles = [];

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }

    [Fact]
    public async Task Should_throw_application_exception_when_neither_file_path_nor_blob_name_is_configured()
    {
        // Arrange
        var options = Options.Create(new EnrollmentCsvOptions
        {
            FilePath = "",
            BlobName = ""
        });
        var sut = new EnrollmentCsvStore(options, _blobStorageService);

        // Act & Assert
        var ex = await Should.ThrowAsync<ApplicationException>(async () => await sut.ListAsync());
        ex.Message.ShouldBe("Enrollment CSV not initialised correctly");
    }

    [Fact]
    public async Task Should_read_and_filter_valid_recipients_from_local_file_path()
    {
        // Arrange
        var rows = new object[]
        {
            new { LaeStab = "1111111", Email = "valid1@school.sch.uk" },
            new { LaeStab = "", Email = "invalid_no_laestab@school.sch.uk" },
            new { LaeStab = "2222222", Email = "" },
            new { LaeStab = "3333333", Email = "valid2@school.sch.uk" }
        };
        var filePath = CreateTestExcelFile(rows);

        var options = Options.Create(new EnrollmentCsvOptions
        {
            FilePath = filePath
        });
        var sut = new EnrollmentCsvStore(options, _blobStorageService);

        // Act
        var result = await sut.ListAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldContain(x => x.LaeStab == "1111111" && x.Email == "valid1@school.sch.uk");
        result.Value.ShouldContain(x => x.LaeStab == "3333333" && x.Email == "valid2@school.sch.uk");
    }

    [Fact]
    public async Task Should_read_and_filter_valid_recipients_from_blob_store()
    {
        // Arrange
        const string blobName = "enrollments/recipients.xlsx";
        var rows = new object[]
        {
            new { LaeStab = "4444444", Email = "blob1@school.sch.uk" },
            new { LaeStab = "5555555", Email = "blob2@school.sch.uk" },
            new { LaeStab = "6666666", Email = (string?)null }
        };
        var stream = CreateTestExcelStream(rows);

        _blobStorageService
            .GetFileAsync(blobName, Arg.Any<CancellationToken>())
            .Returns(Result.Success<Stream?>(stream));

        var options = Options.Create(new EnrollmentCsvOptions
        {
            FilePath = null,
            BlobName = blobName
        });
        var sut = new EnrollmentCsvStore(options, _blobStorageService);

        // Act
        var result = await sut.ListAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldContain(x => x.LaeStab == "4444444" && x.Email == "blob1@school.sch.uk");
        result.Value.ShouldContain(x => x.LaeStab == "5555555" && x.Email == "blob2@school.sch.uk");
    }

    [Fact]
    public async Task Should_return_failure_when_blob_retrieval_fails()
    {
        // Arrange
        const string blobName = "enrollments/recipients.xlsx";
        _blobStorageService
            .GetFileAsync(blobName, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Stream?>("Blob does not exist"));

        var options = Options.Create(new EnrollmentCsvOptions
        {
            FilePath = "",
            BlobName = blobName
        });
        var sut = new EnrollmentCsvStore(options, _blobStorageService);

        // Act
        var result = await sut.ListAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Blob does not exist");
    }

    [Fact]
    public async Task Should_return_failure_when_blob_stream_is_null()
    {
        // Arrange
        const string blobName = "enrollments/recipients.xlsx";
        _blobStorageService
            .GetFileAsync(blobName, Arg.Any<CancellationToken>())
            .Returns(Result.Success<Stream?>(null));

        var options = Options.Create(new EnrollmentCsvOptions
        {
            FilePath = "",
            BlobName = blobName
        });
        var sut = new EnrollmentCsvStore(options, _blobStorageService);

        // Act
        var result = await sut.ListAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    private static MemoryStream CreateTestExcelStream(IEnumerable<object> rows)
    {
        var stream = new MemoryStream();
        MiniExcel.SaveAs(stream, rows);
        stream.Position = 0;
        return stream;
    }

    private string CreateTestExcelFile(IEnumerable<object> rows)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");
        MiniExcel.SaveAs(tempPath, rows);
        _tempFiles.Add(tempPath);
        return tempPath;
    }
}

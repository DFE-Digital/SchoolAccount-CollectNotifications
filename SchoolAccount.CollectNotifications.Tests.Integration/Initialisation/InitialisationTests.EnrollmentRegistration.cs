using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models.Databases;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Stores.Enrollment;

namespace SchoolAccount.CollectNotifications.Tests.Integration.Initialisation;

public partial class InitialisationTests
{
    [Fact]
    public void ConfigureService_should_register_enrollment_db_store_when_database_connection_string_is_configured()
    {
        const string testConnectionString =
            "Server=localhost;Database=EnrollmentDb;User Id=sa;Password=test;TrustServerCertificate=true";
        
        // Arrange
        using var host = CreateHost(new Dictionary<string, string?>
        {
            ["Enrollment:Db:ConnectionString"] =testConnectionString
        });

        // Act
        using var scope = host.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var enrollmentStore = sp.GetRequiredService<IEnrollmentStore>();
        var enrollmentDbFactory = sp.GetRequiredService<IDbConnectionFactory<EnrollmentDatabase>>();
        var dbOptions = sp.GetRequiredService<IOptions<EnrollmentDbOptions>>().Value;
        
        // Assert
        enrollmentStore.ShouldBeOfType<EnrollmentDbStore>();
        enrollmentDbFactory.ShouldNotBeNull();
        dbOptions.ConnectionString.ShouldBeEquivalentTo(testConnectionString);
    }

    [Fact]
    public void ConfigureService_should_register_enrollment_csv_store_and_bind_options_when_csv_file_path_is_configured()
    {
        const string testFilePath = "/custom/path/recipients.xlsx";
        const string testSheetName = "Sheet1";
        const string testStartCell = "B2";
        
        // Arrange
        using var host = CreateHost(new Dictionary<string, string?>
        {
            ["Enrollment:Csv:FilePath"] = testFilePath,
            ["Enrollment:Csv:SheetName"] = testSheetName,
            ["Enrollment:Csv:StartCell"] = testStartCell
        });

        // Act
        using var scope = host.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var enrollmentStore = sp.GetRequiredService<IEnrollmentStore>();
        var csvOptions = sp.GetRequiredService<IOptions<EnrollmentCsvOptions>>().Value;
        
        // Assert
        enrollmentStore.ShouldBeOfType<EnrollmentCsvStore>();

        csvOptions.ShouldSatisfyAllConditions(
            x => x.FilePath.ShouldBe(testFilePath),
            x => x.SheetName.ShouldBe(testSheetName),
            x => x.StartCell.ShouldBe(testStartCell)
        );
    }
}

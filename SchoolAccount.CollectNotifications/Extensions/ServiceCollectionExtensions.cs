using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Databases;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Services.BlobStorage;
using SchoolAccount.CollectNotifications.Stores.Enrollment;

namespace SchoolAccount.CollectNotifications.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase<TDb>(this IServiceCollection services, Func<string> connectionStringFactory)
    {
        var connectionString = connectionStringFactory();
        return services.AddSingleton<IDbConnectionFactory<TDb>>(_ => new DbConnectionFactory<TDb>(connectionString));
    }

    public static IServiceCollection AddDatabase<TDb>(this IServiceCollection services, IConfiguration configuration)
    {
        var identifier = typeof(TDb).Name;
        var factory = () => configuration.GetConnectionString(identifier)
                            ?? throw new ArgumentException($"Connection string for {identifier} was not found.");
        return services.AddDatabase<TDb>(factory);
    }

    public static IServiceCollection AddEnrollmentStores(this IServiceCollection services, IConfiguration configuration)
    {
        var dbSection = configuration.GetSection(EnrollmentDbOptions.SectionName);
        var useDb = !string.IsNullOrWhiteSpace(dbSection[nameof(EnrollmentDbOptions.ConnectionString)]);

        if (useDb)
        {
            services.AddOptions<EnrollmentDbOptions>()
                .Bind(dbSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddDatabase<EnrollmentDatabase>(() => dbSection[nameof(EnrollmentDbOptions.ConnectionString)]!);
            services.AddSingleton<EnrollmentDbStore>();
            services.AddSingleton<IEnrollmentStore>(sp => sp.GetRequiredService<EnrollmentDbStore>());
        }
        else
        {
            services.AddOptions<EnrollmentCsvOptions>()
                .Bind(configuration.GetSection(EnrollmentCsvOptions.SectionName))
                .Validate(x => !string.IsNullOrEmpty(x.FilePath) || !string.IsNullOrWhiteSpace(x.BlobName),
                    $"Either properties \"{nameof(EnrollmentCsvOptions.FilePath)}\" or \"{nameof(EnrollmentCsvOptions.BlobName)}\" must be set.")
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<EnrollmentCsvStore>();
            services.AddSingleton<IEnrollmentStore>(sp => sp.GetRequiredService<EnrollmentCsvStore>());
        }

        return services;
    }
    
    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AzureBlobStorageOptions.SectionName);
        var options = section.Get<AzureBlobStorageOptions>() ?? new AzureBlobStorageOptions();

        if (!options.IsConfigured)
        {
            services.AddSingleton<IBlobStorageService, BlankedBlobStorageService>();
            return services;
        }

        services.AddOptions<AzureBlobStorageOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(CreateBlobServiceClient);
        services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

        return services;
    }

    internal static BlobServiceClient CreateBlobServiceClient(IServiceProvider sp)
    {
        var storageOptions = sp.GetRequiredService<IOptions<AzureBlobStorageOptions>>().Value;
        return CreateBlobServiceClient(storageOptions);
    }

    internal static BlobServiceClient CreateBlobServiceClient(AzureBlobStorageOptions storageOptions)
    {
        if (!string.IsNullOrWhiteSpace(storageOptions.ConnectionString))
        {
            return new BlobServiceClient(storageOptions.ConnectionString);
        }

        if (string.IsNullOrWhiteSpace(storageOptions.ServiceUri))
        {
            throw new InvalidOperationException(
                $"Set either {AzureBlobStorageOptions.SectionName}:ConnectionString or " +
                $"{AzureBlobStorageOptions.SectionName}:ServiceUri.");
        }

        return new BlobServiceClient(new Uri(storageOptions.ServiceUri), new DefaultAzureCredential());
    }
}

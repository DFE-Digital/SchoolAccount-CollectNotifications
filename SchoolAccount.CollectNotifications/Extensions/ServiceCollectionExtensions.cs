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

    public static IServiceCollection AddDatabase<TDb>(this IServiceCollection services, string identifier,
        IConfiguration configuration)
    {
        var factory = () => configuration.GetConnectionString(identifier)
                            ?? throw new ArgumentException($"Connection string for {identifier} was not found.");
        return services.AddDatabase<TDb>(factory);
    }

    public static IServiceCollection AddDatabase<TDb>(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddDatabase<TDb>(typeof(TDb).Name, configuration);
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
        }
        else
        {
            services.AddOptions<EnrollmentCsvOptions>()
                .Bind(configuration.GetSection(EnrollmentCsvOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<EnrollmentCsvStore>();
        }

        services.AddSingleton<IEnrollmentStore>(sp =>
        {
            var db = sp.GetRequiredService<IOptions<EnrollmentDbOptions>>().Value;
            return !string.IsNullOrWhiteSpace(db.ConnectionString)
                ? sp.GetRequiredService<EnrollmentDbStore>()
                : sp.GetRequiredService<EnrollmentCsvStore>();
        });

        return services;
    }
    
    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AzureBlobStorageOptions.SectionName);
        var options = section.Get<AzureBlobStorageOptions>() ?? new AzureBlobStorageOptions();

        if (options.IsConfigured)
        {
            services.AddOptions<AzureBlobStorageOptions>()
                .Bind(section)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            
            services.AddSingleton(sp =>
            {
                var storageOptions = sp.GetRequiredService<IOptions<AzureBlobStorageOptions>>().Value;

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
            });
 
            services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddSingleton<IBlobStorageService, BlankedBlobStorageService>();
        }
 
        return services;
    }
}
using Microsoft.Extensions.Hosting;
using SchoolAccount.CollectionNotifications.Extensions;
using SchoolAccount.CollectionNotifications.Models.Databases;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    services.AddEnrollmentStores(hostContext.Configuration);
    services.AddDatabase<LedgerDatabase>(hostContext.Configuration);
});
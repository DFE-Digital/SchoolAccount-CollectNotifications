using System.ComponentModel.DataAnnotations;
using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.Models.Options;

public sealed class CensusOptions
{
    public const string SectionName = "Census";

    /// <summary>
    /// The statuses that make a change notifiable, at either end of the transition. An empty list
    /// means nothing ever qualifies and every run sends nothing, so it has to be set.
    /// </summary>
    [Required, MinLength(1)]
    public List<ReturnStatusCodes> AllowedStatuses { get; init; } = [];

    /// <summary>
    /// Identifies this job's row in the ledger's JobStatus table, where the last run time lives.
    /// </summary>
    [Required, MinLength(1)]
    public string JobName { get; init; } = string.Empty;

    /// <summary>
    /// The collection to watch, matching the Collection column the ledger procedure writes,
    /// for example SchoolCensus2025_Spring. The ledger holds every collection, so without this
    /// a school's rows from different censuses rank against each other.
    /// </summary>
    [Required, MinLength(1)]
    public string Collection { get; init; } = string.Empty;
    
    /// <summary>
    /// Db Connection string
    /// </summary>
    [Required, MinLength(1)]
    public string ConnectionString { get; init; } = string.Empty;
}

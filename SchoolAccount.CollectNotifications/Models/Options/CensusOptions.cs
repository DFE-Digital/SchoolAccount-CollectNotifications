using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.Models.Options;

public sealed class CensusOptions
{
    public const string SectionName = "Census";

    public List<ReturnStatusCodes> AllowedStatuses { get; init; } = [];
    /// <summary>
    /// Identifies this job's row in the ledger's JobStatus table, where the last run time lives.
    /// </summary>
    public string JobName { get; init; } = string.Empty;

    /// <summary>
    /// The collection to watch, matching the Collection column the ledger procedure writes,
    /// for example SchoolCensus2025_Spring. The ledger holds every collection, so without this
    /// a school's rows from different censuses rank against each other.
    /// </summary>
    public string Collection { get; init; } = string.Empty;
}

using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.Models.Options;

public class CensusOptions
{
    public const string SectionName = "Census";

    public List<ReturnStatusCodes> AllowedStatuses { get; init; } = [];

}
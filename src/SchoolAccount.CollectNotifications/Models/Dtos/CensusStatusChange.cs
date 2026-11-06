using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.Models.Dtos;

/// <summary>
/// One notifiable status change, already paired with a registered recipient. A school with two
/// registered contacts produces two of these for the same change.
/// </summary>
public sealed class CensusStatusChange
{
    public required string SchoolName { get; init; }

    public required string LaeStab { get; init; }

    public required string Email { get; init; }

    public required ReturnStatusCodes ReturnStatusCode { get; init; }

    public ReturnStatusCodes? PreviousReturnStatusCode { get; init; }

    public required DateTime UpdatedAt { get; init; }

    public required string Collection { get; init; }

    public required int DcId { get; init; }
}

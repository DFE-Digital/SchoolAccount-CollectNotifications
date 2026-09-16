using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.Models;

public sealed class ComparableCollectReturnStatus : CollectReturnStatus
{
    public ReturnStatusCodes? PreviousReturnStatusCode { get; init; }
    
    public ReturnStatusCodes? InitialReturnStatusCode { get; init; }
}
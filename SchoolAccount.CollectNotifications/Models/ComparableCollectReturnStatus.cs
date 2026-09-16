using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.Models;

public class ComparableCollectReturnStatus : CollectReturnStatus
{
    public ReturnStatusCodes? FirstReturnStatusCode { get; init; }
    
    public ReturnStatusCodes? BaselineReturnStatusCode { get; init; }
}
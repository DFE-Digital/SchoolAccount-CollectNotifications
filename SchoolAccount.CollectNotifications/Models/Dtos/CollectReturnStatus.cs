using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.Models.Dtos;

public class CollectReturnStatus
{
    public required int Id { get; init; }
    
    public required string SchoolName { get; init; }
    
    public required string LaeStab { get; init; }
    
    public required ReturnStatusCodes ReturnStatusCode { get; init; }
    
    public required int Errors { get; init; }
    
    public required int Queries { get; init; }
    
    public required int OkdErrorsQueries { get; init; }
    
    public required string Hash { get; init; }
    
    public required DateTime UpdatedAt { get; init; }
    
    public required int DcId { get; init; }
    
    public required string Collection { get; init; }
    
    public required int DataReturnId { get; init; }
}
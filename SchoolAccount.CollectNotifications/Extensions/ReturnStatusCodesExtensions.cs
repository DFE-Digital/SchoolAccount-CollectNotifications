using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.Extensions;

public static class ReturnStatusCodesExtensions
{
    private static readonly Dictionary<int, string> StatusDescriptions = new()
    {
        { 1, "No data" },
        { 23, "Awaiting file upload" },
        { 24, "File upload in progress" },
        { 31, "File uploaded" },
        { 32, "File upload failed" },
        { 13, "Waiting for validation" },
        { 12, "Validation in progress" },
        { 11, "Uploaded validation failed" },
        { 2, "Loaded and validated" },
        { 3, "Amended by source" },
        { 14, "Awaiting submission" },
        { 17, "Submission in progress" },
        { 4, "Submitted" },
        { 5, "Rejected" },
        { 6, "Amended by agent" },
        { 15, "Awaiting approval" },
        { 18, "Approval in progress" },
        { 7, "Approved" },
        { 8, "Rejected by collector" },
        { 9, "Amended by collector" },
        { 16, "Awaiting authorisation" },
        { 19, "Authorisation in progress" },
        { 10, "Authorised" },
        { 25, "Awaiting matching" },
        { 26, "Matching in progress" },
        { 27, "Awaiting reconciliation" },
        { 28, "Reconciliation in progress" },
        { 29, "Matching failed" },
        { 30, "Reconciliation failed" },
    };

    public static string GetHumanName(this ReturnStatusCodes status)
    {
        return StatusDescriptions.GetValueOrDefault((int)status, "Not started"); 
    }
}
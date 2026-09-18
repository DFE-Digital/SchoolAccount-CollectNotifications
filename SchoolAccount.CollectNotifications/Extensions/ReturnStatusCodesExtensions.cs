using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.Extensions;

public static class ReturnStatusCodesExtensions
{
    private static readonly Dictionary<ReturnStatusCodes, string> StatusDescriptions = new()
    {
        { ReturnStatusCodes.NoData, "No data" },
        { ReturnStatusCodes.AwaitingFileUpload, "Awaiting file upload" },
        { ReturnStatusCodes.FileUploadInProgress, "File upload in progress" },
        { ReturnStatusCodes.FileUploaded, "File uploaded" },
        { ReturnStatusCodes.FileUploadFailed, "File upload failed" },
        { ReturnStatusCodes.WaitingForValidation, "Waiting for validation" },
        { ReturnStatusCodes.ValidationInProgress, "Validation in progress" },
        { ReturnStatusCodes.UploadedValidationFailed, "Uploaded validation failed" },
        { ReturnStatusCodes.LoadedAndValidated, "Loaded and validated" },
        { ReturnStatusCodes.AmendedBySource, "Amended by source" },
        { ReturnStatusCodes.AwaitingSubmission, "Awaiting submission" },
        { ReturnStatusCodes.SubmissionInProgress, "Submission in progress" },
        { ReturnStatusCodes.Submitted, "Submitted" },
        { ReturnStatusCodes.Rejected, "Rejected" },
        { ReturnStatusCodes.AmendedByAgent, "Amended by agent" },
        { ReturnStatusCodes.AwaitingApproval, "Awaiting approval" },
        { ReturnStatusCodes.ApprovalInProgress, "Approval in progress" },
        { ReturnStatusCodes.Approved, "Approved" },
        { ReturnStatusCodes.RejectedByCollector, "Rejected by collector" },
        { ReturnStatusCodes.AmendedByCollector, "Amended by collector" },
        { ReturnStatusCodes.AwaitingAuthorisation, "Awaiting authorisation" },
        { ReturnStatusCodes.AuthorisationInProgress, "Authorisation in progress" },
        { ReturnStatusCodes.Authorised, "Authorised" },
        { ReturnStatusCodes.AwaitingMatching, "Awaiting matching" },
        { ReturnStatusCodes.MatchingInProgress, "Matching in progress" },
        { ReturnStatusCodes.AwaitingReconciliation, "Awaiting reconciliation" },
        { ReturnStatusCodes.ReconciliationInProgress, "Reconciliation in progress" },
        { ReturnStatusCodes.MatchingFailed, "Matching failed" },
        { ReturnStatusCodes.ReconciliationFailed, "Reconciliation failed" },
    };
    
    public static string GetHumanName(this ReturnStatusCodes status)
    {
        return StatusDescriptions.GetValueOrDefault(status, "Unavailable"); 
    }
}
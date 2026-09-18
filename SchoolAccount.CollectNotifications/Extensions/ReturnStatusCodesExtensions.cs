using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.Extensions;

public static class ReturnStatusCodesExtensions
{
    private static readonly Dictionary<ReturnStatusCodes, string> StatusDescriptions = new()
    {
        { ReturnStatusCodes.NoData, "No_Data" },
        { ReturnStatusCodes.AwaitingFileUpload, "Awaiting_File_Upload" },
        { ReturnStatusCodes.FileUploadInProgress, "File_Upload_in_Progress" },
        { ReturnStatusCodes.FileUploaded, "File_Uploaded" },
        { ReturnStatusCodes.FileUploadFailed, "File_Upload_Failed" },
        { ReturnStatusCodes.WaitingForValidation, "Waiting_for_validation" },
        { ReturnStatusCodes.ValidationInProgress, "Validation_in_progress" },
        { ReturnStatusCodes.UploadedValidationFailed, "Uploaded_Validation_Failed" },
        { ReturnStatusCodes.LoadedAndValidated, "Loaded_and_Validated" },
        { ReturnStatusCodes.AmendedBySource, "Amended_by_source" },
        { ReturnStatusCodes.AwaitingSubmission, "Awaiting_Submission" },
        { ReturnStatusCodes.SubmissionInProgress, "Submission_in_Progress" },
        { ReturnStatusCodes.Submitted, "Submitted" },
        { ReturnStatusCodes.Rejected, "Rejected" },
        { ReturnStatusCodes.AmendedByAgent, "Amended_by_agent" },
        { ReturnStatusCodes.AwaitingApproval, "Awaiting_Approval" },
        { ReturnStatusCodes.ApprovalInProgress, "Approval_in_Progress" },
        { ReturnStatusCodes.Approved, "Approved" },
        { ReturnStatusCodes.RejectedByCollector, "Rejected_by_collector" },
        { ReturnStatusCodes.AmendedByCollector, "Amended_by_collector" },
        { ReturnStatusCodes.AwaitingAuthorisation, "Awaiting_Authorisation" },
        { ReturnStatusCodes.AuthorisationInProgress, "Authorisation_In_Progress" },
        { ReturnStatusCodes.Authorised, "Authorised" },
        { ReturnStatusCodes.AwaitingMatching, "Awaiting_Matching" },
        { ReturnStatusCodes.MatchingInProgress, "Matching_in_Progress" },
        { ReturnStatusCodes.AwaitingReconciliation, "Awaiting_Reconciliation" },
        { ReturnStatusCodes.ReconciliationInProgress, "Reconciliation_in_Progress" },
        { ReturnStatusCodes.MatchingFailed, "Matching_Failed" },
        { ReturnStatusCodes.ReconciliationFailed, "Reconciliation_Failed" },
    };
    
    public static string GetHumanName(this ReturnStatusCodes status)
    {
        return StatusDescriptions.GetValueOrDefault(status, "Unavailable"); 
    }
}
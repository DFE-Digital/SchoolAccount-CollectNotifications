using SchoolAccount.CollectNotifications.Models.Enums;
using static SchoolAccount.CollectNotifications.Models.Enums.ReturnStatusCodes;
using Descriptor = SchoolAccount.CollectNotifications.Models.StatusDescriptors;

namespace SchoolAccount.CollectNotifications.Models;

public static class StatusWordings
{
    private static readonly StatusWording _unavailable = new("Unavailable");

    private static readonly Dictionary<ReturnStatusCodes, StatusWording> _wordings = new()
    {
        [AmendedByAgent] = new("Amended_by_agent", Descriptor.AmendedByAgent),
        [AmendedByCollector] = new("Amended_by_collector", Descriptor.AmendedByCollector),
        [AmendedBySource] = new("Amended_by_source", Descriptor.AmendedBySource),
        [ApprovalInProgress] = new("Approval_in_Progress"),
        [Approved] = new("Approved", Descriptor.Approved),
        [AuthorisationInProgress] = new("Authorisation_In_Progress"),
        [Authorised] = new("Authorised", Descriptor.Authorised),
        [AwaitingApproval] = new("Awaiting_Approval"),
        [AwaitingAuthorisation] = new("Awaiting_Authorisation"),
        [AwaitingFileUpload] = new("Awaiting_File_Upload"),
        [AwaitingMatching] = new("Awaiting_Matching"),
        [AwaitingReconciliation] = new("Awaiting_Reconciliation"),
        [AwaitingSubmission] = new("Awaiting_Submission"),
        [FileUploadFailed] = new("File_Upload_Failed"),
        [FileUploadInProgress] = new("File_Upload_in_Progress"),
        [FileUploaded] = new("File_Uploaded"),
        [LoadedAndValidated] = new("Loaded_and_Validated", Descriptor.LoadedAndValidated),
        [MatchingFailed] = new("Matching_Failed"),
        [MatchingInProgress] = new("Matching_in_Progress"),
        [NoData] = new("No_Data", Descriptor.NoData),
        [ReconciliationFailed] = new("Reconciliation_Failed"),
        [ReconciliationInProgress] = new("Reconciliation_in_Progress"),
        [Rejected] = new("Rejected"),
        [RejectedByCollector] = new("Rejected_by_collector"),
        [SubmissionInProgress] = new("Submission_in_Progress"),
        [Submitted] = new("Submitted", Descriptor.Submitted),
        [UploadedValidationFailed] = new("Uploaded_Validation_Failed"),
        [ValidationInProgress] = new("Validation_in_progress"),
        [WaitingForValidation] = new("Waiting_for_validation"),
    };

    public static StatusWording For(ReturnStatusCodes status) =>
        _wordings.GetValueOrDefault(status, _unavailable);
}

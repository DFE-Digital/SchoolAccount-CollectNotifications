using System.Diagnostics.CodeAnalysis;

namespace SchoolAccount.CollectNotifications.Models.Enums;

[SuppressMessage("Design", "CA1027:Mark enums with FlagsAttribute",
    Justification = "These are COLLECT's status codes. A return is in one state at a time, they are never combined.")]
public enum ReturnStatusCodes
{
    NoData = 1,
    AwaitingFileUpload = 23,
    FileUploadInProgress = 24,
    FileUploaded = 31,
    FileUploadFailed = 32,
    WaitingForValidation = 13,
    ValidationInProgress = 12,
    UploadedValidationFailed = 11,
    LoadedAndValidated = 2,
    AmendedBySource = 3,
    AwaitingSubmission = 14,
    SubmissionInProgress = 17,
    Submitted = 4,
    Rejected = 5,
    AmendedByAgent = 6,
    AwaitingApproval = 15,
    ApprovalInProgress = 18,
    Approved = 7,
    RejectedByCollector = 8,
    AmendedByCollector = 9,
    AwaitingAuthorisation = 16,
    AuthorisationInProgress = 19,
    Authorised = 10,
    AwaitingMatching = 25,
    MatchingInProgress = 26,
    AwaitingReconciliation = 27,
    ReconciliationInProgress = 28,
    MatchingFailed = 29,
    ReconciliationFailed = 30,
}

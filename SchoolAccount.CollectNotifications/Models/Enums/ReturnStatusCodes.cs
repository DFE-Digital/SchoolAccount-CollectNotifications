using System.ComponentModel.DataAnnotations;

namespace SchoolAccount.CollectNotifications.Models.Enums;

public enum ReturnStatusCodes
{
    [Display(Name = "No data")]
    NoData = 1,
    
    [Display(Name = "Awaiting file upload")]
    AwaitingFileUpload = 23,
    
    [Display(Name = "File upload in progress")]
    FileUploadInProgress = 24,
    
    [Display(Name = "File uploaded")]
    FileUploaded = 31,
    
    [Display(Name = "File upload failed")]
    FileUploadFailed = 32,
    
    [Display(Name = "Waiting for validation")]
    WaitingForValidation = 13,
    
    [Display(Name = "Validation in progress")]
    ValidationInProgress = 12,
    
    [Display(Name = "Uploaded validation failed")]
    UploadedValidationFailed = 11,
    
    [Display(Name = "Loaded and validated")]
    LoadedAndValidated = 2,
    
    [Display(Name = "Amended by source")]
    AmendedBySource = 3,
    
    [Display(Name = "Awaiting submission")]
    AwaitingSubmission = 14,
    
    [Display(Name = "Submission in progress")]
    SubmissionInProgress = 17,
    
    [Display(Name = "Submitted")]
    Submitted = 4,
    
    [Display(Name = "Rejected")]
    Rejected = 5,
    
    [Display(Name = "Amended By agent")]
    AmendedByAgent = 6,
    
    [Display(Name = "Awaiting approval")]
    AwaitingApproval = 15,
    
    [Display(Name = "Approval in progress")]
    ApprovalInProgress = 18,
    
    [Display(Name = "Approved")]
    Approved = 7,
    
    [Display(Name = "Rejected by collector")]
    RejectedByCollector = 8,
    
    [Display(Name = "Amended by collector")]
    AmendedByCollector = 9,
    
    [Display(Name = "Awaiting authorisation")]
    AwaitingAuthorisation = 16,
    
    [Display(Name = "Authorisation in progress")]
    AuthorisationInProgress = 19,
    
    [Display(Name = "Authorised")]
    Authorised = 10,
    
    [Display(Name = "Awaiting matching")]
    AwaitingMatching = 25,
    
    [Display(Name = "Matching in progress")]
    MatchingInProgress = 26,
    
    [Display(Name = "Awaiting reconciliation")]
    AwaitingReconciliation = 27,
    
    [Display(Name = "Reconciliation in progress")]
    ReconciliationInProgress = 28,
    
    [Display(Name = "Matching failed")]
    MatchingFailed = 29,
    
    [Display(Name = "Reconciliation failed")]
    ReconciliationFailed = 30
}
using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.UnitTests.Extensions;

public class ReturnStatusCodesExtensionsTests
{
    [Theory]
    [InlineData(ReturnStatusCodes.NoData, "No_Data")]
    [InlineData(ReturnStatusCodes.AwaitingFileUpload, "Awaiting_File_Upload")]
    [InlineData(ReturnStatusCodes.FileUploadInProgress, "File_Upload_in_Progress")]
    [InlineData(ReturnStatusCodes.FileUploaded, "File_Uploaded")]
    [InlineData(ReturnStatusCodes.FileUploadFailed, "File_Upload_Failed")]
    [InlineData(ReturnStatusCodes.WaitingForValidation, "Waiting_for_validation")]
    [InlineData(ReturnStatusCodes.ValidationInProgress, "Validation_in_progress")]
    [InlineData(ReturnStatusCodes.UploadedValidationFailed, "Uploaded_Validation_Failed")]
    [InlineData(ReturnStatusCodes.LoadedAndValidated, "Loaded_and_Validated")]
    [InlineData(ReturnStatusCodes.AmendedBySource, "Amended_by_source")]
    [InlineData(ReturnStatusCodes.AwaitingSubmission, "Awaiting_Submission")]
    [InlineData(ReturnStatusCodes.SubmissionInProgress, "Submission_in_Progress")]
    [InlineData(ReturnStatusCodes.Submitted, "Submitted")]
    [InlineData(ReturnStatusCodes.Rejected, "Rejected")]
    [InlineData(ReturnStatusCodes.AmendedByAgent, "Amended_by_agent")]
    [InlineData(ReturnStatusCodes.AwaitingApproval, "Awaiting_Approval")]
    [InlineData(ReturnStatusCodes.ApprovalInProgress, "Approval_in_Progress")]
    [InlineData(ReturnStatusCodes.Approved, "Approved")]
    [InlineData(ReturnStatusCodes.RejectedByCollector, "Rejected_by_collector")]
    [InlineData(ReturnStatusCodes.AmendedByCollector, "Amended_by_collector")]
    [InlineData(ReturnStatusCodes.AwaitingAuthorisation, "Awaiting_Authorisation")]
    [InlineData(ReturnStatusCodes.AuthorisationInProgress, "Authorisation_In_Progress")]
    [InlineData(ReturnStatusCodes.Authorised, "Authorised")]
    [InlineData(ReturnStatusCodes.AwaitingMatching, "Awaiting_Matching")]
    [InlineData(ReturnStatusCodes.MatchingInProgress, "Matching_in_Progress")]
    [InlineData(ReturnStatusCodes.AwaitingReconciliation, "Awaiting_Reconciliation")]
    [InlineData(ReturnStatusCodes.ReconciliationInProgress, "Reconciliation_in_Progress")]
    [InlineData(ReturnStatusCodes.MatchingFailed, "Matching_Failed")]
    [InlineData(ReturnStatusCodes.ReconciliationFailed, "Reconciliation_Failed")]
    public void Getting_name_should_return_correct_description_for_known_status(
        ReturnStatusCodes status,
        string expectedDescription
    )
    {
        // Act
        var result = status.GetHumanName();

        // Assert
        result.ShouldBe(expectedDescription);
    }

    [Fact]
    public void Getting_name_should_return_unavailable_for_unknown_status()
    {
        // Arrange
        const ReturnStatusCodes unknownStatus = (ReturnStatusCodes)9999;

        // Act
        var result = unknownStatus.GetHumanName();

        // Assert
        result.ShouldBe("Unavailable");
    }
}

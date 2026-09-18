using SchoolAccount.CollectNotifications.Extensions;
using SchoolAccount.CollectNotifications.Models.Enums;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Extensions;

public class ReturnStatusCodesExtensionsTests
{
    [Theory]
    [InlineData(ReturnStatusCodes.NoData, "No data")]
    [InlineData(ReturnStatusCodes.AwaitingFileUpload, "Awaiting file upload")]
    [InlineData(ReturnStatusCodes.FileUploadInProgress, "File upload in progress")]
    [InlineData(ReturnStatusCodes.FileUploaded, "File uploaded")]
    [InlineData(ReturnStatusCodes.FileUploadFailed, "File upload failed")]
    [InlineData(ReturnStatusCodes.WaitingForValidation, "Waiting for validation")]
    [InlineData(ReturnStatusCodes.ValidationInProgress, "Validation in progress")]
    [InlineData(ReturnStatusCodes.UploadedValidationFailed, "Uploaded validation failed")]
    [InlineData(ReturnStatusCodes.LoadedAndValidated, "Loaded and validated")]
    [InlineData(ReturnStatusCodes.AmendedBySource, "Amended by source")]
    [InlineData(ReturnStatusCodes.AwaitingSubmission, "Awaiting submission")]
    [InlineData(ReturnStatusCodes.SubmissionInProgress, "Submission in progress")]
    [InlineData(ReturnStatusCodes.Submitted, "Submitted")]
    [InlineData(ReturnStatusCodes.Rejected, "Rejected")]
    [InlineData(ReturnStatusCodes.AmendedByAgent, "Amended by agent")]
    [InlineData(ReturnStatusCodes.AwaitingApproval, "Awaiting approval")]
    [InlineData(ReturnStatusCodes.ApprovalInProgress, "Approval in progress")]
    [InlineData(ReturnStatusCodes.Approved, "Approved")]
    [InlineData(ReturnStatusCodes.RejectedByCollector, "Rejected by collector")]
    [InlineData(ReturnStatusCodes.AmendedByCollector, "Amended by collector")]
    [InlineData(ReturnStatusCodes.AwaitingAuthorisation, "Awaiting authorisation")]
    [InlineData(ReturnStatusCodes.AuthorisationInProgress, "Authorisation in progress")]
    [InlineData(ReturnStatusCodes.Authorised, "Authorised")]
    [InlineData(ReturnStatusCodes.AwaitingMatching, "Awaiting matching")]
    [InlineData(ReturnStatusCodes.MatchingInProgress, "Matching in progress")]
    [InlineData(ReturnStatusCodes.AwaitingReconciliation, "Awaiting reconciliation")]
    [InlineData(ReturnStatusCodes.ReconciliationInProgress, "Reconciliation in progress")]
    [InlineData(ReturnStatusCodes.MatchingFailed, "Matching failed")]
    [InlineData(ReturnStatusCodes.ReconciliationFailed, "Reconciliation failed")]
    public void GetHumanName_should_return_correct_description_for_known_status(ReturnStatusCodes status, string expectedDescription)
    {
        // Act
        var result = status.GetHumanName();

        // Assert
        result.ShouldBe(expectedDescription);
    }

    [Fact]
    public void GetHumanName_should_return_unavailable_for_unknown_status()
    {
        // Arrange
        const ReturnStatusCodes unknownStatus = (ReturnStatusCodes)9999;

        // Act
        var result = unknownStatus.GetHumanName();

        // Assert
        result.ShouldBe("Unavailable");
    }
}

using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Enums;
using static SchoolAccount.CollectNotifications.Models.Enums.ReturnStatusCodes;

namespace SchoolAccount.CollectNotifications.UnitTests.Models;

public class StatusWordingsTests
{
    [Theory]
    [InlineData(NoData, "No_Data")]
    [InlineData(AwaitingFileUpload, "Awaiting_File_Upload")]
    [InlineData(FileUploadInProgress, "File_Upload_in_Progress")]
    [InlineData(FileUploaded, "File_Uploaded")]
    [InlineData(FileUploadFailed, "File_Upload_Failed")]
    [InlineData(WaitingForValidation, "Waiting_for_validation")]
    [InlineData(ValidationInProgress, "Validation_in_progress")]
    [InlineData(UploadedValidationFailed, "Uploaded_Validation_Failed")]
    [InlineData(LoadedAndValidated, "Loaded_and_Validated")]
    [InlineData(AmendedBySource, "Amended_by_source")]
    [InlineData(AwaitingSubmission, "Awaiting_Submission")]
    [InlineData(SubmissionInProgress, "Submission_in_Progress")]
    [InlineData(Submitted, "Submitted")]
    [InlineData(Rejected, "Rejected")]
    [InlineData(AmendedByAgent, "Amended_by_agent")]
    [InlineData(AwaitingApproval, "Awaiting_Approval")]
    [InlineData(ApprovalInProgress, "Approval_in_Progress")]
    [InlineData(Approved, "Approved")]
    [InlineData(RejectedByCollector, "Rejected_by_collector")]
    [InlineData(AmendedByCollector, "Amended_by_collector")]
    [InlineData(AwaitingAuthorisation, "Awaiting_Authorisation")]
    [InlineData(AuthorisationInProgress, "Authorisation_In_Progress")]
    [InlineData(Authorised, "Authorised")]
    [InlineData(AwaitingMatching, "Awaiting_Matching")]
    [InlineData(MatchingInProgress, "Matching_in_Progress")]
    [InlineData(AwaitingReconciliation, "Awaiting_Reconciliation")]
    [InlineData(ReconciliationInProgress, "Reconciliation_in_Progress")]
    [InlineData(MatchingFailed, "Matching_Failed")]
    [InlineData(ReconciliationFailed, "Reconciliation_Failed")]
    public void Getting_wording_should_return_correct_name_for_known_status(
        ReturnStatusCodes status,
        string expectedName
    )
    {
        // Act
        var result = StatusWordings.For(status);

        // Assert
        result.Name.ShouldBe(expectedName);
    }

    [Theory]
    [InlineData(NoData, "Nothing has been uploaded to COLLECT yet. You need to upload your data.")]
    [InlineData(
        LoadedAndValidated,
        "Your data has been uploaded to COLLECT. You need to submit it."
    )]
    [InlineData(
        Submitted,
        "You have submitted your data. Your local authority or DfE will now check it."
    )]
    [InlineData(
        AmendedBySource,
        "Your data has been changed since you submitted it. You need to submit it again."
    )]
    [InlineData(
        Approved,
        "Your data has been approved and is waiting for DfE to authorise it. DfE will contact you if you need to do anything else."
    )]
    [InlineData(
        Authorised,
        "DfE has accepted your data. You do not need to do anything else unless DfE contacts you."
    )]
    public void Getting_wording_should_explain_what_the_status_means(
        ReturnStatusCodes status,
        string expectedDescriptor
    )
    {
        // Act
        var result = StatusWordings.For(status);

        // Assert
        result.Descriptor.ShouldBe(expectedDescriptor);
    }

    [Fact]
    public void Getting_wording_should_return_an_empty_descriptor_for_a_status_without_one()
    {
        // Act
        var result = StatusWordings.For(RejectedByCollector);

        // Assert
        result.Descriptor.ShouldBeEmpty();
    }

    [Fact]
    public void Getting_wording_should_return_unavailable_for_unknown_status()
    {
        // Arrange
        const ReturnStatusCodes unknownStatus = (ReturnStatusCodes)9999;

        // Act
        var result = StatusWordings.For(unknownStatus);

        // Assert
        result.Name.ShouldBe("Unavailable");
        result.Descriptor.ShouldBeEmpty();
    }

    [Fact]
    public void Every_status_should_have_wording()
    {
        foreach (var status in Enum.GetValues<ReturnStatusCodes>())
        {
            StatusWordings.For(status).Name.ShouldNotBe("Unavailable", $"{status} has no wording");
        }
    }
}

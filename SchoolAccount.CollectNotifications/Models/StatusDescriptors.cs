namespace SchoolAccount.CollectNotifications.Models;

/// <summary>
/// What a status means for the school, shown under "What this means" in the email.
/// </summary>
public static class StatusDescriptors
{
    public const string NoData =
        "Nothing has been uploaded to COLLECT yet. You need to upload your data.";

    public const string Loaded =
        "Your data has been uploaded to COLLECT and automatically checked. You need to resolve any "
        + "errors or queries then submit your return.";

    public const string Submitted =
        "You have submitted your data. Your local authority or DfE will now check it.";

    public const string AmendedBySource =
        "Your data has been changed since you submitted it. You need to submit it again.";

    public const string Approved =
        "Your data has been approved and is waiting for DfE to authorise it. DfE will contact you "
        + "if you need to do anything else.";

    public const string Authorised =
        "DfE has accepted your data. You do not need to do anything else unless DfE contacts you.";

    public const string AmendedByAgent =
        "Your local authority has made changes to your data. Contact your local authority if you "
        + "want to know what they changed.";

    public const string AmendedByCollector =
        "DfE has changed or rechecked your data. Check COLLECT for any notes, errors or queries "
        + "you need to resolve.";
}

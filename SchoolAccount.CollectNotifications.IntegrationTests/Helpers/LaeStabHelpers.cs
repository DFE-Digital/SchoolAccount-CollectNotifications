namespace SchoolAccount.CollectNotifications.Tests.Integration.Helpers;

public static class LaeStabHelpers
{
    /// <summary>
    /// Tests share the ledger with dev data and with each other, and clean up by deleting their own
    /// LAEStab, so a key that repeats means one test deleting another's rows. A four digit suffix only
    /// had 9,000 values, which is fine within a run but not across two running at once. LAEStab is
    /// nvarchar(50) and nothing validates its shape, so a guid costs nothing here and ensures uniqueness.
    /// </summary>
    public static string GenerateUnique(string prefix = "999")
    {
        return $"{prefix}{Guid.NewGuid():N}";
    }
}

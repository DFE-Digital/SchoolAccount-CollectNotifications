namespace SchoolAccount.CollectNotifications.Tests.Integration.Helpers;

public static class LaeStabHelpers
{
    public static string GenerateUnique(string prefix = "999")
    {
        return $"{prefix}{Random.Shared.Next(1000, 9999)}";
    }
}
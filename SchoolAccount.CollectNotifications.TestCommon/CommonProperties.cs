using System.Diagnostics.CodeAnalysis;

namespace SchoolAccount.CollectNotifications.TestCommon;

public static class CommonProperties
{
    public const string TemplateKey = "test-template-key";

    [SuppressMessage(
        "Major Code Smell",
        "S1075:URIs should not be hardcoded",
        Justification = "Test data. Nothing resolves it, it is here so the value looks like the one "
            + "the option is validated against."
    )]
    public const string SchoolAccountUrl = "https://school-account.test";
}

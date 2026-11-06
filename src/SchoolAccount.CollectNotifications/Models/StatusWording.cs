namespace SchoolAccount.CollectNotifications.Models;

/// <summary>
/// How a status reads in the email. Name fills ((status)), Descriptor fills
/// ((status_descriptor)) under "What this means".
/// </summary>
public sealed record StatusWording(string Name, string Descriptor = "");

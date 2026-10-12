namespace SchoolAccount.CollectNotifications.Models.Dtos;

public sealed record Notification(
    string LaeStab,
    string Recipient,
    string Status,
    string StatusDescriptor,
    string School
);

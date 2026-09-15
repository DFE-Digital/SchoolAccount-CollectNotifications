using SchoolAccount.CollectNotifications.Models.Dtos;

namespace SchoolAccount.CollectNotifications.Tests.Builders;

public class NotificationBuilder
{
    private string _laeStab = "1234567";
    private string _recipient = "head@school.sch.uk";
    private string _status = "10";
    private string _school = "Default Test School";

    public static NotificationBuilder ANotification()
    {
        return new NotificationBuilder();
    }

    public NotificationBuilder WithLaeStab(string laeStab)
    {
        _laeStab = laeStab;
        return this;
    }

    public NotificationBuilder WithRecipient(string recipient)
    {
        _recipient = recipient;
        return this;
    }

    public NotificationBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public NotificationBuilder WithSchool(string school)
    {
        _school = school;
        return this;
    }

    public Notification Build()
    {
        return new Notification(_laeStab, _recipient, _status, _school);
    }

    public static implicit operator Notification(NotificationBuilder builder)
    {
        return builder.Build();
    }
}

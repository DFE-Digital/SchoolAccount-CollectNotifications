using SchoolAccount.CollectNotifications.Models.Dtos;

namespace SchoolAccount.CollectNotifications.Tests.Builders;

public class EnrolledRecipientBuilder
{
    private string? _laeStab = "1234567";
    private string? _email = "user@school.sch.uk";

    public static EnrolledRecipientBuilder AnEnrolledRecipient()
    {
        return new EnrolledRecipientBuilder();
    }

    public EnrolledRecipientBuilder WithLaeStab(string? laeStab)
    {
        _laeStab = laeStab;
        return this;
    }

    public EnrolledRecipientBuilder WithEmail(string? email)
    {
        _email = email;
        return this;
    }

    public EnrolledRecipientBuilder WithoutEmail()
    {
        _email = null;
        return this;
    }

    public EnrolledRecipientBuilder WithoutLaeStab()
    {
        _laeStab = null;
        return this;
    }

    public EnrolledRecipient Build()
    {
        return new EnrolledRecipient
        {
            LaeStab = _laeStab,
            Email = _email
        };
    }

    public static implicit operator EnrolledRecipient(EnrolledRecipientBuilder builder)
    {
        return builder.Build();
    }
}

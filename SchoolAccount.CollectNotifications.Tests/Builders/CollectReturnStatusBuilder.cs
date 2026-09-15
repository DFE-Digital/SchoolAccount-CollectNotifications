using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Tests.Builders;

public class CollectReturnStatusBuilder
{
    private int _id = 1;
    private string _schoolName = "Default Test School";
    private string _laeStab = "1234567";
    private int _returnStatusCode = 10;
    private int _errors = 0;
    private int _queries = 0;
    private int _okdErrorsQueries = 0;
    private string _hash = "default-hash";
    private DateTime _updatedAt = DateTime.UtcNow;
    private int _dcId = 1;
    private string _collection = "Census";
    private int _dataReturnId = 1;

    public static CollectReturnStatusBuilder ACollectReturnStatus()
    {
        return new CollectReturnStatusBuilder();
    }

    public CollectReturnStatusBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public CollectReturnStatusBuilder WithSchoolName(string schoolName)
    {
        _schoolName = schoolName;
        return this;
    }

    public CollectReturnStatusBuilder WithLaeStab(string laeStab)
    {
        _laeStab = laeStab;
        return this;
    }

    public CollectReturnStatusBuilder WithReturnStatusCode(int returnStatusCode)
    {
        _returnStatusCode = returnStatusCode;
        return this;
    }

    public CollectReturnStatusBuilder WithErrors(int errors)
    {
        _errors = errors;
        return this;
    }

    public CollectReturnStatusBuilder WithQueries(int queries)
    {
        _queries = queries;
        return this;
    }

    public CollectReturnStatusBuilder WithOkdErrorsQueries(int okdErrorsQueries)
    {
        _okdErrorsQueries = okdErrorsQueries;
        return this;
    }

    public CollectReturnStatusBuilder WithHash(string hash)
    {
        _hash = hash;
        return this;
    }

    public CollectReturnStatusBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public CollectReturnStatusBuilder WithDcId(int dcId)
    {
        _dcId = dcId;
        return this;
    }

    public CollectReturnStatusBuilder WithCollection(string collection)
    {
        _collection = collection;
        return this;
    }

    public CollectReturnStatusBuilder WithDataReturnId(int dataReturnId)
    {
        _dataReturnId = dataReturnId;
        return this;
    }

    public CollectReturnStatus Build()
    {
        return new CollectReturnStatus
        {
            Id = _id,
            SchoolName = _schoolName,
            LaeStab = _laeStab,
            ReturnStatusCode = _returnStatusCode,
            Errors = _errors,
            Queries = _queries,
            OkdErrorsQueries = _okdErrorsQueries,
            Hash = _hash,
            UpdatedAt = _updatedAt,
            DcId = _dcId,
            Collection = _collection,
            DataReturnId = _dataReturnId
        };
    }

    public static implicit operator CollectReturnStatus(CollectReturnStatusBuilder builder)
    {
        return builder.Build();
    }
}

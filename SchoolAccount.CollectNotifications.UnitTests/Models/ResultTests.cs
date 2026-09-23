using SchoolAccount.CollectNotifications.Models;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Models;

public class ResultTests
{
    [Fact]
    public void Success_should_create_successful_result()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Success_with_value_should_create_successful_typed_result()
    {
        // Act
        var result = Result.Success(42);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe(42);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Warning_should_create_successful_result_with_warning_message()
    {
        // Act
        var result = Result.Warning("Warning message");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe("Warning message");
    }

    [Fact]
    public void Warning_with_value_should_create_successful_typed_result_with_warning_message()
    {
        // Act
        var result = Result.Warning("payload", "Warning message");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe("payload");
        result.Error.ShouldBe("Warning message");
    }

    [Fact]
    public void Failure_should_create_failed_result()
    {
        // Act
        var result = Result.Failure("Something went wrong");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Something went wrong");
    }

    [Fact]
    public void Failure_typed_should_create_failed_result_and_throw_on_value_access()
    {
        // Act
        var result = Result.Failure<string>("Error occurred");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Error occurred");

        var ex = Should.Throw<InvalidOperationException>(() => _ = result.Value);
        ex.Message.ShouldBe("The value of a failure result can't be accessed.");
    }
}

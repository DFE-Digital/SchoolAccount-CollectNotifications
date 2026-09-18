using Microsoft.Extensions.Logging.Abstractions;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Services;
using static SchoolAccount.CollectNotifications.Tests.Common.Builders.CollectReturnStatusBuilder;
using static SchoolAccount.CollectNotifications.Tests.Common.Builders.EnrolledRecipientBuilder;
using static SchoolAccount.CollectNotifications.Tests.Common.Builders.NotificationBuilder;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Services;

public class StatusChangedLedgerMonitoringServiceTests
{
    private readonly IEnrollmentStore _enrollmentStore = Substitute.For<IEnrollmentStore>();
    private readonly ILastRanService _lastRanService = Substitute.For<ILastRanService>();
    private readonly ILedgerStore _ledgerStore = Substitute.For<ILedgerStore>();
    private readonly IThreadingService _threadingService = Substitute.For<IThreadingService>();
    private readonly IGovNotifyService _govNotifyService = Substitute.For<IGovNotifyService>();
    private readonly StatusChangedLedgerMonitoringService _sut;

    public StatusChangedLedgerMonitoringServiceTests()
    {
        _sut = new StatusChangedLedgerMonitoringService(
            NullLogger<StatusChangedLedgerMonitoringService>.Instance,
            _enrollmentStore,
            _lastRanService,
            _ledgerStore,
            _threadingService,
            _govNotifyService);
    }

    [Fact]
    public async Task InvokeAsync_should_abort_workflow_when_retrieving_recipients_fails()
    {
        // Arrange
        _enrollmentStore
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<List<EnrolledRecipient>>("Could not read enrollment CSV"));

        // Act
        await _sut.InvokeAsync();

        // Assert
        await _lastRanService.DidNotReceive().GetTimestampAsync(Arg.Any<CancellationToken>());
        await _ledgerStore.DidNotReceive().GetWhatHasChangedAsync(
            Arg.Any<DateTime>(),
            Arg.Any<List<string>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_should_abort_workflow_when_retrieving_last_ran_timestamp_fails()
    {
        // Arrange
        var recipients = new List<EnrolledRecipient>
        {
            AnEnrolledRecipient().WithLaeStab("1234567").WithEmail("head@school.sch.uk")
        };

        _enrollmentStore
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(recipients));

        _lastRanService
            .GetTimestampAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DateTime>("Blob read failure"));

        // Act
        await _sut.InvokeAsync();

        // Assert
        await _ledgerStore.DidNotReceive().GetWhatHasChangedAsync(
            Arg.Any<DateTime>(),
            Arg.Any<List<string>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
        await _lastRanService.DidNotReceive().SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_should_abort_workflow_when_retrieving_ledger_changes_fails()
    {
        // Arrange
        var recipients = new List<EnrolledRecipient>
        {
            AnEnrolledRecipient().WithLaeStab("1234567").WithEmail("head@school.sch.uk")
        };

        _enrollmentStore
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(recipients));

        _lastRanService
            .GetTimestampAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)));

        _ledgerStore
            .GetWhatHasChangedAsync(
                Arg.Any<DateTime>(),
                Arg.Any<List<string>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Failure<List<ComparableCollectReturnStatus>>("Database connection timeout"));

        // Act
        await _sut.InvokeAsync();

        // Assert
        await _lastRanService.DidNotReceive().SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        await _threadingService.DidNotReceive().Batch(
            Arg.Any<IEnumerable<Notification>>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<Func<Notification, CancellationToken, Task<bool>>>());
    }

    [Fact]
    public async Task InvokeAsync_should_abort_workflow_when_updating_last_ran_timestamp_fails()
    {
        // Arrange
        var recipients = new List<EnrolledRecipient>
        {
            AnEnrolledRecipient().WithLaeStab("1111111").WithEmail("head@school1.sch.uk")
        };
        var changes = new List<ComparableCollectReturnStatus>
        {
            ACollectReturnStatus().WithLaeStab("1111111").WithSchoolName("School One").WithReturnStatusCode(ReturnStatusCodes.Authorised)
        };

        _enrollmentStore.ListAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(recipients));
        _lastRanService.GetTimestampAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(DateTime.UtcNow.AddDays(-1)));
        _ledgerStore.GetWhatHasChangedAsync(Arg.Any<DateTime>(), Arg.Any<List<string>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Result.Success(changes));
        _lastRanService.SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(Result.Failure("Failed to write timestamp blob"));

        // Act
        await _sut.InvokeAsync();

        // Assert
        await _threadingService.DidNotReceive().Batch(
            Arg.Any<IEnumerable<Notification>>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<Func<Notification, CancellationToken, Task<bool>>>());
    }

    [Fact]
    public async Task InvokeAsync_should_match_changed_schools_with_recipients_and_trigger_notifications()
    {
        // Arrange
        var recipients = new List<EnrolledRecipient>
        {
            AnEnrolledRecipient().WithLaeStab("1111111").WithEmail("head@school1.sch.uk"),
            AnEnrolledRecipient().WithLaeStab("1111111").WithEmail("office@school1.sch.uk"),
            AnEnrolledRecipient().WithLaeStab("2222222").WithEmail("admin@school2.sch.uk"),
            AnEnrolledRecipient().WithLaeStab("3333333").WithEmail("unaffected@school3.sch.uk")
        };

        var lastRan = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

        var changes = new List<ComparableCollectReturnStatus>
        {
            ACollectReturnStatus().WithLaeStab("1111111").WithSchoolName("School One").WithReturnStatusCode(ReturnStatusCodes.Authorised),
            ACollectReturnStatus().WithLaeStab("2222222").WithSchoolName("School Two").WithReturnStatusCode(ReturnStatusCodes.Submitted)
        };

        _enrollmentStore.ListAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(recipients));
        _lastRanService.GetTimestampAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(lastRan));
        _ledgerStore.GetWhatHasChangedAsync(lastRan, Arg.Any<List<string>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Result.Success(changes));
        _lastRanService.SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        List<Notification>? capturedNotifications = null;
        await _threadingService.Batch(
            Arg.Do<IEnumerable<Notification>>(items => capturedNotifications = items.ToList()),
            Arg.Any<CancellationToken>(),
            Arg.Any<Func<Notification, CancellationToken, Task<bool>>>());

        await _sut.InvokeAsync();

        await _lastRanService.Received(1).SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>());

        // Assert
        capturedNotifications.ShouldNotBeNull();
        capturedNotifications.Count.ShouldBe(3);

        capturedNotifications.ShouldContain(n =>
            n.LaeStab == "1111111" &&
            n.Recipient == "head@school1.sch.uk" &&
            n.Status == "Authorised" &&
            n.School == "School One");

        capturedNotifications.ShouldContain(n =>
            n.LaeStab == "1111111" &&
            n.Recipient == "office@school1.sch.uk" &&
            n.Status == "Authorised" &&
            n.School == "School One");

        capturedNotifications.ShouldContain(n =>
            n.LaeStab == "2222222" &&
            n.Recipient == "admin@school2.sch.uk" &&
            n.Status == "Submitted" &&
            n.School == "School Two");

        capturedNotifications.Any(n => n.LaeStab == "3333333").ShouldBeFalse();
    }

    [Fact]
    public async Task InvokeAsync_should_skip_enrolled_recipients_missing_email()
    {
        // Arrange
        var recipients = new List<EnrolledRecipient>
        {
            AnEnrolledRecipient().WithLaeStab("1111111").WithoutEmail(),
            AnEnrolledRecipient().WithLaeStab("1111111").WithEmail("valid@school1.sch.uk")
        };

        var changes = new List<ComparableCollectReturnStatus>
        {
            ACollectReturnStatus().WithLaeStab("1111111").WithSchoolName("School One").WithReturnStatusCode(ReturnStatusCodes.Authorised)
        };

        _enrollmentStore.ListAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(recipients));
        _lastRanService.GetTimestampAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(DateTime.UtcNow.AddDays(-1)));
        _ledgerStore.GetWhatHasChangedAsync(Arg.Any<DateTime>(), Arg.Any<List<string>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Result.Success(changes));
        _lastRanService.SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        List<Notification>? capturedNotifications = null;
        await _threadingService.Batch(
            Arg.Do<IEnumerable<Notification>>(items => capturedNotifications = items.ToList()),
            Arg.Any<CancellationToken>(),
            Arg.Any<Func<Notification, CancellationToken, Task<bool>>>());

        await _sut.InvokeAsync();

        // Assert
        capturedNotifications.ShouldNotBeNull();
        capturedNotifications.Count.ShouldBe(1);
        capturedNotifications.Single().Recipient.ShouldBe("valid@school1.sch.uk");
    }

    [Fact]
    public async Task InvokeAsync_should_send_email_via_gov_notify_inside_batch_worker()
    {
        // Arrange
        var recipients = new List<EnrolledRecipient>
        {
            AnEnrolledRecipient().WithLaeStab("1111111").WithEmail("head@school1.sch.uk")
        };
        var changes = new List<ComparableCollectReturnStatus>
        {
            ACollectReturnStatus().WithLaeStab("1111111").WithSchoolName("School One").WithReturnStatusCode(ReturnStatusCodes.Authorised)
        };

        _enrollmentStore.ListAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(recipients));
        _lastRanService.GetTimestampAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(DateTime.UtcNow.AddDays(-1)));
        _ledgerStore.GetWhatHasChangedAsync(Arg.Any<DateTime>(), Arg.Any<List<string>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Result.Success(changes));
        _lastRanService.SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        Func<Notification, CancellationToken, Task<bool>>? capturedWorker = null;

        await _threadingService.Batch(
            Arg.Any<IEnumerable<Notification>>(),
            Arg.Any<CancellationToken>(),
            Arg.Do<Func<Notification, CancellationToken, Task<bool>>>(worker => capturedWorker = worker));

        await _sut.InvokeAsync();

        // Assert
        capturedWorker.ShouldNotBeNull();

        _govNotifyService
            .SendMessage(
                GovNotifyTemplates.CensusStatusChange,
                "head@school1.sch.uk",
                Arg.Any<Dictionary<string, dynamic>>())
            .Returns(Result.Success(new NotificationResult()));

        var notification = ANotification()
            .WithLaeStab("1111111")
            .WithRecipient("head@school1.sch.uk")
            .WithStatus("Authorised")
            .WithSchool("School One")
            .Build();

        var workerResult = await capturedWorker(notification, CancellationToken.None);

        workerResult.ShouldBeTrue();
        await _govNotifyService.Received(1).SendMessage(
            GovNotifyTemplates.CensusStatusChange,
            "head@school1.sch.uk",
            Arg.Is<Dictionary<string, dynamic>>(d => MatchesPersonalisation(d, "Authorised", "School One")));
    }

    [Fact]
    public async Task InvokeAsync_should_return_false_in_batch_worker_when_gov_notify_service_fails()
    {
        // Arrange
        var recipients = new List<EnrolledRecipient>
        {
            AnEnrolledRecipient().WithLaeStab("1111111").WithEmail("head@school1.sch.uk")
        };
        var changes = new List<ComparableCollectReturnStatus>
        {
            ACollectReturnStatus().WithLaeStab("1111111").WithSchoolName("School One").WithReturnStatusCode(ReturnStatusCodes.Authorised)
        };

        _enrollmentStore.ListAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(recipients));
        _lastRanService.GetTimestampAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(DateTime.UtcNow.AddDays(-1)));
        _ledgerStore.GetWhatHasChangedAsync(Arg.Any<DateTime>(), Arg.Any<List<string>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Result.Success(changes));
        _lastRanService.SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        Func<Notification, CancellationToken, Task<bool>>? capturedWorker = null;

        await _threadingService.Batch(
            Arg.Any<IEnumerable<Notification>>(),
            Arg.Any<CancellationToken>(),
            Arg.Do<Func<Notification, CancellationToken, Task<bool>>>(worker => capturedWorker = worker));

        await _sut.InvokeAsync();

        // Assert
        capturedWorker.ShouldNotBeNull();

        _govNotifyService
            .SendMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, dynamic>>())
            .Returns(Result.Failure<NotificationResult>("Notify API rate limit exceeded"));

        var notification = ANotification()
            .WithLaeStab("1111111")
            .WithRecipient("head@school1.sch.uk")
            .WithStatus("Authorised")
            .WithSchool("School One")
            .Build();

        var workerResult = await capturedWorker(notification, CancellationToken.None);

        workerResult.ShouldBeFalse();
    }

    private static bool MatchesPersonalisation(Dictionary<string, dynamic>? dict, string expectedStatus, string expectedSchoolName)
    {
        if (dict is null)
        {
            return false;
        }

        if (!dict.TryGetValue("status", out var status) || !Equals(status, expectedStatus))
        {
            return false;
        }

        if (!dict.TryGetValue("school_name", out var school) || !Equals(school, expectedSchoolName))
        {
            return false;
        }

        return true;
    }
}

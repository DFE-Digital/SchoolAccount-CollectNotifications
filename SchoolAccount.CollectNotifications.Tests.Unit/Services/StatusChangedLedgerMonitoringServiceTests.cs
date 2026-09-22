using Microsoft.Extensions.Logging.Abstractions;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Services;
using static SchoolAccount.CollectNotifications.Tests.Common.Builders.NotificationBuilder;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Services;

public class StatusChangedLedgerMonitoringServiceTests
{
    private readonly ILastRanService _lastRanService = Substitute.For<ILastRanService>();
    private readonly ILedgerStore _ledgerStore = Substitute.For<ILedgerStore>();
    private readonly IThreadingService _threadingService = Substitute.For<IThreadingService>();
    private readonly IGovNotifyService _govNotifyService = Substitute.For<IGovNotifyService>();
    private readonly StatusChangedLedgerMonitoringService _sut;

    public StatusChangedLedgerMonitoringServiceTests()
    {
        var nullLogger = NullLogger<StatusChangedLedgerMonitoringService>.Instance;
        var instrumentation = new StatusChangedLedgerMonitoringServiceInstrumentation(nullLogger);
        
        _sut = new StatusChangedLedgerMonitoringService(
            instrumentation,
            _lastRanService,
            _ledgerStore,
            _threadingService,
            _govNotifyService);
    }

    private static CensusStatusChange AChange(
        string laeStab,
        string email,
        ReturnStatusCodes status,
        string schoolName = "A test School") =>
        new()
        {
            SchoolName = schoolName,
            LaeStab = laeStab,
            Email = email,
            ReturnStatusCode = status,
            PreviousReturnStatusCode = ReturnStatusCodes.LoadedAndValidated,
            UpdatedAt = new DateTime(2026, 9, 21, 22, 0, 0, DateTimeKind.Utc),
            Collection = "SchoolCensus2025_Spring",
            DcId = 1172
        };

    [Fact]
    public async Task InvokeAsync_should_abort_workflow_when_retrieving_last_ran_timestamp_fails()
    {
        // Arrange
        _lastRanService
            .GetTimestampAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DateTime>("Blob read failure"));

        // Act
        await _sut.InvokeAsync();

        // Assert
        await _ledgerStore.DidNotReceive().GetWhatHasChangedAsync(
            Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _lastRanService.DidNotReceive().SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_should_record_where_we_are_and_send_nothing_when_there_is_no_previous_run()
    {
        // Without this the first run treats the whole ledger as new and emails every school about
        // every qualifying change it has ever had.

        // Arrange
        _lastRanService
            .GetTimestampAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(LastRanService.NeverRun));

        _lastRanService
            .SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _sut.InvokeAsync();

        // Assert
        await _lastRanService.Received(1).SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>());

        await _ledgerStore.DidNotReceive().GetWhatHasChangedAsync(
            Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());

        await _threadingService.DidNotReceive().Batch(
            Arg.Any<IEnumerable<Notification>>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<Func<Notification, CancellationToken, Task<bool>>>());
    }

    [Fact]
    public async Task InvokeAsync_should_abort_workflow_when_retrieving_ledger_changes_fails()
    {
        // Arrange
        _lastRanService
            .GetTimestampAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)));

        _ledgerStore
            .GetWhatHasChangedAsync(Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<List<CensusStatusChange>>("Database connection timeout"));

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
    public async Task InvokeAsync_should_turn_every_change_into_a_notification_for_its_own_recipient()
    {
        // The query pairs each change with its registered recipients, so a school with two
        // contacts arrives as two changes and becomes two notifications.

        // Arrange
        var changes = new List<CensusStatusChange>
        {
            AChange("1111111", "head@school1.sch.uk", ReturnStatusCodes.Authorised, "School One"),
            AChange("1111111", "office@school1.sch.uk", ReturnStatusCodes.Authorised, "School One"),
            AChange("2222222", "admin@school2.sch.uk", ReturnStatusCodes.Submitted, "School Two")
        };

        _lastRanService.GetTimestampAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(DateTime.UtcNow.AddDays(-1)));
        _ledgerStore.GetWhatHasChangedAsync(Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(changes));
        _lastRanService.SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        List<Notification>? captured = null;
        await _threadingService.Batch(
            Arg.Do<IEnumerable<Notification>>(items => captured = items.ToList()),
            Arg.Any<CancellationToken>(),
            Arg.Any<Func<Notification, CancellationToken, Task<bool>>>());

        // Act
        await _sut.InvokeAsync();

        // Assert
        captured.ShouldNotBeNull();
        captured.Count.ShouldBe(3);

        captured.ShouldContain(n =>
            n.LaeStab == "1111111" && n.Recipient == "head@school1.sch.uk" &&
            n.Status == "Authorised" && n.School == "School One");

        captured.ShouldContain(n =>
            n.LaeStab == "1111111" && n.Recipient == "office@school1.sch.uk" &&
            n.Status == "Authorised" && n.School == "School One");

        captured.ShouldContain(n =>
            n.LaeStab == "2222222" && n.Recipient == "admin@school2.sch.uk" &&
            n.Status == "Submitted" && n.School == "School Two");

        await _lastRanService.Received(1).SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_should_send_email_via_gov_notify_inside_batch_worker()
    {
        // Arrange
        await ArrangeSingleChangeAsync();
        var worker = await CaptureWorkerAsync();

        _govNotifyService
            .SendMessage(GovNotifyTemplates.CensusStatusChange, "head@school1.sch.uk",
                Arg.Any<Dictionary<string, dynamic>>())
            .Returns(Result.Success(new NotificationResult()));

        var notification = ANotification()
            .WithLaeStab("1111111")
            .WithRecipient("head@school1.sch.uk")
            .WithStatus("Authorised")
            .WithSchool("School One")
            .Build();

        // Act
        var workerResult = await worker(notification, CancellationToken.None);

        // Assert
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
        await ArrangeSingleChangeAsync();
        var worker = await CaptureWorkerAsync();

        _govNotifyService
            .SendMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, dynamic>>())
            .Returns(Result.Failure<NotificationResult>("Notify API rate limit exceeded"));

        var notification = ANotification()
            .WithRecipient("head@school1.sch.uk")
            .WithStatus("Authorised")
            .WithSchool("School One")
            .Build();

        // Act
        var workerResult = await worker(notification, CancellationToken.None);

        // Assert
        workerResult.ShouldBeFalse();
    }

    private async Task ArrangeSingleChangeAsync()
    {
        var changes = new List<CensusStatusChange>
        {
            AChange("1111111", "head@school1.sch.uk", ReturnStatusCodes.Authorised, "School One")
        };

        _lastRanService.GetTimestampAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(DateTime.UtcNow.AddDays(-1)));
        _ledgerStore.GetWhatHasChangedAsync(Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(changes));
        _lastRanService.SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        await Task.CompletedTask;
    }

    private async Task<Func<Notification, CancellationToken, Task<bool>>> CaptureWorkerAsync()
    {
        Func<Notification, CancellationToken, Task<bool>>? captured = null;

        await _threadingService.Batch(
            Arg.Any<IEnumerable<Notification>>(),
            Arg.Any<CancellationToken>(),
            Arg.Do<Func<Notification, CancellationToken, Task<bool>>>(worker => captured = worker));

        await _sut.InvokeAsync();

        captured.ShouldNotBeNull();
        return captured;
    }

    private static bool MatchesPersonalisation(
        Dictionary<string, dynamic>? dict,
        string expectedStatus,
        string expectedSchoolName)
    {
        if (dict is null)
        {
            return false;
        }

        if (!dict.TryGetValue("status", out var status) || !Equals(status, expectedStatus))
        {
            return false;
        }

        return dict.TryGetValue("school_name", out var school) && Equals(school, expectedSchoolName);
    }
}

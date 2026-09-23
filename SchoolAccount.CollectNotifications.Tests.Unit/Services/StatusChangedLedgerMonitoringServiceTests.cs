using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Dtos;
using SchoolAccount.CollectNotifications.Models.Enums;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Services;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Services;

public class StatusChangedLedgerMonitoringServiceTests
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly ILastRanService _lastRanService = Substitute.For<ILastRanService>();
    private readonly ILedgerStore _ledgerStore = Substitute.For<ILedgerStore>();
    private readonly IGovNotifyService _govNotifyService = Substitute.For<IGovNotifyService>();
    private readonly StatusChangedLedgerMonitoringService _sut;

    public StatusChangedLedgerMonitoringServiceTests()
    {
        _sut = CreateService();
    }

    private StatusChangedLedgerMonitoringService CreateService(int delayBetweenSendsInMs = 0) =>
        new(
            new StatusChangedLedgerMonitoringServiceInstrumentation(
                NullLogger<StatusChangedLedgerMonitoringService>.Instance),
            _lastRanService,
            _ledgerStore,
            _govNotifyService,
            Options.Create(new GovNotifyOptions
            {
                ApiKey = "test-key",
                DelayBetweenSendsInMs = delayBetweenSendsInMs
            }));

    private static CensusStatusChange AChange(
        string laeStab,
        string email,
        ReturnStatusCodes status = ReturnStatusCodes.Authorised,
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

    private void GivenChanges(params CensusStatusChange[] changes)
    {
        _lastRanService.GetTimestampAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(DateTime.UtcNow.AddDays(-1)));
        _ledgerStore.GetWhatHasChangedAsync(Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(changes.ToList()));
        _lastRanService.SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
    }

    [Fact]
    public async Task InvokeAsync_should_abort_workflow_when_retrieving_last_ran_timestamp_fails()
    {
        // Arrange
        _lastRanService.GetTimestampAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DateTime>("Ledger unreachable"));

        // Act
        await _sut.InvokeAsync(_cancellationToken);

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
        _lastRanService.GetTimestampAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(LastRanService.NeverRun));
        _lastRanService.SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _sut.InvokeAsync(_cancellationToken);

        // Assert
        await _lastRanService.Received(1).SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        await _ledgerStore.DidNotReceive().GetWhatHasChangedAsync(
            Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _govNotifyService.DidNotReceive().SendMessage(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, dynamic>>());
    }

    [Fact]
    public async Task InvokeAsync_should_abort_workflow_when_retrieving_ledger_changes_fails()
    {
        // Arrange
        _lastRanService.GetTimestampAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(DateTime.UtcNow.AddDays(-1)));
        _ledgerStore.GetWhatHasChangedAsync(Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<List<CensusStatusChange>>("Database connection timeout"));

        // Act
        await _sut.InvokeAsync(_cancellationToken);

        // Assert
        await _lastRanService.DidNotReceive().SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        await _govNotifyService.DidNotReceive().SendMessage(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, dynamic>>());
    }

    [Fact]
    public async Task InvokeAsync_should_send_one_notification_per_change()
    {
        // The query pairs each change with its registered recipients, so a school with two
        // contacts arrives as two changes and becomes two emails.

        // Arrange
        GivenChanges(
            AChange("1111111", "head@school1.sch.uk", schoolName: "School One"),
            AChange("1111111", "office@school1.sch.uk", schoolName: "School One"),
            AChange("2222222", "admin@school2.sch.uk", ReturnStatusCodes.Submitted, "School Two"));

        _govNotifyService.SendMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, dynamic>>())
            .Returns(Result.Success(new NotificationResult()));

        // Act
        await _sut.InvokeAsync(_cancellationToken);

        // Assert
        await _govNotifyService.Received(1).SendMessage(
            GovNotifyTemplates.CensusStatusChange, "head@school1.sch.uk",
            Arg.Is<Dictionary<string, dynamic>>(d => MatchesPersonalisation(d, "Authorised", "School One")));

        await _govNotifyService.Received(1).SendMessage(
            GovNotifyTemplates.CensusStatusChange, "office@school1.sch.uk",
            Arg.Is<Dictionary<string, dynamic>>(d => MatchesPersonalisation(d, "Authorised", "School One")));

        await _govNotifyService.Received(1).SendMessage(
            GovNotifyTemplates.CensusStatusChange, "admin@school2.sch.uk",
            Arg.Is<Dictionary<string, dynamic>>(d => MatchesPersonalisation(d, "Submitted", "School Two")));

        await _lastRanService.Received(1).SetTimestampAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_should_keep_going_when_one_recipient_is_rejected()
    {
        // An address Notify won't accept comes back as a warning, which is a problem with that one
        // recipient and not a reason to stop telling everybody else.

        // Arrange
        GivenChanges(
            AChange("1111111", "bad-address"),
            AChange("2222222", "fine@school.sch.uk"));

        _govNotifyService.SendMessage(Arg.Any<string>(), "bad-address", Arg.Any<Dictionary<string, dynamic>>())
            .Returns(Result.Warning(new NotificationResult(), "The recipient email address is invalid."));

        _govNotifyService.SendMessage(Arg.Any<string>(), "fine@school.sch.uk", Arg.Any<Dictionary<string, dynamic>>())
            .Returns(Result.Success(new NotificationResult()));

        // Act
        await _sut.InvokeAsync(_cancellationToken);

        // Assert
        await _govNotifyService.Received(1).SendMessage(
            Arg.Any<string>(), "fine@school.sch.uk", Arg.Any<Dictionary<string, dynamic>>());
    }

    [Fact]
    public async Task InvokeAsync_should_stop_sending_when_notify_reports_a_failure()
    {
        // A rate limit or a bad key is a problem with the whole run, so there is no point working
        // through the rest of the list.

        // Arrange
        GivenChanges(
            AChange("1111111", "first@school.sch.uk"),
            AChange("2222222", "second@school.sch.uk"));

        _govNotifyService.SendMessage(Arg.Any<string>(), "first@school.sch.uk", Arg.Any<Dictionary<string, dynamic>>())
            .Returns(Result.Failure<NotificationResult>("Gov Notify rate limit exceeded."));

        // Act
        await _sut.InvokeAsync(_cancellationToken);

        // Assert
        await _govNotifyService.DidNotReceive().SendMessage(
            Arg.Any<string>(), "second@school.sch.uk", Arg.Any<Dictionary<string, dynamic>>());
    }

    [Fact]
    public async Task InvokeAsync_should_keep_going_when_a_send_throws()
    {
        // The last run date has already moved by this point, so giving up on the run would lose
        // every notification after the one that threw.

        // Arrange
        GivenChanges(
            AChange("1111111", "throws@school.sch.uk"),
            AChange("2222222", "fine@school.sch.uk"));

        _govNotifyService.SendMessage(Arg.Any<string>(), "throws@school.sch.uk", Arg.Any<Dictionary<string, dynamic>>())
            .Returns<Result<NotificationResult>>(_ => throw new HttpRequestException("Connection reset"));

        _govNotifyService.SendMessage(Arg.Any<string>(), "fine@school.sch.uk", Arg.Any<Dictionary<string, dynamic>>())
            .Returns(Result.Success(new NotificationResult()));

        // Act
        await _sut.InvokeAsync(_cancellationToken);

        // Assert
        await _govNotifyService.Received(1).SendMessage(
            Arg.Any<string>(), "fine@school.sch.uk", Arg.Any<Dictionary<string, dynamic>>());
    }

    [Fact]
    public async Task InvokeAsync_should_wait_between_sends_when_a_delay_is_configured()
    {
        // The lever for backing off if Notify ever starts rate limiting us. Asserts a lower bound
        // only, so a slow machine can't make it fail.

        // Arrange
        const int delayMs = 100;
        var sut = CreateService(delayMs);

        GivenChanges(
            AChange("1111111", "first@school.sch.uk"),
            AChange("2222222", "second@school.sch.uk"),
            AChange("3333333", "third@school.sch.uk"));

        _govNotifyService.SendMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, dynamic>>())
            .Returns(Result.Success(new NotificationResult()));

        // Act
        var stopwatch = Stopwatch.StartNew();
        await sut.InvokeAsync(_cancellationToken);
        stopwatch.Stop();

        // Assert
        // Three sends means two gaps, so nothing before the first one.
        stopwatch.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(delayMs * 2));

        await _govNotifyService.Received(3).SendMessage(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, dynamic>>());
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

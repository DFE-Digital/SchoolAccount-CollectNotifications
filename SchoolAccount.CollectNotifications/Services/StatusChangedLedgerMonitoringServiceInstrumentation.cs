using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace SchoolAccount.CollectNotifications.Services;

[SuppressMessage(
    "Major Code Smell",
    "S6672:Generic logger injection should match enclosing type",
    Justification = "Deliberate. These entries are the service's, so they should be categorised under it "
        + "rather than under a class name that only exists to hold the log messages."
)]
public sealed partial class StatusChangedLedgerMonitoringServiceInstrumentation(
    ILogger<StatusChangedLedgerMonitoringService> logger
)
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Looking for census status changes since {LastRan}, running at {RunningAt}"
    )]
    public partial void RunStarted(DateTime lastRan, DateTime runningAt);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Could not read the last run date, nothing will be sent: {Error}"
    )]
    public partial void LastRunDateUnavailable(string? error);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "No previous run recorded, recording {SeededTo} and sending nothing this time"
    )]
    public partial void NoPreviousRunRecorded(DateTime seededTo);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Could not record the first run, the next run will start over: {Error}"
    )]
    public partial void CouldNotRecordFirstRun(string? error);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Could not read what has changed, nothing will be sent: {Error}"
    )]
    public partial void LedgerUnavailable(string? error);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Found {Count} notifiable status changes"
    )]
    public partial void ChangesFound(int count);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Could not record this run, so nothing will be sent to avoid repeating it next time: {Error}"
    )]
    public partial void CouldNotRecordRun(string? error);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Notifying a recipient for {LaeStab}")]
    public partial void NotifyingRecipient(string laeStab);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Notification not delivered: {Error}")]
    public partial void NotificationRejected(string? error);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Could not reach Notify for {LaeStab}, moving on to the next one"
    )]
    public partial void NotificationFailed(Exception exception, string laeStab);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Stopped sending, {Remaining} notifications were not attempted"
    )]
    public partial void SendingStopped(int remaining);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Run finished in {ElapsedMilliseconds}ms"
    )]
    public partial void RunFinished(double elapsedMilliseconds);
}

using System.ComponentModel.DataAnnotations;

namespace SchoolAccount.CollectNotifications.Models.Options;

public sealed class GovNotifyOptions
{
    public const string SectionName = "GovNotify";
    
    [Required]
    public required string ApiKey { get; init; }
    
    /// <summary>
    /// How long to wait between sends. Zero, the default, sends as fast as the round trip allows,
    /// which at beta volumes is nowhere near Notify's limits. It's here so there's something to
    /// turn up if we ever do start getting rate limited.
    /// </summary>
    [Range(0, 60_000)]
    public int DelayBetweenSendsInMs { get; init; }
}
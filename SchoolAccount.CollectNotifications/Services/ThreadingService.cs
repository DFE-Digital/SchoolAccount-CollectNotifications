using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Interfaces;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Services;

public class ThreadingService(
    ILogger<ThreadingService> logger,
    IOptions<ThreadingOptions> threadingOptions
) : IThreadingService
{
    public async Task Batch<TSource>(IEnumerable<TSource> items, CancellationToken cancellationToken, Func<TSource, CancellationToken, Task<bool>> func)
    {
        var batches = items.Chunk(threadingOptions.Value.BatchAmount).ToList();
        logger.LogInformation("Chunking threadable items into blocks of {ValueBatchAmount} which created {BatchesCount} batches", threadingOptions.Value.BatchAmount, batches.Count);
        
        for (var b = 0; b < batches.Count; b++)
        {
            var batch = batches[b];
            
            logger.LogInformation("Batch {I}/{BatchesCount}: Starting a new batch of {BatchLength} items", b + 1, batches.Count, batch.Length);

            var actions = batch.Select(async (source, i) =>
            {
                try
                {
                    if (threadingOptions.Value.ItemWaitAmountInMs > 0)
                    {
                        logger.LogDebug("Pausing {I}/{BatchLength}. Pausing for {ValueItemWaitAmountInMs} milliseconds to respect rate limits...", i + 1, batch.Length, threadingOptions.Value.ItemWaitAmountInMs * i);
                        await Task.Delay(TimeSpan.FromMilliseconds(threadingOptions.Value.ItemWaitAmountInMs * i), cancellationToken);
                    }
                    
                    return await func(source, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Threading: Command failed: {message}", ex.Message);
                    return false;
                }
            });

            var results = await Task.WhenAll(actions);

            if (results.Any(x => !x))
            {
                logger.LogWarning("Threading: Informated to quit");
                break;
            }

            if (b >= batches.Count - 1)
            {
                continue;
            }

            if (threadingOptions.Value.BatchWaitAmountInMs > 0)
            {
                logger.LogDebug("Batch finished. Pausing for {ValueBatchWaitAmountInMs} milliseconds to respect rate limits...", threadingOptions.Value.BatchWaitAmountInMs);
                await Task.Delay(TimeSpan.FromMilliseconds(threadingOptions.Value.BatchWaitAmountInMs), cancellationToken);
            }
        }
    }
}

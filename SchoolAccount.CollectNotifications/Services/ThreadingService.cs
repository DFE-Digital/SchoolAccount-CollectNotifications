using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Models;
using SchoolAccount.CollectNotifications.Models.Options;

namespace SchoolAccount.CollectNotifications.Services;

public class ThreadingService(
    ILogger<ThreadingService> logger,
    IOptions<ThreadingOptions> threadingOptions
)
{
    public async Task Batch<TSource>(IEnumerable<TSource> items, CancellationToken cancellationToken, Func<TSource, CancellationToken, Task<bool>> func)
    {
        var batches = items.Chunk(threadingOptions.Value.BatchAmount).ToList();
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = threadingOptions.Value.MaxDegreeOfParallelism };
        var shouldContinue = true;

        for (var b = 0; b < batches.Count; b++)
        {
            var batch = batches[b];
            
            logger.LogInformation($"Batch {b + 1}/{batch.Length}: Starting a new batch of {batch.Length} items");

            await Parallel.ForEachAsync(source: batch, parallelOptions: parallelOptions, async (source, token) =>
            {
                if (token.IsCancellationRequested || !shouldContinue)
                {
                    return;
                }
                
                try
                {
                    shouldContinue = await func(source, token);
                    logger.LogDebug("Threading: Command complete");
                }
                catch (Exception exception)
                {
                    logger.LogCritical(exception, "Threading: Command failed: {message}", exception.Message);
                }
            });

            if (!shouldContinue)
            {
                logger.LogWarning("Threading: Informated to quit");
                break;
            }

            if (b >= batches.Count - 1)
            {
                continue;
            }

            for (var i = 1; i <= threadingOptions.Value.BatchWaitAmountInSec; i++)
            {
                logger.LogDebug(
                    $"Batch finished. Pausing for {i}/{threadingOptions.Value.BatchWaitAmountInSec} seconds to respect rate limits...");

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
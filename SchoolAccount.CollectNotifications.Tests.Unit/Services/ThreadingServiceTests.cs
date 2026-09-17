using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SchoolAccount.CollectNotifications.Models.Options;
using SchoolAccount.CollectNotifications.Services;

namespace SchoolAccount.CollectNotifications.Tests.Unit.Services;

public class ThreadingServiceTests
{
    [Fact]
    public async Task Batch_should_process_all_items_across_batches()
    {
        // Arrange
        var options = Options.Create(new ThreadingOptions
        {
            BatchAmount = 2,
            MaxDegreeOfParallelism = 2,
            BatchWaitAmountInSec = 0
        });

        var sut = new ThreadingService(NullLogger<ThreadingService>.Instance, options);
        
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var processed = new ConcurrentBag<int>();

        // Act
        await sut.Batch(items, CancellationToken.None, (item, _) =>
        {
            processed.Add(item);
            return Task.FromResult(true);
        });

        // Assert
        processed.Count.ShouldBe(5);
        processed.ShouldBe(items, ignoreOrder: true);
    }

    [Fact]
    public async Task Batch_should_stop_further_batches_when_func_returns_false()
    {
        // Arrange
        var options = Options.Create(new ThreadingOptions
        {
            BatchAmount = 2,
            MaxDegreeOfParallelism = 1,
            BatchWaitAmountInSec = 0
        });

        var sut = new ThreadingService(NullLogger<ThreadingService>.Instance, options);
        
        var items = new List<int> { 1, 2, 3, 4 };
        var processed = new ConcurrentBag<int>();

        // Act
        await sut.Batch(items, CancellationToken.None, (item, _) =>
        {
            processed.Add(item);
            return Task.FromResult(item != 1); // returns false on item 1
        });

        // Assert
        processed.ShouldNotContain(3);
        processed.ShouldNotContain(4);
    }

    [Fact]
    public async Task Batch_should_continue_processing_when_func_throws_exception()
    {
        // Arrange
        var options = Options.Create(new ThreadingOptions
        {
            BatchAmount = 3,
            MaxDegreeOfParallelism = 1,
            BatchWaitAmountInSec = 0
        });

        var sut = new ThreadingService(NullLogger<ThreadingService>.Instance, options);
        
        var items = new List<int> { 1, 2, 3 };
        var processed = new ConcurrentBag<int>();

        // Act
        await sut.Batch(items, CancellationToken.None, (item, _) =>
        {
            if (item == 2)
            {
                throw new InvalidOperationException("Simulated error");
            }

            processed.Add(item);
            return Task.FromResult(true);
        });

        // Arrange
        processed.ShouldContain(1);
        processed.ShouldContain(3);
        processed.ShouldNotContain(2);
    }

    [Fact]
    public async Task Batch_should_throw_operation_canceled_exception_when_cancellation_is_requested()
    {
        // Arrange
        var options = Options.Create(new ThreadingOptions
        {
            BatchAmount = 1,
            MaxDegreeOfParallelism = 1,
            BatchWaitAmountInSec = 0
        });

        var sut = new ThreadingService(NullLogger<ThreadingService>.Instance, options);
        using var cts = new CancellationTokenSource();
        
        var items = new List<int> { 1, 2, 3, 4 };

        // Act
        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await sut.Batch(items, cts.Token, (item, _) =>
            {
                if (item == 1)
                {
                    cts.Cancel();
                }

                return Task.FromResult(true);
            });
        });
    }
}

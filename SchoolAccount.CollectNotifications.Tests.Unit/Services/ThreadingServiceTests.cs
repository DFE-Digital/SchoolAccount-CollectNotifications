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
        processed.ShouldContain(1);
        processed.ShouldContain(2);
        processed.ShouldNotContain(3);
        processed.ShouldNotContain(4);
    }

    [Fact]
    public async Task Batch_should_catch_exception_in_func_and_stop_further_batches()
    {
        // Arrange
        var options = Options.Create(new ThreadingOptions
        {
            BatchAmount = 2,
            BatchWaitAmountInSec = 0
        });

        var sut = new ThreadingService(NullLogger<ThreadingService>.Instance, options);
        
        var items = new List<int> { 1, 2, 3, 4 };
        var processed = new ConcurrentBag<int>();

        // Act
        await sut.Batch(items, CancellationToken.None, (item, _) =>
        {
            if (item == 1)
            {
                throw new InvalidOperationException("Simulated error");
            }

            processed.Add(item);
            return Task.FromResult(true);
        });

        // Assert
        processed.ShouldContain(2);
        processed.ShouldNotContain(1);
        processed.ShouldNotContain(3);
        processed.ShouldNotContain(4);
    }

    [Fact]
    public async Task Batch_should_throw_operation_canceled_exception_when_cancellation_is_requested()
    {
        // Arrange
        var options = Options.Create(new ThreadingOptions
        {
            BatchAmount = 1,
            BatchWaitAmountInSec = 1
        });

        var sut = new ThreadingService(NullLogger<ThreadingService>.Instance, options);
        using var cts = new CancellationTokenSource();
        
        var items = new List<int> { 1, 2, 3, 4 };

        // Act & Assert
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

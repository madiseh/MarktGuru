using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.Jobs;
using OffersService.Models;
using OffersService.Tests.Helpers;
using Xunit;

namespace OffersService.Tests.Unit;

public class OfferExpiryJobTests
{
    [Fact]
    public async Task ExecuteAsync_ExpiresOnlyMatchingOffers_WithSingleUpdateStatement()
    {
        // Arrange
        var commandCounter = new DbCommandCounterInterceptor();
        using var connection = new SqliteConnection("Data Source=:memory:");
        await using var context = await CreateContextAsync(connection, commandCounter);
        var now = DateTime.UtcNow;

        context.Offers.AddRange(
            CreateOffer(1, now.AddDays(-2), OfferStatus.Active),
            CreateOffer(2, now.AddDays(-1), OfferStatus.Pending),
            CreateOffer(3, now.AddDays(-3), OfferStatus.Expired),
            CreateOffer(4, now.AddDays(1), OfferStatus.Active),
            CreateOffer(5, now.AddDays(2), OfferStatus.Pending));
        await context.SaveChangesAsync();

        var logger = new ListLogger<OfferExpiryJob>();
        var job = new OfferExpiryJob(context, logger);
        commandCounter.Reset();

        // Act
        var expiredCount = await job.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, expiredCount);
        Assert.Equal(1, commandCounter.ExecutedCount);
        Assert.Single(commandCounter.CommandTexts, IsUpdateCommand);
        Assert.Contains(logger.Messages, message => message.StartsWith("Expired 2 offers at "));

        context.ChangeTracker.Clear();
        var statuses = await context.Offers
            .OrderBy(offer => offer.Id)
            .ToDictionaryAsync(offer => offer.Id, offer => offer.Status);

        Assert.Equal(OfferStatus.Expired, statuses[1]);
        Assert.Equal(OfferStatus.Expired, statuses[2]);
        Assert.Equal(OfferStatus.Expired, statuses[3]);
        Assert.Equal(OfferStatus.Active, statuses[4]);
        Assert.Equal(OfferStatus.Pending, statuses[5]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlreadyClean_ReturnsZeroAndLogsResult()
    {
        // Arrange
        var commandCounter = new DbCommandCounterInterceptor();
        using var connection = new SqliteConnection("Data Source=:memory:");
        await using var context = await CreateContextAsync(connection, commandCounter);
        var now = DateTime.UtcNow;

        context.Offers.AddRange(
            CreateOffer(1, now.AddDays(-2), OfferStatus.Expired),
            CreateOffer(2, now.AddDays(1), OfferStatus.Active));
        await context.SaveChangesAsync();

        var logger = new ListLogger<OfferExpiryJob>();
        var job = new OfferExpiryJob(context, logger);
        commandCounter.Reset();

        // Act
        var expiredCount = await job.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.Equal(0, expiredCount);
        Assert.Equal(1, commandCounter.ExecutedCount);
        Assert.Single(commandCounter.CommandTexts, IsUpdateCommand);
        Assert.Contains(logger.Messages, message => message.StartsWith("Expired 0 offers at "));
    }

    private static async Task<AppDbContext> CreateContextAsync(
        DbConnection connection,
        DbCommandCounterInterceptor commandCounter)
    {
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(commandCounter)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Offers.RemoveRange(context.Offers);
        await context.SaveChangesAsync();

        return context;
    }

    private static Offer CreateOffer(int id, DateTime validTo, OfferStatus status)
    {
        return new Offer
        {
            Id = id,
            ProductId = 1,
            Price = 1.00m,
            ValidFrom = validTo.AddDays(-7),
            ValidTo = validTo,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static bool IsUpdateCommand(string commandText)
    {
        return commandText.TrimStart().StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase);
    }
}

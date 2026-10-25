using GenoCare.Domain.Inventory;
using Microsoft.EntityFrameworkCore;

namespace GenoCare.Infrastructure.Persistence;

public sealed class InventoryPersistence(IDbContextFactory<GenoCareDbContext> contextFactory)
{
    public async Task PostMovementAsync(StockMovement movement, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(movement);
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        if (await db.StockMovements.AnyAsync(x => x.TransactionId == movement.Id, cancellationToken))
            throw new InvalidOperationException("This stock transaction has already been posted.");
        if (movement is StockIssue && movement.Condition != StockCondition.Available)
            throw new InvalidOperationException("Stock issues must use available stock.");
        if (movement is StockIssue)
            await EnsureAvailableAsync(db, movement.BatchId, movement.Quantity.Value, cancellationToken);
        db.StockMovements.Add(ToRecord(movement, movement.Id));
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task PostTransitionAsync(InventoryStatusTransition transition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transition);
        var movements = transition.ToMovements();
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        if (await db.StockMovements.AnyAsync(x => x.TransactionId == transition.Id, cancellationToken))
            throw new InvalidOperationException("This status transition has already been posted.");
        var balance = await GetBalanceAsync(db, transition.BatchId, cancellationToken);
        var sourceQuantity = balance.For(transition.From);
        if (sourceQuantity < transition.Quantity.Value)
            throw new InvalidOperationException("The source condition does not have enough stock.");
        db.StockMovements.AddRange(movements.Select((movement, index) => ToRecord(movement, transition.Id, index)));
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<StockBalance> GetBalanceAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await GetBalanceAsync(db, batchId, cancellationToken);
    }

    private static async Task EnsureAvailableAsync(GenoCareDbContext db, Guid batchId, int quantity, CancellationToken cancellationToken)
    {
        var balance = await GetBalanceAsync(db, batchId, cancellationToken);
        if (balance.Available < quantity)
            throw new InvalidOperationException("Insufficient available stock.");
    }

    private static async Task<StockBalance> GetBalanceAsync(GenoCareDbContext db, Guid batchId, CancellationToken cancellationToken)
    {
        var records = await db.StockMovements.Where(x => x.BatchId == batchId).ToListAsync(cancellationToken);
        return StockBalance.FromMovements(records.Select(ToDomain), batchId);
    }

    private static StockMovementRecord ToRecord(StockMovement movement, Guid transactionId, int entryIndex = 0) => new()
    {
        Id = entryIndex == 0 ? movement.Id : Guid.NewGuid(),
        TransactionId = transactionId,
        EntryIndex = entryIndex,
        ProductId = movement.ProductId,
        BatchId = movement.BatchId,
        Quantity = movement.Quantity.Value,
        Condition = movement.Condition,
        Direction = movement is StockAdjustment adjustment ? adjustment.Direction : null,
        Kind = movement switch
        {
            StockReceipt => PersistedMovementKind.Receipt,
            OpeningStock => PersistedMovementKind.OpeningStock,
            StockIssue => PersistedMovementKind.Issue,
            StockAdjustment => PersistedMovementKind.Adjustment,
            _ => throw new ArgumentOutOfRangeException(nameof(movement))
        },
        SourceReference = movement.SourceReference,
        OccurredAt = movement.OccurredAt
    };

    private static StockMovement ToDomain(StockMovementRecord record) => record.Kind switch
    {
        PersistedMovementKind.Receipt => new StockReceipt(record.ProductId, record.BatchId, BoxQuantity.Positive(record.Quantity), record.Condition, record.SourceReference, record.OccurredAt, record.Id),
        PersistedMovementKind.OpeningStock => new OpeningStock(record.ProductId, record.BatchId, BoxQuantity.Positive(record.Quantity), record.Condition, record.SourceReference, record.OccurredAt, record.Id),
        PersistedMovementKind.Issue => new StockIssue(record.ProductId, record.BatchId, BoxQuantity.Positive(record.Quantity), record.Condition, record.SourceReference, record.OccurredAt, record.Id),
        PersistedMovementKind.Adjustment when record.Direction is AdjustmentDirection direction => new StockAdjustment(record.ProductId, record.BatchId, BoxQuantity.Positive(record.Quantity), direction, record.Condition, record.SourceReference, record.OccurredAt, record.Id),
        _ => throw new InvalidOperationException("Persisted movement is invalid.")
    };
}

internal static class StockBalanceExtensions
{
    public static int For(this StockBalance balance, StockCondition condition) => condition switch
    {
        StockCondition.Available => balance.Available,
        StockCondition.Quarantined => balance.Quarantined,
        StockCondition.Damaged => balance.Damaged,
        _ => throw new ArgumentOutOfRangeException(nameof(condition))
    };
}

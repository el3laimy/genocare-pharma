using GenoCare.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace GenoCare.Infrastructure.Persistence;

public sealed class GenoCareDbContext(DbContextOptions<GenoCareDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<StockMovementRecord> StockMovements => Set<StockMovementRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var moneyConverter = new ValueConverter<decimal, string>(
            value => value.ToString("0.##############################", CultureInfo.InvariantCulture),
            value => decimal.Parse(value, CultureInfo.InvariantCulture));

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products", table =>
            {
                table.HasCheckConstraint("CK_Products_RetailPrice_NonNegative", "CAST(CurrentRetailPrice AS NUMERIC) >= 0");
                table.HasCheckConstraint("CK_Products_PurchasePrice_NonNegative", "CAST(CurrentPurchasePrice AS NUMERIC) >= 0");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.InternalCode).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.InternalCode).IsUnique();
            entity.Property(x => x.ArabicName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.CurrentRetailPrice).HasConversion(moneyConverter).HasColumnType("TEXT").IsRequired();
            entity.Property(x => x.CurrentPurchasePrice).HasConversion(moneyConverter).HasColumnType("TEXT").IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Ignore(x => x.SellingUnit);
        });

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.ToTable("Batches", table => table.HasCheckConstraint("CK_Batches_BatchNumber_NotBlank", "length(trim(BatchNumber)) > 0"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BatchNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ExpiryDate).HasConversion<DateOnlyConverter, DateOnlyComparer>();
            entity.HasIndex(x => new { x.ProductId, x.BatchNumber }).IsUnique();
            entity.HasAlternateKey(x => new { x.ProductId, x.Id });
            entity.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockMovementRecord>(entity =>
        {
            entity.ToTable("StockMovements", table =>
            {
                table.HasCheckConstraint("CK_StockMovements_Quantity_Positive", "Quantity > 0");
                table.HasCheckConstraint("CK_StockMovements_Condition_Valid", "Condition IN (1, 2, 3)");
                table.HasCheckConstraint("CK_StockMovements_Kind_Valid", "Kind IN (1, 2, 3, 4)");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceReference).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Kind).HasConversion<int>().IsRequired();
            entity.Property(x => x.Condition).HasConversion<int>().IsRequired();
            entity.Property(x => x.Direction).HasConversion<int>();
            entity.HasIndex(x => new { x.TransactionId, x.EntryIndex }).IsUnique();
            entity.HasIndex(x => new { x.BatchId, x.OccurredAt });
            entity.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Batch>().WithMany().HasForeignKey(x => new { x.ProductId, x.BatchId })
                .HasPrincipalKey(x => new { x.ProductId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureMovementAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnsureMovementAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnsureMovementAppendOnly()
    {
        if (ChangeTracker.Entries<StockMovementRecord>().Any(entry =>
            entry.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Stock movements are append-only and cannot be updated or deleted.");
    }
}

internal sealed class DateOnlyConverter : ValueConverter<DateOnly, string>
{
    public DateOnlyConverter() : base(
        value => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        value => DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture)) { }
}

internal sealed class DateOnlyComparer : ValueComparer<DateOnly>
{
    public DateOnlyComparer() : base((a, b) => a == b, value => value.GetHashCode(), value => value) { }
}

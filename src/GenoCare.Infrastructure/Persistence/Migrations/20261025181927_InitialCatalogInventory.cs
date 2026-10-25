using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenoCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalogInventory : Migration
    {
        private static readonly string[] ProductBatchKeyColumns = { "ProductId", "Id" };
        private static readonly string[] ProductBatchIndexColumns = { "ProductId", "BatchNumber" };
        private static readonly string[] StockBatchDateIndexColumns = { "BatchId", "OccurredAt" };
        private static readonly string[] StockProductBatchIndexColumns = { "ProductId", "BatchId" };
        private static readonly string[] TransactionEntryIndexColumns = { "TransactionId", "EntryIndex" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InternalCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ArabicName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CurrentRetailPrice = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentPurchasePrice = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Products_PurchasePrice_NonNegative", "CAST(CurrentPurchasePrice AS NUMERIC) >= 0");
                    table.CheckConstraint("CK_Products_RetailPrice_NonNegative", "CAST(CurrentRetailPrice AS NUMERIC) >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ExpiryDate = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                    table.UniqueConstraint("AK_Batches_ProductId_Id", x => new { x.ProductId, x.Id });
                    table.CheckConstraint("CK_Batches_BatchNumber_NotBlank", "length(trim(BatchNumber)) > 0");
                    table.ForeignKey(
                        name: "FK_Batches_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntryIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BatchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Condition = table.Column<int>(type: "INTEGER", nullable: false),
                    Direction = table.Column<int>(type: "INTEGER", nullable: true),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceReference = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.CheckConstraint("CK_StockMovements_Condition_Valid", "Condition IN (1, 2, 3)");
                    table.CheckConstraint("CK_StockMovements_Kind_Valid", "Kind IN (1, 2, 3, 4)");
                    table.CheckConstraint("CK_StockMovements_Quantity_Positive", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_StockMovements_Batches_ProductId_BatchId",
                        columns: x => new { x.ProductId, x.BatchId },
                        principalTable: "Batches",
                        principalColumns: ProductBatchKeyColumns,
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Batches_ProductId_BatchNumber",
                table: "Batches",
                columns: ProductBatchIndexColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_InternalCode",
                table: "Products",
                column: "InternalCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_BatchId_OccurredAt",
                table: "StockMovements",
                columns: StockBatchDateIndexColumns);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId_BatchId",
                table: "StockMovements",
                columns: StockProductBatchIndexColumns);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_TransactionId_EntryIndex",
                table: "StockMovements",
                columns: TransactionEntryIndexColumns,
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}

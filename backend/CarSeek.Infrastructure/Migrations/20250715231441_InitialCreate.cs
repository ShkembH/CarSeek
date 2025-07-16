using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarSeek.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarSeekUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSeekUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CarSeekActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSeekActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarSeekActivityLogs_CarSeekUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "CarSeekUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CarSeekDealerships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Address_Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address_State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address_PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CompanyUniqueNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BusinessCertificatePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSeekDealerships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarSeekDealerships_CarSeekUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "CarSeekUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarSeekCarListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Mileage = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FuelType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transmission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Features = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DealershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSeekCarListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarSeekCarListings_CarSeekDealerships_DealershipId",
                        column: x => x.DealershipId,
                        principalTable: "CarSeekDealerships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CarSeekCarListings_CarSeekUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "CarSeekUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CarSeekCarImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CarListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSeekCarImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarSeekCarImages_CarSeekCarListings_CarListingId",
                        column: x => x.CarListingId,
                        principalTable: "CarSeekCarListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarSeekSavedListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CarListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSeekSavedListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarSeekSavedListings_CarSeekCarListings_CarListingId",
                        column: x => x.CarListingId,
                        principalTable: "CarSeekCarListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarSeekSavedListings_CarSeekUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "CarSeekUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "CarSeekUsers",
                columns: new[] { "Id", "City", "Country", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "PhoneNumber", "Role", "UpdatedAt" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@CarSeek.com", "System", true, "Administrator", "dGVzdHNhbHQxMjM0NTY3OA==.DWWRzeHacFCVfkMSYV96OVV5Oiflw4vNbbZlOMv9L9Q=", null, 2, null });

            migrationBuilder.CreateIndex(
                name: "IX_CarSeekActivityLogs_UserId",
                table: "CarSeekActivityLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSeekCarImages_CarListingId_DisplayOrder",
                table: "CarSeekCarImages",
                columns: new[] { "CarListingId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CarSeekCarImages_CarListingId_IsPrimary",
                table: "CarSeekCarImages",
                columns: new[] { "CarListingId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_CarSeekCarListings_DealershipId",
                table: "CarSeekCarListings",
                column: "DealershipId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSeekCarListings_UserId",
                table: "CarSeekCarListings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSeekDealerships_UserId",
                table: "CarSeekDealerships",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarSeekSavedListings_CarListingId",
                table: "CarSeekSavedListings",
                column: "CarListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSeekSavedListings_UserId_CarListingId",
                table: "CarSeekSavedListings",
                columns: new[] { "UserId", "CarListingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarSeekUsers_Email",
                table: "CarSeekUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId_RecipientId_ListingId",
                table: "ChatMessages",
                columns: new[] { "SenderId", "RecipientId", "ListingId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarSeekActivityLogs");

            migrationBuilder.DropTable(
                name: "CarSeekCarImages");

            migrationBuilder.DropTable(
                name: "CarSeekSavedListings");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "CarSeekCarListings");

            migrationBuilder.DropTable(
                name: "CarSeekDealerships");

            migrationBuilder.DropTable(
                name: "CarSeekUsers");
        }
    }
}

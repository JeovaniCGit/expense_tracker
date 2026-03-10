using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ExpenseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PermissionDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenTypeDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TimeToLiveInMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserRoleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    PermissionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermission_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_UserRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "UserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Firstname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Lastname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    PasswordLastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_UserRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "UserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    EstimatedBudget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RealBudget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TokenValue = table.Column<string>(type: "text", nullable: false),
                    TokenTypeId = table.Column<long>(type: "bigint", nullable: false),
                    TokenUserId = table.Column<long>(type: "bigint", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tokens_TokenTypes_TokenTypeId",
                        column: x => x.TokenTypeId,
                        principalTable: "TokenTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tokens_Users_TokenUserId",
                        column: x => x.TokenUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionRecordCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionRecordCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionRecordCategories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransactionValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionUserId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionCategoryId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionCollectionId = table.Column<long>(type: "bigint", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionRecords_Collections_TransactionCollectionId",
                        column: x => x.TransactionCollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionRecords_TransactionRecordCategories_TransactionC~",
                        column: x => x.TransactionCategoryId,
                        principalTable: "TransactionRecordCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionRecords_Users_TransactionUserId",
                        column: x => x.TransactionUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ExternalId", "PermissionDescription", "PermissionName" },
                values: new object[,]
                {
                    { 1L, new Guid("00edafe3-b047-5980-d0fa-da10f400c1e5"), "Admin privileges", "Admin" },
                    { 2L, new Guid("0918f225-c6f5-a57f-891d-a07ed959163d"), "Read user information", "User.Read" },
                    { 3L, new Guid("1ca8d15b-48f0-e349-5983-a900cfbc7fd7"), "Write user information", "User.Write" },
                    { 4L, new Guid("f149a130-8402-f611-4b56-cfa03d64c3bb"), "Delete user information", "User.Delete" },
                    { 5L, new Guid("3baea94e-bcfa-2629-84c3-651cdc9de0d3"), "Read record information", "Record.Read" },
                    { 6L, new Guid("b1d243fc-d4f7-4ae8-dc1d-d39e70838c1a"), "Write record information", "Record.Write" },
                    { 7L, new Guid("502ee51c-c616-910e-b779-63ba87667975"), "Delete record information", "Record.Delete" },
                    { 8L, new Guid("391e0c54-4528-7134-0753-fa270fb8f203"), "Read category information", "Category.Read" },
                    { 9L, new Guid("56c18f65-ba29-5499-e0ae-ee3329334c83"), "Write category information", "Category.Write" },
                    { 10L, new Guid("9ad393f2-dd00-d1f1-2c1e-07eab87aeb70"), "Delete category information", "Category.Delete" },
                    { 11L, new Guid("c5a9471b-6f8b-cb0b-7700-e3ed16440562"), "Read collection information", "Collection.Read" },
                    { 12L, new Guid("37bfa9f3-7ccf-e5f0-b539-4813004d8a82"), "Write collection information", "Collection.Write" },
                    { 13L, new Guid("18e15ec1-c800-fe03-7525-181cab9808ee"), "Delete collection information", "Collection.Delete" }
                });

            migrationBuilder.InsertData(
                table: "TokenTypes",
                columns: new[] { "Id", "ExternalId", "TimeToLiveInMinutes", "TokenTypeDescription" },
                values: new object[,]
                {
                    { 1L, new Guid("22446af7-0338-7c34-3be2-9dc71287959e"), 7200, "RefreshToken" },
                    { 2L, new Guid("595fa42b-5a70-f4a0-82a8-c5bd5f6153e8"), 7200, "EmailVerificationToken" },
                    { 3L, new Guid("7c403fa7-89f6-cfbc-d01d-5b3ba3df803c"), 7200, "PasswordResetToken" },
                    { 4L, new Guid("7a0be061-70d4-b8c7-638b-10803d4d67c9"), 7200, "AccessToken" }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "ExternalId", "UserRoleName" },
                values: new object[,]
                {
                    { 1L, new Guid("6da95da4-f60b-5957-70f2-d27af22be28a"), "System" },
                    { 2L, new Guid("00edafe3-b047-5980-d0fa-da10f400c1e5"), "Admin" },
                    { 3L, new Guid("74e727b0-535f-32c0-5e09-f7de8314c82f"), "RegularUser" }
                });

            migrationBuilder.InsertData(
                table: "RolePermission",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 2L, 1L },
                    { 3L, 1L },
                    { 4L, 1L },
                    { 5L, 1L },
                    { 6L, 1L },
                    { 7L, 1L },
                    { 8L, 1L },
                    { 9L, 1L },
                    { 10L, 1L },
                    { 11L, 1L },
                    { 12L, 1L },
                    { 13L, 1L },
                    { 1L, 2L },
                    { 2L, 2L },
                    { 3L, 2L },
                    { 4L, 2L },
                    { 5L, 2L },
                    { 6L, 2L },
                    { 7L, 2L },
                    { 8L, 2L },
                    { 9L, 2L },
                    { 10L, 2L },
                    { 11L, 2L },
                    { 12L, 2L },
                    { 13L, 2L },
                    { 2L, 3L },
                    { 3L, 3L },
                    { 4L, 3L },
                    { 5L, 3L },
                    { 6L, 3L },
                    { 7L, 3L },
                    { 8L, 3L },
                    { 9L, 3L },
                    { 10L, 3L },
                    { 11L, 3L },
                    { 12L, 3L },
                    { 13L, 3L }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ExternalId",
                table: "Collections",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_UserId_StartDate",
                table: "Collections",
                columns: new[] { "UserId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistory_UserId",
                table: "PasswordHistory",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ExternalId",
                table: "Permissions",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionId",
                table: "RolePermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_ExternalId",
                table: "Tokens",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_TokenTypeId",
                table: "Tokens",
                column: "TokenTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_TokenUserId",
                table: "Tokens",
                column: "TokenUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_TokenValue",
                table: "Tokens",
                column: "TokenValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenTypes_ExternalId",
                table: "TokenTypes",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenTypes_TokenTypeDescription",
                table: "TokenTypes",
                column: "TokenTypeDescription",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecordCategories_ExternalId",
                table: "TransactionRecordCategories",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecordCategories_UserId",
                table: "TransactionRecordCategories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecords_ExternalId",
                table: "TransactionRecords",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecords_TransactionCategoryId",
                table: "TransactionRecords",
                column: "TransactionCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecords_TransactionCollectionId",
                table: "TransactionRecords",
                column: "TransactionCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecords_TransactionUserId",
                table: "TransactionRecords",
                column: "TransactionUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_ExternalId",
                table: "UserRoles",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ExternalId",
                table: "Users",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordHistory");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "TransactionRecords");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "TokenTypes");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropTable(
                name: "TransactionRecordCategories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "UserRoles");
        }
    }
}

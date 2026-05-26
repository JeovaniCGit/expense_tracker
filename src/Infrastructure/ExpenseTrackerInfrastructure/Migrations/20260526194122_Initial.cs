using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ExpenseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
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
                    PermissionName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PermissionDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    TokenTypeDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TimeToLiveInMinutes = table.Column<int>(type: "integer", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    UserRoleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    PermissionId = table.Column<long>(type: "bigint", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                name: "EmailDeliveries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailDeliveries_Users_UserId",
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                columns: new[] { "PermissionId", "RoleId", "ExternalId" },
                values: new object[,]
                {
                    { 2L, 1L, new Guid("c9a0fd83-7c68-3b05-b702-4f6a7b61d3ac") },
                    { 3L, 1L, new Guid("d15761c8-0eb3-5d39-0d47-ab039a8e424a") },
                    { 4L, 1L, new Guid("94c623aa-2a52-fe29-823e-ed27757994af") },
                    { 5L, 1L, new Guid("a725ee2c-f14f-1f7f-34a7-8df0e7cb0227") },
                    { 6L, 1L, new Guid("8c424643-5a9a-0726-b5ea-217c982fe1b9") },
                    { 7L, 1L, new Guid("9a7ac054-05a3-69ad-799f-9bc6dbef753c") },
                    { 8L, 1L, new Guid("2cbbf64c-fe68-ecce-2dd7-51d83718c1f1") },
                    { 9L, 1L, new Guid("0af91c38-304f-b552-905b-2020455980d7") },
                    { 10L, 1L, new Guid("1d871f8e-aad2-7050-1092-5e2d8c6a4117") },
                    { 11L, 1L, new Guid("ee9c5384-af1b-2c41-8f19-2290d05a9077") },
                    { 12L, 1L, new Guid("f95d305c-11ed-df10-384a-f70592d94a51") },
                    { 13L, 1L, new Guid("dfa90665-285c-28d2-504f-1d817deec235") },
                    { 1L, 2L, new Guid("4033cabb-30dd-5d7a-9ce8-c2c54dfb8914") },
                    { 2L, 2L, new Guid("5e233cc1-ee98-84e7-dd19-434b1d2cbac4") },
                    { 3L, 2L, new Guid("23e6bb54-e6a2-d0ba-ca96-5c136d673408") },
                    { 4L, 2L, new Guid("c4ad54e3-26ac-45d3-0e6b-9b80b96a3c0d") },
                    { 5L, 2L, new Guid("98bab483-e1be-80d9-35ae-18e866b007de") },
                    { 6L, 2L, new Guid("d0d1fc9e-45b4-3a10-16a8-e754c3d9ef57") },
                    { 7L, 2L, new Guid("12e81ae9-9edb-83df-917c-90df8fe67272") },
                    { 8L, 2L, new Guid("fe4bade1-7a77-97c1-94b8-fc63884b57a2") },
                    { 9L, 2L, new Guid("40e36409-d2ea-c2fe-2ae3-d766f27dff8e") },
                    { 10L, 2L, new Guid("882afe9a-73d9-176e-2574-7157129a8c1c") },
                    { 11L, 2L, new Guid("b65dd8d5-097a-5e70-43af-bb6ac596537f") },
                    { 12L, 2L, new Guid("9eff1f04-0617-9d7e-7e53-e6d5517fc691") },
                    { 13L, 2L, new Guid("a934a83a-be13-03fb-e508-cee05ab9ebe9") },
                    { 2L, 3L, new Guid("366f9fdb-bb63-57fd-d1f5-6e1ff41d2f3f") },
                    { 3L, 3L, new Guid("25be4811-3723-559b-5a8f-421a6ea8782d") },
                    { 4L, 3L, new Guid("4e9a33c9-0396-b818-b464-487334a52367") },
                    { 5L, 3L, new Guid("94b5897f-e947-f7c6-2e83-51deca4cd054") },
                    { 6L, 3L, new Guid("3fd0f514-7802-cf0e-2d69-11c9d9269d24") },
                    { 7L, 3L, new Guid("7f8eee39-2e96-0400-3ebf-10b2a6677f80") },
                    { 8L, 3L, new Guid("6a350754-f9cf-ed1d-c4fc-26c721f75596") },
                    { 9L, 3L, new Guid("3ebbcbd9-1683-a094-50dd-bf33e07814b3") },
                    { 10L, 3L, new Guid("ad32adf2-4a2f-cee3-af77-0521d1ee1e70") },
                    { 11L, 3L, new Guid("c66d048b-e67e-fccc-b8cf-a5e410dd2341") },
                    { 12L, 3L, new Guid("883e184e-bd6a-6ace-0826-1b976f55b6c1") },
                    { 13L, 3L, new Guid("ff513122-1b1f-5c42-e8fe-b4e66acd88dd") }
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
                name: "IX_Collections_UserId_StartDate_EndDate_Description",
                table: "Collections",
                columns: new[] { "UserId", "StartDate", "EndDate", "Description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailDeliveries_ExternalId",
                table: "EmailDeliveries",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailDeliveries_UserId",
                table: "EmailDeliveries",
                column: "UserId",
                unique: true);

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
                name: "IX_TransactionRecordCategories_UserId_CategoryName",
                table: "TransactionRecordCategories",
                columns: new[] { "UserId", "CategoryName" },
                unique: true);

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
                name: "IX_TransactionRecords_TransactionValue_TransactionUserId_Trans~",
                table: "TransactionRecords",
                columns: new[] { "TransactionValue", "TransactionUserId", "TransactionCategoryId" },
                unique: true);

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
                name: "EmailDeliveries");

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

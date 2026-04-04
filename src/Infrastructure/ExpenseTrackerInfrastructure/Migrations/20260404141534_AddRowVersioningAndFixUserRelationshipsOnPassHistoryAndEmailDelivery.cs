using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExpenseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersioningAndFixUserRelationshipsOnPassHistoryAndEmailDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionRecordCategories_UserId",
                table: "TransactionRecordCategories");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Users",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "TransactionRecords",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "TransactionRecordCategories",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Tokens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "RolePermission",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "PasswordHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Collections",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "EmailDeliveries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2L, 1L },
                column: "ExternalId",
                value: new Guid("c9a0fd83-7c68-3b05-b702-4f6a7b61d3ac"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3L, 1L },
                column: "ExternalId",
                value: new Guid("d15761c8-0eb3-5d39-0d47-ab039a8e424a"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4L, 1L },
                column: "ExternalId",
                value: new Guid("94c623aa-2a52-fe29-823e-ed27757994af"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5L, 1L },
                column: "ExternalId",
                value: new Guid("a725ee2c-f14f-1f7f-34a7-8df0e7cb0227"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6L, 1L },
                column: "ExternalId",
                value: new Guid("8c424643-5a9a-0726-b5ea-217c982fe1b9"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7L, 1L },
                column: "ExternalId",
                value: new Guid("9a7ac054-05a3-69ad-799f-9bc6dbef753c"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8L, 1L },
                column: "ExternalId",
                value: new Guid("2cbbf64c-fe68-ecce-2dd7-51d83718c1f1"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9L, 1L },
                column: "ExternalId",
                value: new Guid("0af91c38-304f-b552-905b-2020455980d7"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10L, 1L },
                column: "ExternalId",
                value: new Guid("1d871f8e-aad2-7050-1092-5e2d8c6a4117"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11L, 1L },
                column: "ExternalId",
                value: new Guid("ee9c5384-af1b-2c41-8f19-2290d05a9077"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12L, 1L },
                column: "ExternalId",
                value: new Guid("f95d305c-11ed-df10-384a-f70592d94a51"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13L, 1L },
                column: "ExternalId",
                value: new Guid("dfa90665-285c-28d2-504f-1d817deec235"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1L, 2L },
                column: "ExternalId",
                value: new Guid("4033cabb-30dd-5d7a-9ce8-c2c54dfb8914"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2L, 2L },
                column: "ExternalId",
                value: new Guid("5e233cc1-ee98-84e7-dd19-434b1d2cbac4"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3L, 2L },
                column: "ExternalId",
                value: new Guid("23e6bb54-e6a2-d0ba-ca96-5c136d673408"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4L, 2L },
                column: "ExternalId",
                value: new Guid("c4ad54e3-26ac-45d3-0e6b-9b80b96a3c0d"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5L, 2L },
                column: "ExternalId",
                value: new Guid("98bab483-e1be-80d9-35ae-18e866b007de"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6L, 2L },
                column: "ExternalId",
                value: new Guid("d0d1fc9e-45b4-3a10-16a8-e754c3d9ef57"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7L, 2L },
                column: "ExternalId",
                value: new Guid("12e81ae9-9edb-83df-917c-90df8fe67272"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8L, 2L },
                column: "ExternalId",
                value: new Guid("fe4bade1-7a77-97c1-94b8-fc63884b57a2"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9L, 2L },
                column: "ExternalId",
                value: new Guid("40e36409-d2ea-c2fe-2ae3-d766f27dff8e"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10L, 2L },
                column: "ExternalId",
                value: new Guid("882afe9a-73d9-176e-2574-7157129a8c1c"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11L, 2L },
                column: "ExternalId",
                value: new Guid("b65dd8d5-097a-5e70-43af-bb6ac596537f"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12L, 2L },
                column: "ExternalId",
                value: new Guid("9eff1f04-0617-9d7e-7e53-e6d5517fc691"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13L, 2L },
                column: "ExternalId",
                value: new Guid("a934a83a-be13-03fb-e508-cee05ab9ebe9"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2L, 3L },
                column: "ExternalId",
                value: new Guid("366f9fdb-bb63-57fd-d1f5-6e1ff41d2f3f"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3L, 3L },
                column: "ExternalId",
                value: new Guid("25be4811-3723-559b-5a8f-421a6ea8782d"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4L, 3L },
                column: "ExternalId",
                value: new Guid("4e9a33c9-0396-b818-b464-487334a52367"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5L, 3L },
                column: "ExternalId",
                value: new Guid("94b5897f-e947-f7c6-2e83-51deca4cd054"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6L, 3L },
                column: "ExternalId",
                value: new Guid("3fd0f514-7802-cf0e-2d69-11c9d9269d24"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 7L, 3L },
                column: "ExternalId",
                value: new Guid("7f8eee39-2e96-0400-3ebf-10b2a6677f80"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 8L, 3L },
                column: "ExternalId",
                value: new Guid("6a350754-f9cf-ed1d-c4fc-26c721f75596"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 9L, 3L },
                column: "ExternalId",
                value: new Guid("3ebbcbd9-1683-a094-50dd-bf33e07814b3"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10L, 3L },
                column: "ExternalId",
                value: new Guid("ad32adf2-4a2f-cee3-af77-0521d1ee1e70"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11L, 3L },
                column: "ExternalId",
                value: new Guid("c66d048b-e67e-fccc-b8cf-a5e410dd2341"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 12L, 3L },
                column: "ExternalId",
                value: new Guid("883e184e-bd6a-6ace-0826-1b976f55b6c1"));

            migrationBuilder.UpdateData(
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 13L, 3L },
                column: "ExternalId",
                value: new Guid("ff513122-1b1f-5c42-e8fe-b4e66acd88dd"));

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecords_TransactionValue_TransactionUserId_Trans~",
                table: "TransactionRecords",
                columns: new[] { "TransactionValue", "TransactionUserId", "TransactionCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecordCategories_UserId_CategoryName",
                table: "TransactionRecordCategories",
                columns: new[] { "UserId", "CategoryName" },
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_TransactionRecords_TransactionValue_TransactionUserId_Trans~",
                table: "TransactionRecords");

            migrationBuilder.DropIndex(
                name: "IX_TransactionRecordCategories_UserId_CategoryName",
                table: "TransactionRecordCategories");

            migrationBuilder.DropIndex(
                name: "IX_Collections_UserId_StartDate_EndDate_Description",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "TransactionRecords");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "TransactionRecordCategories");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "PasswordHistory");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Collections");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecordCategories_UserId",
                table: "TransactionRecordCategories",
                column: "UserId");
        }
    }
}

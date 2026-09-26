using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ExpenseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLocalRoleBasedAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserRoles_RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RoleId",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PermissionName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
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
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

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
                name: "IX_UserRoles_ExternalId",
                table: "UserRoles",
                column: "ExternalId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserRoles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "UserRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

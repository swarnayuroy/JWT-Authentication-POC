using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API_Service.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoggedInAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "IsVerified", "Name" },
                values: new object[,]
                {
                    { new Guid("1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e"), "doe.john@gmail.com", true, "John Doe" },
                    { new Guid("4b79aeeb-96cd-49bf-abf0-8b5f6f693467"), "doe.jane@gmail.com", true, "Jane Doe" }
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "CreatedAt", "LoggedInAt", "Password", "UserId" },
                values: new object[,]
                {
                    { new Guid("48e283eb-8193-4de0-a025-e8dcb6bc678a"), new DateTime(2025, 4, 10, 10, 15, 30, 0, DateTimeKind.Unspecified), null, "TestJohn@1994", new Guid("1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e") },
                    { new Guid("50b26a44-c7f9-462a-9d4f-c66ac2e9938e"), new DateTime(2025, 5, 10, 10, 15, 30, 0, DateTimeKind.Unspecified), null, "TestJane@1994", new Guid("4b79aeeb-96cd-49bf-abf0-8b5f6f693467") }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "UserId", "Role" },
                values: new object[,]
                {
                    { new Guid("1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e"), 2 },
                    { new Guid("4b79aeeb-96cd-49bf-abf0-8b5f6f693467"), 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

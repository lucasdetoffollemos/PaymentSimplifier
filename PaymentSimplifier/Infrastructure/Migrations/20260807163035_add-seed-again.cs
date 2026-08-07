using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PaymentSimplifier.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addseedagain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "cpf", "email", "name", "password", "user_type" },
                values: new object[,]
                {
                    { new Guid("019fd90d-97c1-720c-812b-f502f65f600d"), "12345679", "user@gmail.com", "User1", "user1", 1 },
                    { new Guid("019fd90d-e427-74cd-aaf7-a6464f779375"), "12345678", "lojista@gmail.com", "Lojista1", "lojista1", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("019fd90d-97c1-720c-812b-f502f65f600d"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("019fd90d-e427-74cd-aaf7-a6464f779375"));
        }
    }
}

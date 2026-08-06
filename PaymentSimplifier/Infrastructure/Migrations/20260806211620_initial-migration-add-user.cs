using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PaymentSimplifier.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialmigrationadduser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    user_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "cpf", "email", "name", "password", "user_type" },
                values: new object[,]
                {
                    { new Guid("6e4390e9-f996-4c8c-94e0-b6725777c9da"), "12345678", "lojista@gmail.com", "Lojista1", "lojista1", 2 },
                    { new Guid("87ad49d2-f21d-43c0-ae54-265ed6ad61ff"), "12345679", "user@gmail.com", "User1", "user1", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}

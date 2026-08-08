using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentSimplifier.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRegistrationAndUniqueDocumentEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "cpf",
                table: "users",
                newName: "document");

            migrationBuilder.AlterColumn<string>(
                name: "document",
                table: "users",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("019fd90d-97c1-720c-812b-f502f65f600d"),
                column: "document",
                value: "52998224725");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("019fd90d-e427-74cd-aaf7-a6464f779375"),
                column: "document",
                value: "11222333000181");

            migrationBuilder.CreateIndex(
                name: "ix_users_document",
                table: "users",
                column: "document",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_document",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_email",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "document",
                table: "users",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14);

            migrationBuilder.RenameColumn(
                name: "document",
                table: "users",
                newName: "cpf");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("019fd90d-97c1-720c-812b-f502f65f600d"),
                column: "cpf",
                value: "12345679");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("019fd90d-e427-74cd-aaf7-a6464f779375"),
                column: "cpf",
                value: "12345678");
        }
    }
}

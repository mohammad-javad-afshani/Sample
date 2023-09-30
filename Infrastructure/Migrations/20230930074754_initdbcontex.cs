using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class initdbcontex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Customers_CustomerId1",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_CustomerId1",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "CustomerId1",
                table: "Addresses");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Addresses",
                newName: "street");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "Addresses",
                newName: "postalCode");

            migrationBuilder.RenameColumn(
                name: "CountryTitle",
                table: "Addresses",
                newName: "countrytitle");

            migrationBuilder.RenameColumn(
                name: "CityTitle",
                table: "Addresses",
                newName: "cityTitle");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Addresses",
                newName: "id");

            migrationBuilder.AlterColumn<string>(
                name: "phonenumber",
                table: "Customers",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "street",
                table: "Addresses",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "postalCode",
                table: "Addresses",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "countrytitle",
                table: "Addresses",
                newName: "CountryTitle");

            migrationBuilder.RenameColumn(
                name: "cityTitle",
                table: "Addresses",
                newName: "CityTitle");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Addresses",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "phonenumber",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId1",
                table: "Addresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CustomerId1",
                table: "Addresses",
                column: "CustomerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Customers_CustomerId1",
                table: "Addresses",
                column: "CustomerId1",
                principalTable: "Customers",
                principalColumn: "Id");
        }
    }
}

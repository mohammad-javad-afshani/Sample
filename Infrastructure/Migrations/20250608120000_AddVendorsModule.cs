using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations;

public partial class AddVendorsModule : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Vendors",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                TaxId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                InternalNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false, defaultValue: ""),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                migrationBuilder.PrimaryKey("PK_Vendors", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Vendors_Email",
            table: "Vendors",
            column: "Email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Vendors");
    }
}

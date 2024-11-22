using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fryzjer.Migrations
{
    /// <inheritdoc />
    public partial class AddHairdresserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "Hairdresser");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Specialization",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Administrator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Surname = table.Column<string>(type: "TEXT", nullable: true),
                    Login = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrator", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administrator");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Specialization");

            migrationBuilder.AddColumn<char>(
                name: "status",
                table: "Hairdresser",
                type: "TEXT",
                nullable: false,
                defaultValue: '\0');
        }
    }
}

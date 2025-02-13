using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCatEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Cats",
                newName: "ImagePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Cats",
                newName: "ImageUrl");
        }
    }
}

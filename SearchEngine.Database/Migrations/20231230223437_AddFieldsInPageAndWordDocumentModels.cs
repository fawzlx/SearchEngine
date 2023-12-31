using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchEngine.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsInPageAndWordDocumentModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizeBody",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalizeBody",
                table: "Pages");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchPublicationTracker.Migrations
{
    /// <inheritdoc />
    public partial class PublicationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecordUrl",
                table: "Publications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordUrl",
                table: "Publications");
        }
    }
}

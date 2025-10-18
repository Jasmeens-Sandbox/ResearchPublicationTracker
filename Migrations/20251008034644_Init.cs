using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchPublicationTracker.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Publications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Provider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Abstract = table.Column<string>(type: "TEXT", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicationAuthors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    PublicationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationAuthors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationAuthors_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationAuthors_PublicationId",
                table: "PublicationAuthors",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_Provider_ProviderId",
                table: "Publications",
                columns: new[] { "Provider", "ProviderId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicationAuthors");

            migrationBuilder.DropTable(
                name: "Publications");
        }
    }
}

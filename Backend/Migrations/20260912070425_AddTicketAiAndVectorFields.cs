using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketAiAndVectorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AddColumn<string>(
                name: "AiCategory",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                table: "Tickets",
                type: "vector(384)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiCategory",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Tickets");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}

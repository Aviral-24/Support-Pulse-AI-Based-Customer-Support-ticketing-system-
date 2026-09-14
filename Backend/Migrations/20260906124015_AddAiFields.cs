using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAiFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiImageAnalysis",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiSentiment",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiSummary",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiTranscription",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuggestedResponse",
                table: "Tickets",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiImageAnalysis",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AiSentiment",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AiSummary",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AiTranscription",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SuggestedResponse",
                table: "Tickets");
        }
    }
}

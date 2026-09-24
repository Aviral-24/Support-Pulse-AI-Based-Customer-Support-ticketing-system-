using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddKnowledgeBaseEmbedding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                table: "KnowledgeBases",
                type: "vector(384)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "KnowledgeBases");
        }
    }
}

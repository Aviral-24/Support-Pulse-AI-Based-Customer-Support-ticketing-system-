using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AuditLogs",
                newName: "Timestamp");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AuditLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "AuditLogs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Details",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "AuditLogs",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AuditLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class FixRecordingInterviewType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recording",
                table: "Interviews");

            migrationBuilder.AddColumn<string>(
                name: "Recording",
                table: "Interviews",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recording",
                table: "Interviews");

            migrationBuilder.AddColumn<byte[]>(
                name: "Recording",
                table: "Interviews",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}

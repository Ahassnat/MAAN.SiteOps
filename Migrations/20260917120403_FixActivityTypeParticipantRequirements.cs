using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAN.SiteOps.Migrations
{
    /// <inheritdoc />
    public partial class FixActivityTypeParticipantRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ActivityTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequiresParticipants",
                value: true);

            migrationBuilder.UpdateData(
                table: "ActivityTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "RequiresParticipants",
                value: true);

            migrationBuilder.UpdateData(
                table: "ActivityTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "RequiresParticipants",
                value: true);

            migrationBuilder.UpdateData(
                table: "ActivityTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "RequiresParticipants",
                value: true);

            migrationBuilder.UpdateData(
                table: "ActivityTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "RequiresParticipants",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ActivityTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequiresParticipants",
                value: false);

            migrationBuilder.UpdateData(
                table: "ActivityTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "RequiresParticipants",
                value: false);

            migrationBuilder.UpdateData(
                table: "ActivityTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "RequiresParticipants",
                value: false);

            migrationBuilder.UpdateData(
                table: "ActivityTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "RequiresParticipants",
                value: false);

            migrationBuilder.UpdateData(
                table: "ActivityTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "RequiresParticipants",
                value: false);
        }
    }
}

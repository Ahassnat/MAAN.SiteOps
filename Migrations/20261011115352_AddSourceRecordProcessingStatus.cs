using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAN.SiteOps.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceRecordProcessingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessingStatus",
                table: "ActivitySourceRecords",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WarningsJson",
                table: "ActivitySourceRecords",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingStatus",
                table: "ActivitySourceRecords");

            migrationBuilder.DropColumn(
                name: "WarningsJson",
                table: "ActivitySourceRecords");
        }
    }
}

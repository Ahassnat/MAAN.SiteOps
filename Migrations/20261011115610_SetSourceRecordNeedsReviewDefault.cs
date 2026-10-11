using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAN.SiteOps.Migrations
{
    /// <inheritdoc />
    public partial class SetSourceRecordNeedsReviewDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProcessingStatus",
                table: "ActivitySourceRecords",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "NeedsReview",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 40);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProcessingStatus",
                table: "ActivitySourceRecords",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 40,
                oldDefaultValue: "NeedsReview");
        }
    }
}

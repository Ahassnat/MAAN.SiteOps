using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAN.SiteOps.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleReportFieldsPerIndicator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportIndicatorMappings_ProjectReportId_ProjectIndicatorId",
                table: "ReportIndicatorMappings");

            migrationBuilder.CreateIndex(
                name: "IX_ReportIndicatorMappings_ProjectReportId_ReportFieldCode",
                table: "ReportIndicatorMappings",
                columns: new[] { "ProjectReportId", "ReportFieldCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportIndicatorMappings_ProjectReportId_ReportFieldCode",
                table: "ReportIndicatorMappings");

            migrationBuilder.CreateIndex(
                name: "IX_ReportIndicatorMappings_ProjectReportId_ProjectIndicatorId",
                table: "ReportIndicatorMappings",
                columns: new[] { "ProjectReportId", "ProjectIndicatorId" },
                unique: true);
        }
    }
}

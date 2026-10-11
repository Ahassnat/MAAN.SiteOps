using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAN.SiteOps.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleActivityFromProjectSite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProjectSiteId",
                table: "Activities",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "SiteId",
                table: "Activities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Activities
                SET SiteId = (
                    SELECT ProjectSites.SiteId
                    FROM ProjectSites
                    WHERE ProjectSites.Id = Activities.ProjectSiteId
                )
                WHERE ProjectSiteId IS NOT NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "SiteId",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SiteId",
                table: "Activities",
                column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Sites_SiteId",
                table: "Activities",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Sites_SiteId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_SiteId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "Activities");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectSiteId",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}

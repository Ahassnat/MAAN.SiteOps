using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAN.SiteOps.Migrations
{
    /// <inheritdoc />
    public partial class BackfillActivityProjectAttributions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO ActivityProjectAttributions
                    (ActivityId, ProjectSiteId, IsPrimary, AttributionRole, Notes, CreatedAt)
                SELECT
                    a.Id,
                    a.ProjectSiteId,
                    1,
                    'Primary',
                    NULL,
                    CURRENT_TIMESTAMP
                FROM Activities a
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ActivityProjectAttributions apa
                    WHERE apa.ActivityId = a.Id
                      AND apa.ProjectSiteId = a.ProjectSiteId
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
                        migrationBuilder.Sql("""
                                DELETE FROM ActivityProjectAttributions
                                WHERE IsPrimary = 1
                                    AND AttributionRole = 'Primary'
                                    AND NOT EXISTS (
                                            SELECT 1
                                            FROM Activities a
                                            WHERE a.Id = ActivityProjectAttributions.ActivityId
                                                AND a.ProjectSiteId <> ActivityProjectAttributions.ProjectSiteId
                                    );
                                """);
        }
    }
}

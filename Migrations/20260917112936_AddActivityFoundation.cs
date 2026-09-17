using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MAAN.SiteOps.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActivityCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RequiresParticipants = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresPWD = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresTopic = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowsMultipleTopics = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresItems = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresAttachment = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresSummary = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityTypes_ActivityCategories_ActivityCategoryId",
                        column: x => x.ActivityCategoryId,
                        principalTable: "ActivityCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectSiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActivityTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActivityDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ImplementationRole = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_ActivityTypes_ActivityTypeId",
                        column: x => x.ActivityTypeId,
                        principalTable: "ActivityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Activities_ProjectSites_ProjectSiteId",
                        column: x => x.ProjectSiteId,
                        principalTable: "ProjectSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Men = table.Column<int>(type: "INTEGER", nullable: false),
                    Women = table.Column<int>(type: "INTEGER", nullable: false),
                    Boys = table.Column<int>(type: "INTEGER", nullable: false),
                    Girls = table.Column<int>(type: "INTEGER", nullable: false),
                    ElderlyMen = table.Column<int>(type: "INTEGER", nullable: false),
                    ElderlyWomen = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonsWithDisabilities = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityParticipants_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: false),
                    OldStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NewStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityStatusHistories_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ActivityCategories",
                columns: new[] { "Id", "Code", "DisplayOrder", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "CPR", 1, true, "Community Participation & Representation" },
                    { 2, "SSCM", 2, true, "Site Service Coordination & Monitoring" },
                    { 3, "TWISFC", 3, true, "Two-Way Information Sharing, Feedback & Complaints" },
                    { 4, "SEI", 4, true, "Site Environment Improvements" }
                });

            migrationBuilder.InsertData(
                table: "ActivityTypes",
                columns: new[] { "Id", "ActivityCategoryId", "AllowsMultipleTopics", "Code", "DisplayOrder", "IsActive", "Name", "RequiresAttachment", "RequiresItems", "RequiresPWD", "RequiresParticipants", "RequiresSummary", "RequiresTopic" },
                values: new object[,]
                {
                    { 1, 1, false, "CAPACITY_STRENGTHENING", 1, true, "Capacity Strengthening for Community Members", false, false, false, false, false, false },
                    { 2, 1, false, "COMMUNITY_COMMITTEE_MEETING", 2, true, "Community or Committee Meeting", false, false, false, false, false, false },
                    { 3, 2, false, "SAFETY_AUDIT", 1, true, "Safety Audit", false, false, false, false, false, false },
                    { 4, 3, false, "ADVOCACY_REPORT", 1, true, "Advocacy Report", false, false, false, false, false, false },
                    { 5, 3, false, "AWARENESS_SESSION", 2, true, "Awareness Session", false, false, false, false, false, false },
                    { 6, 2, false, "SITE_IMPROVEMENT_PLAN", 2, true, "Site Improvement Plan", false, false, false, false, false, false },
                    { 7, 4, false, "SITE_IMPROVEMENTS_CARE_MAINTENANCE", 1, true, "Site Improvements, Care & Maintenance", false, false, false, false, false, false },
                    { 8, 1, false, "COMMUNITY_LED_ACTIVITY", 3, true, "Community-Led Activity", false, false, false, false, false, false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ActivityTypeId",
                table: "Activities",
                column: "ActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ProjectSiteId",
                table: "Activities",
                column: "ProjectSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCategories_Code",
                table: "ActivityCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityParticipants_ActivityId",
                table: "ActivityParticipants",
                column: "ActivityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityStatusHistories_ActivityId",
                table: "ActivityStatusHistories",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_ActivityCategoryId",
                table: "ActivityTypes",
                column: "ActivityCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_Code",
                table: "ActivityTypes",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityParticipants");

            migrationBuilder.DropTable(
                name: "ActivityStatusHistories");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "ActivityTypes");

            migrationBuilder.DropTable(
                name: "ActivityCategories");
        }
    }
}

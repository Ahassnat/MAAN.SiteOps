using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAAN.SiteOps.Migrations
{
    /// <inheritdoc />
    public partial class AddFlexibleReportingFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityProjectAttributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectSiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    AttributionRole = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityProjectAttributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityProjectAttributions_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityProjectAttributions_ProjectSites_ProjectSiteId",
                        column: x => x.ProjectSiteId,
                        principalTable: "ProjectSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Indicators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    GlobalCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    UnitType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    CalculationMode = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DataSourceType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indicators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourceSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceSystems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectIndicators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    IndicatorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectIndicatorCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TargetValue = table.Column<decimal>(type: "TEXT", nullable: true),
                    TargetMen = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetWomen = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetBoys = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetGirls = table.Column<int>(type: "INTEGER", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MeansOfVerification = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectIndicators_Indicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalTable: "Indicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectIndicators_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceSystemId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    TotalRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    LinkedRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    SkippedRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportBatches_SourceSystems_SourceSystemId",
                        column: x => x.SourceSystemId,
                        principalTable: "SourceSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityIndicatorContributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectIndicatorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", nullable: true),
                    Men = table.Column<int>(type: "INTEGER", nullable: true),
                    Women = table.Column<int>(type: "INTEGER", nullable: true),
                    Boys = table.Column<int>(type: "INTEGER", nullable: true),
                    Girls = table.Column<int>(type: "INTEGER", nullable: true),
                    PersonsWithDisabilities = table.Column<int>(type: "INTEGER", nullable: true),
                    ContributionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsManual = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityIndicatorContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityIndicatorContributions_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityIndicatorContributions_ProjectIndicators_ProjectIndicatorId",
                        column: x => x.ProjectIndicatorId,
                        principalTable: "ProjectIndicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivitySourceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceSystemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportBatchId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExternalRecordId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    RecordHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    SourceSiteCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SourceActivityType = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SourceDonorLabel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SourceProjectLabel = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SourceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RawPayloadJson = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitySourceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivitySourceRecords_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ActivitySourceRecords_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivitySourceRecords_SourceSystems_SourceSystemId",
                        column: x => x.SourceSystemId,
                        principalTable: "SourceSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityIndicatorContributions_ActivityId",
                table: "ActivityIndicatorContributions",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityIndicatorContributions_ProjectIndicatorId",
                table: "ActivityIndicatorContributions",
                column: "ProjectIndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityProjectAttributions_ActivityId_ProjectSiteId",
                table: "ActivityProjectAttributions",
                columns: new[] { "ActivityId", "ProjectSiteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityProjectAttributions_ProjectSiteId",
                table: "ActivityProjectAttributions",
                column: "ProjectSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySourceRecords_ActivityId",
                table: "ActivitySourceRecords",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySourceRecords_ImportBatchId",
                table: "ActivitySourceRecords",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySourceRecords_SourceSystemId_ExternalRecordId",
                table: "ActivitySourceRecords",
                columns: new[] { "SourceSystemId", "ExternalRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_SourceSystemId",
                table: "ImportBatches",
                column: "SourceSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectIndicators_IndicatorId",
                table: "ProjectIndicators",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectIndicators_ProjectId_ProjectIndicatorCode",
                table: "ProjectIndicators",
                columns: new[] { "ProjectId", "ProjectIndicatorCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SourceSystems_Code",
                table: "SourceSystems",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityIndicatorContributions");

            migrationBuilder.DropTable(
                name: "ActivityProjectAttributions");

            migrationBuilder.DropTable(
                name: "ActivitySourceRecords");

            migrationBuilder.DropTable(
                name: "ProjectIndicators");

            migrationBuilder.DropTable(
                name: "ImportBatches");

            migrationBuilder.DropTable(
                name: "Indicators");

            migrationBuilder.DropTable(
                name: "SourceSystems");
        }
    }
}

using MAAN.SiteOps.Data;
using MAAN.SiteOps.Models;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Services;

public sealed class ReferenceDataInitializer
{
    private const string CurrentProjectCode = "CBPF-OPT-25-R-NGO-38867";

    private static readonly (string Code, string Name)[] SourceSystems =
    [
        ("MANUAL", "Manual Entry"),
        ("ZITE", "Zite"),
        ("4W", "4W"),
        ("KOBO", "Kobo"),
        ("API", "API")
    ];

    private static readonly (string Code, string? GlobalCode, string Name, decimal Target, string Unit, string Calculation, string DataSource, int? Men, int? Women, int? Boys, int? Girls, string? MeansOfVerification)[] ProposalIndicators =
    [
        ("Outcome 1.1", null, "% of people in sites where monitoring has been conducted with updated demographic information and needs shared within the response coordination structure", 100, "Percentage", "Percentage", "Mixed", null, null, null, null, null),
        ("Outcome 1.2", null, "% of people living in sites managed or supported by trained Site Management teams", 100, "Percentage", "Percentage", "Population", null, null, null, null, null),
        ("Outcome 1.3", null, "% of targeted population reporting satisfaction with site management services", 80, "Percentage", "Percentage", "Survey", null, null, null, null, null),
        ("1.1.1", null, "Number of new sites located, recorded and responded to by cluster partners", 25, "Sites", "DistinctSites", "SiteStatus", null, null, null, null, "Site management deployment records; site monitoring reports; SMWG reporting tools; project progress reports; 4Ws"),
        ("1.1.2", null, "Number of Site level gaps analysis developed and updated regularly", 25, "Sites", "DistinctSites", "SiteStatus", null, null, null, null, "Site gap analysis reports; site monitoring reports; service mapping records; project and SMWG coordination reports; 4Ws"),
        ("1.1.3", null, "Number of people in sites managed or supported by trained Site Management teams", 35329, "People", "DistinctPeople", "Population", 9520, 9440, 8223, 8146, "Site population tracking data; site monitoring reports; site management coverage records; site management information system; project and SMWG reports; 4Ws"),
        ("Outcome 2.1", null, "% of sites with protection referral pathway in place", 100, "Percentage", "Percentage", "SiteStatus", null, null, null, null, null),
        ("Outcome 2.2", null, "Sites with inclusive committees increases", 25, "Sites", "DistinctSites", "SiteStatus", null, null, null, null, null),
        ("Outcome 2.3", null, "% of reported issues or concerns received through accountability mechanisms that are addressed or referred to relevant actors", 60, "Percentage", "Percentage", "Mixed", null, null, null, null, null),
        ("2.1.1", null, "Number of sites with effective inclusive committees", 25, "Sites", "DistinctSites", "SiteStatus", null, null, null, null, "SMC membership lists; SMC meeting minutes and attendance records; community governance assessment reports; 4Ws"),
        ("2.1.2", "CM.5", "Number of people receiving training on CCCM", 150, "People", "DistinctPeople", "Activity", 100, 50, 0, 0, "Training attendance sheets and participant lists; training reports and session documentation; training materials and agendas"),
        ("2.2.1", "AP.2b", "Percentage of affected people who state that they are aware of feedback and complaints mechanisms established for their use", 100, "Percentage", "Percentage", "Survey", null, null, null, null, "Community feedback surveys; focus group discussions; Help Desk or outreach awareness monitoring records"),
        ("2.2.2", null, "Number of targeted sites with operational feedback and complaint mechanisms", 25, "Sites", "DistinctSites", "SiteStatus", null, null, null, null, "Site monitoring reports; feedback mechanism records; Help Desk logs; complaint registers; feedback box records; WhatsApp/SMS logs; 4Ws"),
        ("2.3.1", null, "Number of sites with protection referral pathway in place", 25, "Sites", "DistinctSites", "SiteStatus", null, null, null, null, "Referral pathway matrices; site monitoring reports; referral records"),
        ("Outcome 3.1", null, "% of people in displacement sites with improvements made in site planning, facilities and/or infrastructure either directly or through community-led projects", 100, "Percentage", "Percentage", "Mixed", null, null, null, null, null),
        ("3.1.1", "CM.3", "Number of displacement sites with physical site improvements", 25, "Sites", "DistinctSites", "Activity", null, null, null, null, "Site improvement records; site monitoring and assessment reports; engineering/technical supervision reports; photographic documentation; 4Ws"),
        ("3.1.2", null, "Number of people receiving Conditional cash transfers", 60, "People", "DistinctPeople", "Financial", null, null, null, null, "CfW beneficiary registration lists; participant databases; attendance/work logs; completion/supervision reports"),
        ("3.1.3", null, "Total value of Conditional cash transfers distributed in USD", 22500, "USD", "Sum", "Financial", null, null, null, null, "Payment records")
    ];

    private static readonly (string Code, string Name, string Frequency)[] ReportDefinitions =
    [
        ("SM_CLUSTER_MONTHLY", "SM Cluster Monthly Activity Reporting", "Monthly"),
        ("4W", "4W Activity Reporting", "Monthly"),
        ("MAAN_INTERNAL", "MA'AN Internal Operational Reporting", "As Needed")
    ];

    public async Task InitializeAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        foreach (var (code, name) in SourceSystems)
        {
            if (!await context.SourceSystems.AnyAsync(source => source.Code == code, cancellationToken))
            {
                context.SourceSystems.Add(new SourceSystem
                {
                    Code = code,
                    Name = name,
                    IsActive = true
                });
            }
        }

        var category = await context.ActivityCategories
            .SingleOrDefaultAsync(
                activityCategory => activityCategory.Name == "Site Environment Improvements",
                cancellationToken);

        if (category is not null && !await context.ActivityTypes.AnyAsync(
                activityType => activityType.Code == "COMMUNITY_LED_ACTIVITY_PLAN" ||
                                activityType.Name == "Community-Led Activity Plan",
                cancellationToken))
        {
            context.ActivityTypes.Add(new ActivityType
            {
                ActivityCategoryId = category.Id,
                Code = "COMMUNITY_LED_ACTIVITY_PLAN",
                Name = "Community-Led Activity Plan",
                RequiresParticipants = false,
                RequiresPWD = false,
                RequiresTopic = false,
                AllowsMultipleTopics = false,
                RequiresItems = false,
                RequiresAttachment = true,
                RequiresSummary = false,
                DisplayOrder = 2,
                IsActive = true
            });
        }

        await EnsurePrimaryAttributionsAsync(context, cancellationToken);
        await EnsureReportDefinitionsAsync(context, cancellationToken);
        await EnsureProposalIndicatorsAsync(context, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureReportDefinitionsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var definition in ReportDefinitions)
        {
            if (!await context.ReportDefinitions.AnyAsync(x => x.Code == definition.Code, cancellationToken))
            {
                context.ReportDefinitions.Add(new ReportDefinition
                {
                    Code = definition.Code,
                    Name = definition.Name,
                    Frequency = definition.Frequency,
                    IsActive = true
                });
            }
        }
    }

    private static async Task EnsureProposalIndicatorsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var project = await context.Projects.SingleOrDefaultAsync(x => x.ProjectCode == CurrentProjectCode, cancellationToken);
        if (project is null)
            return;

        foreach (var definition in ProposalIndicators)
        {
            var indicator = await context.Indicators.SingleOrDefaultAsync(
                x => definition.GlobalCode != null && x.GlobalCode == definition.GlobalCode ||
                     definition.GlobalCode == null && x.Name == definition.Name,
                cancellationToken);

            if (indicator is null)
            {
                indicator = new Indicator
                {
                    Name = definition.Name,
                    GlobalCode = definition.GlobalCode,
                    UnitType = definition.Unit,
                    CalculationMode = definition.Calculation,
                    DataSourceType = definition.DataSource,
                    IsActive = true
                };
                context.Indicators.Add(indicator);
                await context.SaveChangesAsync(cancellationToken);
            }

            if (!await context.ProjectIndicators.AnyAsync(
                    x => x.ProjectId == project.Id && x.ProjectIndicatorCode == definition.Code,
                    cancellationToken))
            {
                context.ProjectIndicators.Add(new ProjectIndicator
                {
                    ProjectId = project.Id,
                    IndicatorId = indicator.Id,
                    ProjectIndicatorCode = definition.Code,
                    TargetValue = definition.Target,
                    TargetMen = definition.Men,
                    TargetWomen = definition.Women,
                    TargetBoys = definition.Boys,
                    TargetGirls = definition.Girls,
                    MeansOfVerification = definition.MeansOfVerification,
                    IsActive = true
                });
            }
        }
    }

    private static async Task EnsurePrimaryAttributionsAsync(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var activities = await context.Activities
            .Include(activity => activity.ProjectAttributions)
            .ToListAsync(cancellationToken);

        foreach (var activity in activities)
        {
            var primaryAttribution = activity.ProjectAttributions
                .FirstOrDefault(attribution => attribution.ProjectSiteId == activity.ProjectSiteId);

            if (primaryAttribution is null)
            {
                primaryAttribution = new ActivityProjectAttribution
                {
                    ActivityId = activity.Id,
                    ProjectSiteId = activity.ProjectSiteId,
                    IsPrimary = true,
                    AttributionRole = "Primary"
                };
                context.ActivityProjectAttributions.Add(primaryAttribution);
            }

            foreach (var attribution in activity.ProjectAttributions)
                attribution.IsPrimary = attribution == primaryAttribution;
        }
    }
}
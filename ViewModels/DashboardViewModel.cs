using MAAN.SiteOps.Models;

namespace MAAN.SiteOps.ViewModels;

public class DashboardViewModel
{
    public int TotalActivities { get; set; }
    public int DraftActivities { get; set; }
    public int PendingReviewActivities { get; set; }
    public int ReturnedActivities { get; set; }
    public int ApprovedActivities { get; set; }
    public int ActiveProjects { get; set; }
    public int ActiveSites { get; set; }

    public IReadOnlyList<DashboardStatusCount> StatusCounts { get; set; } = [];
    public IReadOnlyList<DashboardTypeCount> TypeCounts { get; set; } = [];
    public IReadOnlyList<Activity> RecentActivities { get; set; } = [];
    public IReadOnlyList<Activity> PendingActivities { get; set; } = [];
}

public class DashboardStatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardTypeCount
{
    public string ActivityType { get; set; } = string.Empty;
    public int Count { get; set; }
}

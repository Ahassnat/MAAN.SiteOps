namespace MAAN.SiteOps.Models;

public class Governorate
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Site> Sites { get; set; } = new List<Site>();
}
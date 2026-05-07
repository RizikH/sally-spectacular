namespace RepairTracker.Models;

public class Episode
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public int EpisodeNumber { get; set; }
    public string ItemDescription { get; set; } = "";
    public double Cost { get; set; }
    public double Parts { get; set; }
    public double? EstSellPrice { get; set; }
    public double? ActualSellPrice { get; set; }
    public double Postage { get; set; }
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
}

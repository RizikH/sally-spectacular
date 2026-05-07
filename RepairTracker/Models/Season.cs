namespace RepairTracker.Models;

public class Season
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public int EpisodeCount { get; set; }
    public double InitialInvestment { get; set; }
}

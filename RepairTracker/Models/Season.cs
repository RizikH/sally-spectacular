namespace RepairTracker.Models;

public class Season
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public int EpisodeCount { get; set; }
    public double InitialInvestment { get; set; }
    public string? DeletedAt { get; set; }

    public int DaysUntilPurge =>
        DeletedAt == null ? 30
        : Math.Max(0, 30 - (int)(DateTime.UtcNow - DateTime.Parse(DeletedAt, null,
            System.Globalization.DateTimeStyles.RoundtripKind)).TotalDays);
}

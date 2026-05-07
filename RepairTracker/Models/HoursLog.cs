namespace RepairTracker.Models;

public class HoursLog
{
    public int Id { get; set; }
    public int EpisodeId { get; set; }
    public double HoursWorked { get; set; }
    public string? Notes { get; set; }
}

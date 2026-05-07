using RepairTracker.Database;
using RepairTracker.Helpers;

namespace RepairTracker.Forms;

public class AppForm : Form
{
    private readonly Panel pnlContent;

    public AppForm()
    {
        Text = "Repair Tracker";
        Size = new Size(1220, 720);
        MinimumSize = new Size(1000, 580);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = new Font("Segoe UI", 9f);

        pnlContent = new Panel { Dock = DockStyle.Fill };
        Controls.Add(pnlContent);
    }

    public void Navigate(UserControl page)
    {
        var old = pnlContent.Controls.Count > 0 ? pnlContent.Controls[0] : null;
        pnlContent.Controls.Clear();
        old?.Dispose();

        page.Dock = DockStyle.Fill;
        pnlContent.Controls.Add(page);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        string? lastId = DbContext.GetAppState("last_season_id");
        if (lastId != null && int.TryParse(lastId, out int sid))
        {
            var seasons = DbContext.GetAllSeasons();
            var season = seasons.FirstOrDefault(s => s.Id == sid);
            if (season != null)
            {
                Navigate(new SeasonViewControl(this, season));
                return;
            }
        }

        Navigate(new MainMenuControl(this));
    }
}

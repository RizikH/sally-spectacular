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

        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        pnlContent = new Panel { Dock = DockStyle.Fill };
        Controls.Add(pnlContent);
    }

    public void Navigate(UserControl page)
    {
        var old = pnlContent.Controls.Count > 0 ? pnlContent.Controls[0] : null;
        pnlContent.Controls.Clear();
        page.Dock = DockStyle.Fill;
        pnlContent.Controls.Add(page);

        // Defer disposal so the caller's event handler finishes unwinding first.
        // Disposing synchronously crashes because Navigate is called from within
        // the old control's own button click, which is still on the call stack.
        if (old != null) BeginInvoke(() => old.Dispose());
    }

    protected override void OnLoad(EventArgs e)
    {
        RestoreWindowSize();
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

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (WindowState == FormWindowState.Normal)
        {
            DbContext.SetAppState("window_width",  Width.ToString());
            DbContext.SetAppState("window_height", Height.ToString());
        }
        DbContext.SetAppState("window_state", WindowState == FormWindowState.Maximized ? "Maximized" : "Normal");
    }

    private void RestoreWindowSize()
    {
        string? state = DbContext.GetAppState("window_state");
        string? w     = DbContext.GetAppState("window_width");
        string? h     = DbContext.GetAppState("window_height");

        if (w != null && h != null
            && int.TryParse(w, out int width) && int.TryParse(h, out int height))
        {
            Size = new Size(
                Math.Max(width,  MinimumSize.Width),
                Math.Max(height, MinimumSize.Height));
        }

        if (state == "Maximized")
            WindowState = FormWindowState.Maximized;
    }
}

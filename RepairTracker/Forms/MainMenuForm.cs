using RepairTracker.Database;
using RepairTracker.Helpers;
using RepairTracker.Models;

namespace RepairTracker.Forms;

public class MainMenuForm : Form
{
    private FlowLayoutPanel pnlCards = null!;
    private Button btnDeleted = null!;

    public MainMenuForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Repair Tracker";
        Size = new Size(960, 680);
        MinimumSize = new Size(600, 400);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = new Font("Segoe UI", 9f);

        // Header
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = AppColors.Surface };
        var lblTitle = AppColors.MakeLabel("REPAIR TRACKER", 18f, bold: true, color: AppColors.TextPrimary);
        lblTitle.Location = new Point(24, 16);
        var lblSub = AppColors.MakeLabel("Track your flips, season by season.", 9.5f, color: AppColors.TextSecond);
        lblSub.Location = new Point(26, 48);
        pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

        // Toolbar
        var pnlBar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = AppColors.Card };
        var btnNew = AppColors.MakeBtn("+ New Season", AppColors.Accent);
        btnNew.Width = 130; btnNew.Height = 34; btnNew.Location = new Point(14, 9);
        btnNew.Click += BtnNew_Click;

        var btnAllProfit = AppColors.MakeBtn("Total Profit — All Seasons", AppColors.Card);
        btnAllProfit.Width = 190; btnAllProfit.Height = 34;
        btnAllProfit.FlatAppearance.BorderSize = 1;
        btnAllProfit.FlatAppearance.BorderColor = AppColors.Border;
        btnAllProfit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAllProfit.Click += (_, _) => new ProfitSummaryForm().ShowDialog(this);

        var btnSeasonProfit = AppColors.MakeBtn("Season Profit...", AppColors.Card);
        btnSeasonProfit.Width = 130; btnSeasonProfit.Height = 34;
        btnSeasonProfit.FlatAppearance.BorderSize = 1;
        btnSeasonProfit.FlatAppearance.BorderColor = AppColors.Border;
        btnSeasonProfit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSeasonProfit.Click += BtnSeasonProfit_Click;

        btnDeleted = AppColors.MakeBtn("🗑 Recently Deleted", AppColors.Card);
        btnDeleted.Width = 155; btnDeleted.Height = 34;
        btnDeleted.FlatAppearance.BorderSize = 1;
        btnDeleted.FlatAppearance.BorderColor = AppColors.Border;
        btnDeleted.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnDeleted.Click += (_, _) =>
        {
            using var dlg = new DeletedSeasonsForm();
            dlg.ShowDialog(this);
            LoadSeasons();
        };

        pnlBar.SizeChanged += (_, _) =>
        {
            btnDeleted.Location      = new Point(pnlBar.Width - 169, 9);
            btnSeasonProfit.Location = new Point(pnlBar.Width - 313, 9);
            btnAllProfit.Location    = new Point(pnlBar.Width - 507, 9);
        };

        pnlBar.Controls.AddRange(new Control[] { btnNew, btnAllProfit, btnSeasonProfit, btnDeleted });

        // Cards scroll area
        pnlCards = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(16, 16, 0, 16),
            BackColor = AppColors.Background
        };

        Controls.Add(pnlCards);
        Controls.Add(pnlBar);
        Controls.Add(pnlHeader);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Auto-navigate to last season if one exists
        string? lastId = DbContext.GetAppState("last_season_id");
        if (lastId != null && int.TryParse(lastId, out int sid))
        {
            var seasons = DbContext.GetAllSeasons();
            var season = seasons.FirstOrDefault(s => s.Id == sid);
            if (season != null)
            {
                OpenSeason(season);
                return;
            }
        }

        RefreshAndShow();
    }

    public void RefreshAndShow()
    {
        Show();
        LoadSeasons();
    }

    private void LoadSeasons()
    {
        string? lastId = DbContext.GetAppState("last_season_id");
        _ = int.TryParse(lastId, out int lastSid);

        pnlCards.Controls.Clear();
        var seasons = DbContext.GetAllSeasons();

        // Update "Recently Deleted" badge
        int deletedCount = DbContext.GetDeletedSeasons().Count;
        btnDeleted.Text = deletedCount > 0
            ? $"🗑 Recently Deleted ({deletedCount})"
            : "🗑 Recently Deleted";

        if (seasons.Count == 0)
        {
            var lbl = AppColors.MakeLabel("No seasons yet. Click \"+ New Season\" to get started.", 10f, color: AppColors.TextMuted);
            lbl.Margin = new Padding(8, 8, 0, 0);
            pnlCards.Controls.Add(lbl);
            return;
        }

        foreach (var s in seasons)
        {
            pnlCards.Controls.Add(BuildCard(s, s.Id == lastSid));
        }
    }

    private Panel BuildCard(Season season, bool isActive)
    {
        var card = new Panel
        {
            Width = 190,
            Height = 130,
            Margin = new Padding(0, 0, 14, 14),
            BackColor = AppColors.Card,
            Cursor = Cursors.Hand
        };

        var lblName = AppColors.MakeLabel(season.Name, 12f, bold: true);
        lblName.Location = new Point(14, 14);
        lblName.MaximumSize = new Size(162, 0);

        var lblCount = AppColors.MakeLabel(
            $"{season.EpisodeCount} episode{(season.EpisodeCount == 1 ? "" : "s")}",
            9f, color: AppColors.TextMuted);
        lblCount.Location = new Point(14, 38);

        var btnView = AppColors.MakeBtn("View", AppColors.Accent);
        btnView.Width = 76; btnView.Height = 30; btnView.Location = new Point(14, 86);
        btnView.Click += (_, _) => OpenSeason(season);

        var btnHours = AppColors.MakeBtn("⏱ Hours", AppColors.Card);
        btnHours.Width = 80; btnHours.Height = 30; btnHours.Location = new Point(96, 86);
        btnHours.FlatAppearance.BorderSize = 1;
        btnHours.FlatAppearance.BorderColor = AppColors.Border;
        btnHours.Click += (_, _) => OpenHours(season);

        // Delete button — top-right corner
        var btnDelete = new Button
        {
            Text = "×",
            Size = new Size(24, 24),
            Location = new Point(card.Width - 28, 4),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = AppColors.TextMuted,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.FlatAppearance.MouseOverBackColor = AppColors.RedBg;
        btnDelete.MouseEnter += (_, _) => btnDelete.ForeColor = AppColors.RedFg;
        btnDelete.MouseLeave += (_, _) => btnDelete.ForeColor = AppColors.TextMuted;
        btnDelete.Click += (_, _) => DeleteSeason(season);

        // Border
        card.Paint += (s, e) =>
        {
            using var pen = new Pen(isActive ? AppColors.BorderHL : AppColors.Border, isActive ? 2f : 1f);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        // Hover (don't override if hovering the delete button)
        card.MouseEnter += (_, _) => { card.BackColor = AppColors.CardHover; card.Invalidate(); };
        card.MouseLeave += (_, _) => { card.BackColor = AppColors.Card; card.Invalidate(); };

        card.Controls.AddRange(new Control[] { lblName, lblCount, btnView, btnHours, btnDelete });
        return card;
    }

    private void OpenSeason(Season season)
    {
        DbContext.SetAppState("last_season_id", season.Id.ToString());
        var form = new SeasonViewForm(season);
        form.FormClosed += (_, _) => RefreshAndShow();
        Hide();
        form.Show();
    }

    private void OpenHours(Season season)
    {
        DbContext.SetAppState("last_season_id", season.Id.ToString());
        var form = new HoursViewForm(season);
        form.FormClosed += (_, _) => RefreshAndShow();
        Hide();
        form.Show();
    }

    private void DeleteSeason(Season season)
    {
        var result = MessageBox.Show(
            $"Delete \"{season.Name}\"?\n\nYou can restore it within 30 days from \"Recently Deleted\".",
            "Delete Season",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes) return;

        DbContext.SoftDeleteSeason(season.Id);
        LoadSeasons();
    }

    private void BtnNew_Click(object? sender, EventArgs e)
    {
        using var dlg = new NewSeasonDialog();
        if (dlg.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dlg.SeasonName)) return;

        try
        {
            DbContext.CreateSeason(dlg.SeasonName.Trim(), dlg.InitialInvestment);
            LoadSeasons();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not create season: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void BtnSeasonProfit_Click(object? sender, EventArgs e)
    {
        var seasons = DbContext.GetAllSeasons();
        if (seasons.Count == 0) { MessageBox.Show("No seasons yet.", "Info"); return; }

        using var picker = new SeasonPickerDialog(seasons);
        if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedSeason == null) return;
        new ProfitSummaryForm(picker.SelectedSeason).ShowDialog(this);
    }
}

// ── Inline helper dialogs ─────────────────────────────────────────────────────

internal class NewSeasonDialog : Form
{
    public string SeasonName => txtName.Text;
    public double InitialInvestment { get; private set; }

    private TextBox txtName = null!, txtInvest = null!;

    public NewSeasonDialog()
    {
        Text = "New Season";
        Size = new Size(380, 240);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppColors.Surface;
        ForeColor = AppColors.TextPrimary;

        var lblName = AppColors.MakeLabel("Season name:", 9.5f);
        lblName.Location = new Point(20, 20);
        txtName = new TextBox
        {
            Location = new Point(20, 40), Width = 320,
            BackColor = AppColors.Card, ForeColor = AppColors.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f)
        };

        var lblInvest = AppColors.MakeLabel("Initial investment (£, optional — for pre-bought stock):", 9.5f, color: AppColors.TextSecond);
        lblInvest.Location = new Point(20, 76);
        txtInvest = new TextBox
        {
            Location = new Point(20, 96), Width = 150,
            BackColor = AppColors.Card, ForeColor = AppColors.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f),
            PlaceholderText = "0"
        };

        var btnOk = AppColors.MakeBtn("Create", AppColors.Accent);
        btnOk.Width = 90; btnOk.Height = 32; btnOk.Location = new Point(230, 152);
        btnOk.Click += (_, _) =>
        {
            InitialInvestment = double.TryParse(txtInvest.Text.Replace("£", "").Trim(), out double v) && v >= 0 ? v : 0;
            DialogResult = DialogResult.OK;
            Close();
        };

        var btnCancel = AppColors.MakeBtn("Cancel", AppColors.Card);
        btnCancel.Width = 80; btnCancel.Height = 32; btnCancel.Location = new Point(138, 152);
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.AddRange(new Control[] { lblName, txtName, lblInvest, txtInvest, btnOk, btnCancel });
        AcceptButton = btnOk; CancelButton = btnCancel;
    }
}

internal class SeasonPickerDialog : Form
{
    public Season? SelectedSeason { get; private set; }
    private ComboBox combo = null!;
    private readonly List<Season> _seasons;

    public SeasonPickerDialog(List<Season> seasons)
    {
        _seasons = seasons;
        Text = "Select Season";
        Size = new Size(340, 170);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppColors.Surface;
        ForeColor = AppColors.TextPrimary;

        var lbl = AppColors.MakeLabel("Show profit for:", 9.5f);
        lbl.Location = new Point(20, 22);

        combo = new ComboBox
        {
            Location = new Point(20, 44), Width = 280,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = AppColors.Card, ForeColor = AppColors.TextPrimary,
            Font = new Font("Segoe UI", 9.5f), FlatStyle = FlatStyle.Flat
        };
        combo.Items.AddRange(seasons.Select(s => s.Name).ToArray<object>());
        if (combo.Items.Count > 0) combo.SelectedIndex = 0;

        var btnOk = AppColors.MakeBtn("View", AppColors.Accent);
        btnOk.Width = 80; btnOk.Height = 32; btnOk.Location = new Point(200, 90);
        btnOk.Click += (_, _) =>
        {
            if (combo.SelectedIndex >= 0) SelectedSeason = _seasons[combo.SelectedIndex];
            DialogResult = DialogResult.OK; Close();
        };
        var btnCancel = AppColors.MakeBtn("Cancel", AppColors.Card);
        btnCancel.Width = 80; btnCancel.Height = 32; btnCancel.Location = new Point(108, 90);
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.AddRange(new Control[] { lbl, combo, btnOk, btnCancel });
        AcceptButton = btnOk; CancelButton = btnCancel;
    }
}

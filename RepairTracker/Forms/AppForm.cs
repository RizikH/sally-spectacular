using RepairTracker.Database;
using RepairTracker.Helpers;

namespace RepairTracker.Forms;

public class AppForm : Form
{
    private readonly Panel pnlContent;
    private Panel _pnlTheme = null!;
    private Button _btnThemeSys = null!, _btnThemeDark = null!, _btnThemeLight = null!;
    private Func<UserControl>? _pageFactory;

    public AppForm()
    {
        ThemeManager.LoadSaved();

        Text = "Repair Tracker";
        Size = new Size(1220, 720);
        MinimumSize = new Size(1000, 580);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = new Font("Segoe UI", 9f);

        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        pnlContent = new Panel { Dock = DockStyle.Fill };

        BuildThemeBar();

        Controls.Add(pnlContent);
        Controls.Add(_pnlTheme);

        ThemeManager.ThemeChanged += OnThemeChanged;
    }

    public void Navigate(UserControl page, Func<UserControl>? factory = null)
    {
        if (factory != null) _pageFactory = factory;

        var old = pnlContent.Controls.Count > 0 ? pnlContent.Controls[0] : null;
        pnlContent.Controls.Clear();
        page.Dock = DockStyle.Fill;
        pnlContent.Controls.Add(page);

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
                Navigate(new SeasonViewControl(this, season), () => new SeasonViewControl(this, season));
                return;
            }
        }

        Navigate(new MainMenuControl(this), () => new MainMenuControl(this));
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

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        ThemeManager.ThemeChanged -= OnThemeChanged;
        base.OnFormClosed(e);
    }

    private void OnThemeChanged()
    {
        if (InvokeRequired) { Invoke(OnThemeChanged); return; }
        BackColor = AppColors.Background;
        RefreshThemeBar();
        if (_pageFactory != null) Navigate(_pageFactory());
    }

    private void BuildThemeBar()
    {
        _pnlTheme = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = AppColors.StatusBar };

        var lbl = new Label
        {
            Text = "Theme:",
            ForeColor = AppColors.TextMuted,
            Font = new Font("Segoe UI", 9f),
            AutoSize = false,
            Width = 58,
            Height = 48,
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(4, 0)
        };

        _btnThemeSys   = MakeThemeBtn("⚙ System", ThemePreference.System);
        _btnThemeDark  = MakeThemeBtn("🌙 Dark",   ThemePreference.Dark);
        _btnThemeLight = MakeThemeBtn("☀ Light",   ThemePreference.Light);

        _btnThemeSys.Location   = new Point(68, 8);
        _btnThemeDark.Location  = new Point(174, 8);
        _btnThemeLight.Location = new Point(274, 8);

        _pnlTheme.Controls.AddRange(new Control[] { lbl, _btnThemeSys, _btnThemeDark, _btnThemeLight });
        RefreshThemeBar();
    }

    private static Button MakeThemeBtn(string text, ThemePreference pref)
    {
        var btn = new Button
        {
            Text = text,
            Width = 98,
            Height = 32,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        btn.FlatAppearance.BorderSize = 1;
        btn.Click += (_, _) => ThemeManager.SetPreference(pref);
        return btn;
    }

    private void RefreshThemeBar()
    {
        _pnlTheme.BackColor = AppColors.StatusBar;
        StyleThemeBtn(_btnThemeSys,   ThemePreference.System);
        StyleThemeBtn(_btnThemeDark,  ThemePreference.Dark);
        StyleThemeBtn(_btnThemeLight, ThemePreference.Light);
    }

    private void StyleThemeBtn(Button btn, ThemePreference pref)
    {
        bool active = ThemeManager.Preference == pref;
        btn.BackColor = active ? AppColors.Accent : AppColors.Card;
        btn.ForeColor = active ? Color.White : AppColors.TextSecond;
        btn.FlatAppearance.BorderColor = active ? AppColors.AccentDark : AppColors.Border;
        btn.FlatAppearance.MouseOverBackColor = active ? AppColors.AccentDark : AppColors.CardHover;
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

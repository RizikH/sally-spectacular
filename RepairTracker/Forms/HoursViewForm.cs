using RepairTracker.Database;
using RepairTracker.Helpers;
using RepairTracker.Logic;
using RepairTracker.Models;

namespace RepairTracker.Forms;

public class HoursViewControl : UserControl
{
    private const int ColEp = 0, ColItems = 1, ColEstProfit = 2, ColActProfit = 3,
                      ColHours = 4, ColHourly = 5, ColNotes = 6;

    private readonly AppForm _app;
    private readonly Season _season;

    private Dictionary<int, List<Episode>> _grouped = new();
    private List<int> _epNums = new();
    private Dictionary<int, HoursLog> _hoursMap = new();

    private DataGridView dgv = null!;
    private Panel pnlSummary = null!;

    public HoursViewControl(AppForm app, Season season)
    {
        _app = app;
        _season = season;
        _app.Text = $"Repair Tracker — {_season.Name} — Hours Log";
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = new Font("Segoe UI", 9f);

        // Header
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = AppColors.Surface };
        var btnBack = AppColors.MakeBtn("← Season", AppColors.Card);
        btnBack.Width = 100; btnBack.Height = 34; btnBack.Location = new Point(10, 12);
        btnBack.Click += (_, _) => _app.Navigate(new SeasonViewControl(_app, _season));
        var lblTitle = AppColors.MakeLabel($"{_season.Name} — Hours Log", 13f, bold: true);
        lblTitle.Location = new Point(122, 18);
        pnlHeader.Controls.AddRange(new Control[] { btnBack, lblTitle });

        // Summary panel (bottom)
        pnlSummary = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 72,
            BackColor = AppColors.StatusBar,
            Padding = new Padding(10, 8, 10, 8)
        };

        // Grid
        dgv = new DataGridView { Dock = DockStyle.Fill };
        AppColors.StyleGrid(dgv);

        AddCol("Ep",             50, false, DataGridViewContentAlignment.MiddleCenter);
        AddCol("Items",          55, false, DataGridViewContentAlignment.MiddleCenter);
        AddCol("Est. Profit",   115, false, DataGridViewContentAlignment.MiddleRight);
        AddCol("Actual Profit", 120, false, DataGridViewContentAlignment.MiddleRight);
        AddCol("Hrs Worked",    100, true,  DataGridViewContentAlignment.MiddleCenter);
        AddCol("Hourly Profit", 120, false, DataGridViewContentAlignment.MiddleRight);
        AddCol("Notes",         250, true,  DataGridViewContentAlignment.MiddleLeft);

        dgv.Columns[ColNotes].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        dgv.EditingControlShowing += Grid_EditingControlShowing;
        dgv.CellEndEdit           += Grid_CellEndEdit;
        dgv.CellFormatting        += Grid_CellFormatting;

        Controls.Add(dgv);
        Controls.Add(pnlSummary);
        Controls.Add(pnlHeader);
    }

    private void AddCol(string header, int width, bool editable, DataGridViewContentAlignment align)
    {
        var col = new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            Width = width,
            ReadOnly = !editable,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        col.DefaultCellStyle.Alignment = align;
        col.HeaderCell.Style.Alignment = align;
        dgv.Columns.Add(col);
    }

    private void LoadData()
    {
        var allEps = DbContext.GetEpisodesForSeason(_season.Id);

        _grouped = allEps
            .GroupBy(e => e.EpisodeNumber)
            .ToDictionary(g => g.Key, g => g.ToList());
        _epNums = _grouped.Keys.OrderBy(n => n).ToList();

        var logs = DbContext.GetHoursLogsForSeason(_season.Id);
        _hoursMap = logs.ToDictionary(h => h.EpisodeNumber);

        dgv.Rows.Clear();
        foreach (var _ in _epNums)
        {
            dgv.Rows.Add();
            RefreshRow(dgv.Rows.Count - 1);
        }

        UpdateSummary();
    }

    private void RefreshRow(int i)
    {
        if (i < 0 || i >= _epNums.Count) return;
        int epNum = _epNums[i];
        var items = _grouped[epNum];
        _hoursMap.TryGetValue(epNum, out var log);
        var row = dgv.Rows[i];

        row.Cells[ColEp].Value    = epNum;
        row.Cells[ColItems].Value = items.Count;

        var withEst = items.Where(e => e.EstSellPrice.HasValue).ToList();
        double? estP = withEst.Count > 0
            ? withEst.Sum(e => Calculations.EstimatedProfit(e.Cost, e.Parts, e.EstSellPrice!.Value))
            : null;
        row.Cells[ColEstProfit].Value = Calculations.Gbp(estP);
        row.Tag = null;

        var withAct = items.Where(e => e.ActualSellPrice.HasValue).ToList();
        double? actP = withAct.Count > 0
            ? withAct.Sum(e => Calculations.NetProfit(e.Cost, e.Parts, e.ActualSellPrice!.Value, e.Postage))
            : null;
        row.Cells[ColActProfit].Value = Calculations.Gbp(actP);

        double hours = log?.HoursWorked ?? 0;
        row.Cells[ColHours].Value = hours > 0 ? hours.ToString("F1") : "";

        double? hourly = (actP.HasValue && hours > 0)
            ? Calculations.HourlyProfit(actP.Value, hours)
            : null;
        row.Cells[ColHourly].Value = hourly.HasValue ? Calculations.Gbp(hourly.Value) + "/hr" : "-";

        row.Cells[ColNotes].Value = log?.Notes ?? "";

        row.Tag = (estP, actP, hourly);
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _epNums.Count) return;
        if (dgv.Rows[e.RowIndex].Tag is not ValueTuple<double?, double?, double?> tag) return;
        var (estP, actP, hourly) = tag;

        if (e.ColumnIndex == ColEstProfit && estP.HasValue)
            ApplyProfitStyle(e.CellStyle, estP.Value);
        else if (e.ColumnIndex == ColActProfit && actP.HasValue)
            ApplyProfitStyle(e.CellStyle, actP.Value);
        else if (e.ColumnIndex == ColHourly && hourly.HasValue)
            ApplyProfitStyle(e.CellStyle, hourly.Value);
    }

    private static void ApplyProfitStyle(DataGridViewCellStyle style, double value)
    {
        if (value < 0)
        {
            style.BackColor = AppColors.RedBg;
            style.ForeColor = AppColors.RedFg;
            style.SelectionBackColor = Color.FromArgb(130, 40, 40);
            style.SelectionForeColor = AppColors.RedFg;
        }
        else
        {
            style.BackColor = AppColors.GreenBg;
            style.ForeColor = AppColors.GreenFg;
            style.SelectionBackColor = Color.FromArgb(30, 100, 50);
            style.SelectionForeColor = AppColors.GreenFg;
        }
    }

    private void UpdateSummary()
    {
        pnlSummary.Controls.Clear();

        var allEps = _grouped.Values.SelectMany(x => x).ToList();

        double totalInvest = _season.InitialInvestment + allEps.Sum(e => e.Cost + e.Parts);
        double totalPostage = allEps.Sum(e => e.Postage);

        var withEst = allEps.Where(e => e.EstSellPrice.HasValue).ToList();
        double? estTotal = withEst.Count > 0
            ? withEst.Sum(e => Calculations.EstimatedProfit(e.Cost, e.Parts, e.EstSellPrice!.Value))
            : null;

        var withAct = allEps.Where(e => e.ActualSellPrice.HasValue).ToList();
        double? actTotal = withAct.Count > 0
            ? withAct.Sum(e => Calculations.NetProfit(e.Cost, e.Parts, e.ActualSellPrice!.Value, e.Postage))
            : null;

        double totalHours = _hoursMap.Values.Sum(h => h.HoursWorked);
        double? hourlyTotal = (actTotal.HasValue && totalHours > 0)
            ? Calculations.HourlyProfit(actTotal.Value, totalHours)
            : null;

        var items = new[]
        {
            ("Est. Total Profit",   Calculations.Gbp(estTotal)),
            ("Actual Total Profit", Calculations.Gbp(actTotal)),
            ("Total Hours",         totalHours > 0 ? $"{totalHours:F1} hrs" : "-"),
            ("Total Hourly Profit", hourlyTotal.HasValue ? Calculations.Gbp(hourlyTotal.Value) + "/hr" : "-"),
            ("Initial Investment",  Calculations.Gbp(totalInvest)),
            ("Postage Total",       Calculations.Gbp(totalPostage)),
        };

        int x = 0;
        foreach (var (label, value) in items)
        {
            pnlSummary.Controls.Add(new Label
            {
                Text = $"{label}\n{value}",
                AutoSize = false,
                Width = 148,
                Height = 56,
                Location = new Point(x, 0),
                ForeColor = AppColors.TextSecond,
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            });
            x += 150;
        }
    }

    private void Grid_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
    {
        if (dgv.CurrentCell == null) return;
        int col = dgv.CurrentCell.ColumnIndex;
        int row = dgv.CurrentCell.RowIndex;
        if (row < 0 || row >= _epNums.Count) return;

        int epNum = _epNums[row];
        _hoursMap.TryGetValue(epNum, out var log);

        if (e.Control is TextBox tb)
        {
            tb.BackColor = AppColors.Card;
            tb.ForeColor = AppColors.TextPrimary;
            tb.BorderStyle = BorderStyle.None;

            string raw = col switch
            {
                ColHours => log != null && log.HoursWorked > 0 ? log.HoursWorked.ToString("F1") : "",
                ColNotes => log?.Notes ?? "",
                _ => tb.Text
            };
            tb.Text = raw;
            if (col == ColHours) tb.SelectAll();
        }
    }

    private void Grid_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _epNums.Count) return;
        int epNum = _epNums[e.RowIndex];
        string raw = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString()?.Trim() ?? "";

        _hoursMap.TryGetValue(epNum, out var log);
        log ??= new HoursLog { SeasonId = _season.Id, EpisodeNumber = epNum };

        switch (e.ColumnIndex)
        {
            case ColHours:
                if (double.TryParse(raw, out double h) && h >= 0) log.HoursWorked = h;
                else return;
                break;
            case ColNotes:
                log.Notes = string.IsNullOrWhiteSpace(raw) ? null : raw;
                break;
            default:
                return;
        }

        DbContext.UpsertHoursLog(log);
        _hoursMap[epNum] = log;
        RefreshRow(e.RowIndex);
        UpdateSummary();
    }
}

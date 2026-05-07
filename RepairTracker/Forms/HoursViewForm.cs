using RepairTracker.Database;
using RepairTracker.Helpers;
using RepairTracker.Logic;
using RepairTracker.Models;

namespace RepairTracker.Forms;

public class HoursViewForm : Form
{
    private const int ColEp = 0, ColEstProfit = 1, ColActProfit = 2,
                      ColHours = 3, ColHourly = 4, ColNotes = 5;

    private readonly Season _season;
    private List<Episode> _episodes = new();
    private Dictionary<int, HoursLog> _hoursMap = new(); // episodeId → HoursLog
    private DataGridView dgv = null!;
    private Panel pnlSummary = null!;

    public HoursViewForm(Season season)
    {
        _season = season;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        Text = $"Hours Log — {_season.Name}";
        Size = new Size(1020, 620);
        MinimumSize = new Size(800, 450);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = new Font("Segoe UI", 9f);

        // Header
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = AppColors.Surface };
        var btnBack = AppColors.MakeBtn("← Season", AppColors.Card);
        btnBack.Width = 100; btnBack.Height = 34; btnBack.Location = new Point(10, 12);
        btnBack.Click += (_, _) => Close();
        var lblTitle = AppColors.MakeLabel($"{_season.Name} — Hours Log", 13f, bold: true);
        lblTitle.Location = new Point(122, 18);
        pnlHeader.Controls.AddRange(new Control[] { btnBack, lblTitle });

        // Summary panel (bottom)
        pnlSummary = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = AppColors.StatusBar, Padding = new Padding(12, 8, 12, 8) };

        // Grid
        dgv = new DataGridView { Dock = DockStyle.Fill };
        AppColors.StyleGrid(dgv);

        AddCol("Ep",              50, false, DataGridViewContentAlignment.MiddleCenter);
        AddCol("Est. Profit",    110, false, DataGridViewContentAlignment.MiddleRight);
        AddCol("Actual Profit",  120, false, DataGridViewContentAlignment.MiddleRight);
        AddCol("Hours Worked",   110, true,  DataGridViewContentAlignment.MiddleCenter);
        AddCol("Hourly Profit",  120, false, DataGridViewContentAlignment.MiddleRight);
        AddCol("Notes",          260, true,  DataGridViewContentAlignment.MiddleLeft);

        dgv.Columns[ColNotes].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        dgv.EditingControlShowing += Grid_EditingControlShowing;
        dgv.CellEndEdit           += Grid_CellEndEdit;

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
        _episodes = DbContext.GetEpisodesForSeason(_season.Id);
        var logs = DbContext.GetHoursLogsForSeason(_season.Id);
        _hoursMap = logs.ToDictionary(h => h.EpisodeId);

        dgv.Rows.Clear();
        foreach (var ep in _episodes)
        {
            dgv.Rows.Add();
            RefreshRow(dgv.Rows.Count - 1);
        }
        UpdateSummary();
    }

    private void RefreshRow(int i)
    {
        if (i < 0 || i >= _episodes.Count) return;
        var ep = _episodes[i];
        var row = dgv.Rows[i];
        _hoursMap.TryGetValue(ep.Id, out var log);

        row.Cells[ColEp].Value = ep.EpisodeNumber;

        // Est. Profit
        if (ep.EstSellPrice.HasValue)
        {
            double eP = Calculations.EstimatedProfit(ep.Cost, ep.Parts, ep.EstSellPrice.Value);
            row.Cells[ColEstProfit].Value = Calculations.Gbp(eP);
            CellFormatter.ApplyProfit(row.Cells[ColEstProfit], eP, triggered: true);
        }
        else
        {
            row.Cells[ColEstProfit].Value = "-";
            CellFormatter.Reset(row.Cells[ColEstProfit]);
        }

        // Actual Profit
        if (ep.ActualSellPrice.HasValue)
        {
            double nP = Calculations.NetProfit(ep.Cost, ep.Parts, ep.ActualSellPrice.Value, ep.Postage);
            row.Cells[ColActProfit].Value = Calculations.Gbp(nP);
            CellFormatter.ApplyProfit(row.Cells[ColActProfit], nP, triggered: true);
        }
        else
        {
            row.Cells[ColActProfit].Value = "-";
            CellFormatter.Reset(row.Cells[ColActProfit]);
        }

        // Hours & Hourly Profit
        if (log != null && log.HoursWorked > 0)
        {
            row.Cells[ColHours].Value = log.HoursWorked.ToString("F1");

            if (ep.ActualSellPrice.HasValue)
            {
                double nP = Calculations.NetProfit(ep.Cost, ep.Parts, ep.ActualSellPrice.Value, ep.Postage);
                double hP = Calculations.HourlyProfit(nP, log.HoursWorked);
                row.Cells[ColHourly].Value = Calculations.Gbp(hP) + "/hr";
                CellFormatter.ApplyProfit(row.Cells[ColHourly], hP, triggered: true);
            }
            else
            {
                row.Cells[ColHourly].Value = "-";
                CellFormatter.Reset(row.Cells[ColHourly]);
            }
        }
        else
        {
            row.Cells[ColHours].Value = log != null ? "0" : "";
            row.Cells[ColHourly].Value = "-";
            CellFormatter.Reset(row.Cells[ColHourly]);
        }

        row.Cells[ColNotes].Value = log?.Notes ?? "";
    }

    private void UpdateSummary()
    {
        pnlSummary.Controls.Clear();

        double totalCost = _episodes.Sum(e => e.Cost + e.Parts);
        double totalPostage = _episodes.Sum(e => e.Postage);

        var withEst = _episodes.Where(e => e.EstSellPrice.HasValue).ToList();
        double? estTotal = withEst.Count > 0
            ? withEst.Sum(e => Calculations.EstimatedProfit(e.Cost, e.Parts, e.EstSellPrice!.Value))
            : null;

        var withAct = _episodes.Where(e => e.ActualSellPrice.HasValue).ToList();
        double? actTotal = withAct.Count > 0
            ? withAct.Sum(e => Calculations.NetProfit(e.Cost, e.Parts, e.ActualSellPrice!.Value, e.Postage))
            : null;

        double totalHours = _hoursMap.Values.Sum(h => h.HoursWorked);
        double? hourlyTotal = actTotal.HasValue && totalHours > 0
            ? Calculations.HourlyProfit(actTotal.Value, totalHours)
            : null;

        var items = new[]
        {
            ("Est. Total Profit",   Calculations.Gbp(estTotal)),
            ("Actual Total Profit", Calculations.Gbp(actTotal)),
            ("Total Hours",         totalHours > 0 ? $"{totalHours:F1} hrs" : "-"),
            ("Total Hourly Profit", hourlyTotal.HasValue ? Calculations.Gbp(hourlyTotal) + "/hr" : "-"),
            ("Initial Investment",  Calculations.Gbp(totalCost)),
            ("Postage Total",       Calculations.Gbp(totalPostage)),
        };

        int x = 0;
        foreach (var (label, value) in items)
        {
            var lbl = new Label
            {
                Text = $"{label}\n{value}",
                AutoSize = false,
                Width = 140,
                Height = 60,
                Location = new Point(x, 0),
                ForeColor = AppColors.TextSecond,
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlSummary.Controls.Add(lbl);
            x += 142;
        }
    }

    private void Grid_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
    {
        if (dgv.CurrentCell == null) return;
        int col = dgv.CurrentCell.ColumnIndex;
        int row = dgv.CurrentCell.RowIndex;
        if (row < 0 || row >= _episodes.Count) return;
        var ep = _episodes[row];
        _hoursMap.TryGetValue(ep.Id, out var log);

        if (e.Control is TextBox tb)
        {
            tb.BackColor = AppColors.Card;
            tb.ForeColor = AppColors.TextPrimary;
            tb.BorderStyle = BorderStyle.None;

            string raw = col switch
            {
                ColHours => log != null ? log.HoursWorked.ToString("F1") : "",
                ColNotes => log?.Notes ?? "",
                _ => tb.Text
            };
            tb.Text = raw;
            if (col == ColHours) tb.SelectAll();
        }
    }

    private void Grid_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _episodes.Count) return;
        var ep = _episodes[e.RowIndex];
        string raw = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString()?.Trim() ?? "";

        _hoursMap.TryGetValue(ep.Id, out var log);
        log ??= new HoursLog { EpisodeId = ep.Id };

        switch (e.ColumnIndex)
        {
            case ColHours:
                if (double.TryParse(raw, out double h) && h >= 0) log.HoursWorked = h;
                break;
            case ColNotes:
                log.Notes = string.IsNullOrWhiteSpace(raw) ? null : raw;
                break;
            default:
                return;
        }

        DbContext.UpsertHoursLog(log);
        _hoursMap[ep.Id] = log;
        RefreshRow(e.RowIndex);
        UpdateSummary();
    }
}

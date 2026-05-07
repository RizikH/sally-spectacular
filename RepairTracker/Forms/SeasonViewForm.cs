using RepairTracker.Database;
using RepairTracker.Helpers;
using RepairTracker.Logic;
using RepairTracker.Models;

namespace RepairTracker.Forms;

public class SeasonViewControl : UserControl
{
    private const int ColEp = 0, ColItem = 1, ColCost = 2, ColParts = 3, ColEbayFee = 4,
                      ColEstSell = 5, ColEstProfit = 6, ColActSell = 7, ColPostage = 8,
                      ColNetProfit = 9, ColDelete = 10;

    private readonly AppForm _app;
    private readonly Season _season;
    private List<Episode> _episodes = new();
    private DataGridView dgv = null!;
    private Label lblSummary = null!;

    public SeasonViewControl(AppForm app, Season season)
    {
        _app = app;
        _season = season;
        _app.Text = $"Repair Tracker — {_season.Name}";
        InitializeComponent();
        LoadEpisodes();
    }

    private void InitializeComponent()
    {
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = new Font("Segoe UI", 9f);

        // Header
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = AppColors.Surface };

        var btnBack = AppColors.MakeBtn("← Seasons", AppColors.Card);
        btnBack.Width = 100; btnBack.Location = new Point(10, 12); btnBack.Height = 34;
        btnBack.Click += BtnBack_Click;

        var lblSeason = AppColors.MakeLabel(_season.Name, 14f, bold: true);
        lblSeason.Location = new Point(122, 18);

        var btnAdd = AppColors.MakeBtn("+ Add Item", AppColors.Accent);
        btnAdd.Width = 110; btnAdd.Height = 34;
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAdd.Click += BtnAdd_Click;

        var btnHours = AppColors.MakeBtn("⏱ Hours Log", AppColors.Card);
        btnHours.Width = 110; btnHours.Height = 34;
        btnHours.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnHours.Click += BtnHours_Click;

        pnlHeader.SizeChanged += (_, _) =>
        {
            btnAdd.Location = new Point(pnlHeader.Width - 232, 12);
            btnHours.Location = new Point(pnlHeader.Width - 122, 12);
        };

        pnlHeader.Controls.AddRange(new Control[] { btnBack, lblSeason, btnAdd, btnHours });

        // Footer
        var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = AppColors.StatusBar };
        lblSummary = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = AppColors.TextSecond,
            Font = new Font("Segoe UI", 9f),
            BackColor = Color.Transparent,
            Padding = new Padding(12, 0, 0, 0)
        };
        pnlFooter.Controls.Add(lblSummary);

        // Grid
        dgv = new DataGridView { Dock = DockStyle.Fill };
        AppColors.StyleGrid(dgv);

        AddTextCol("Ep",             45, true,  DataGridViewContentAlignment.MiddleCenter);
        AddTextCol("Item",          185, true,  DataGridViewContentAlignment.MiddleLeft);
        AddTextCol("Cost",           75, true,  DataGridViewContentAlignment.MiddleRight);
        AddTextCol("Parts",          75, true,  DataGridViewContentAlignment.MiddleRight);
        AddTextCol("eBay Fee (9%)",  90, false, DataGridViewContentAlignment.MiddleRight);
        AddTextCol("Est. Sell",      90, true,  DataGridViewContentAlignment.MiddleRight);
        AddTextCol("Est. Profit",    95, false, DataGridViewContentAlignment.MiddleRight);
        AddTextCol("Actual Sell",    95, true,  DataGridViewContentAlignment.MiddleRight);
        AddTextCol("Postage",        75, true,  DataGridViewContentAlignment.MiddleRight);
        AddTextCol("Net Profit",     95, false, DataGridViewContentAlignment.MiddleRight);

        dgv.Columns[ColItem].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        // Delete column
        var colDel = new DataGridViewButtonColumn
        {
            HeaderText = "",
            Text = "×",
            UseColumnTextForButtonValue = true,
            Width = 36,
            FlatStyle = FlatStyle.Flat,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colDel.DefaultCellStyle.BackColor = Color.FromArgb(70, 25, 25);
        colDel.DefaultCellStyle.ForeColor = AppColors.RedFg;
        colDel.DefaultCellStyle.SelectionBackColor = Color.FromArgb(100, 30, 30);
        colDel.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgv.Columns.Add(colDel);

        dgv.EditingControlShowing += Grid_EditingControlShowing;
        dgv.CellEndEdit           += Grid_CellEndEdit;
        dgv.CellFormatting        += Grid_CellFormatting;
        dgv.CellContentClick      += Grid_CellContentClick;

        Controls.Add(dgv);
        Controls.Add(pnlFooter);
        Controls.Add(pnlHeader);
    }

    private void AddTextCol(string header, int width, bool editable, DataGridViewContentAlignment align)
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

    private void LoadEpisodes()
    {
        _episodes = DbContext.GetEpisodesForSeason(_season.Id);
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

        row.Cells[ColEp].Value    = ep.EpisodeNumber;
        row.Cells[ColItem].Value  = ep.ItemDescription;
        row.Cells[ColCost].Value  = Calculations.Gbp(ep.Cost);
        row.Cells[ColParts].Value = Calculations.Gbp(ep.Parts);

        double? feeBase = ep.EstSellPrice ?? ep.ActualSellPrice;
        row.Cells[ColEbayFee].Value = feeBase.HasValue ? Calculations.Gbp(Calculations.EbayFee(feeBase.Value)) : "-";

        row.Cells[ColEstSell].Value  = ep.EstSellPrice.HasValue    ? Calculations.Gbp(ep.EstSellPrice.Value)    : "-";
        row.Cells[ColActSell].Value  = ep.ActualSellPrice.HasValue ? Calculations.Gbp(ep.ActualSellPrice.Value) : "-";
        row.Cells[ColPostage].Value  = Calculations.Gbp(ep.Postage);

        row.Cells[ColEstProfit].Value = ep.EstSellPrice.HasValue
            ? Calculations.Gbp(Calculations.EstimatedProfit(ep.Cost, ep.Parts, ep.EstSellPrice.Value))
            : "-";

        row.Cells[ColNetProfit].Value = ep.ActualSellPrice.HasValue
            ? Calculations.Gbp(Calculations.NetProfit(ep.Cost, ep.Parts, ep.ActualSellPrice.Value, ep.Postage))
            : "-";
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _episodes.Count) return;
        var ep = _episodes[e.RowIndex];

        if (e.ColumnIndex == ColEstProfit && ep.EstSellPrice.HasValue)
        {
            double v = Calculations.EstimatedProfit(ep.Cost, ep.Parts, ep.EstSellPrice.Value);
            ApplyProfitStyle(e.CellStyle, v);
        }
        else if (e.ColumnIndex == ColNetProfit && ep.ActualSellPrice.HasValue)
        {
            double v = Calculations.NetProfit(ep.Cost, ep.Parts, ep.ActualSellPrice.Value, ep.Postage);
            ApplyProfitStyle(e.CellStyle, v);
        }
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
        int count = _episodes.Count;
        double totalInvest = _episodes.Sum(e => e.Cost + e.Parts);
        var withNet = _episodes.Where(e => e.ActualSellPrice.HasValue).ToList();
        double? totalNet = withNet.Count > 0
            ? withNet.Sum(e => Calculations.NetProfit(e.Cost, e.Parts, e.ActualSellPrice!.Value, e.Postage))
            : null;

        lblSummary.Text = $"  Items: {count}   |   Total Investment: {Calculations.Gbp(totalInvest)}" +
                          $"   |   Total Net Profit: {Calculations.Gbp(totalNet)}";
    }

    private void Grid_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
    {
        if (dgv.CurrentCell == null) return;
        int col = dgv.CurrentCell.ColumnIndex;
        int row = dgv.CurrentCell.RowIndex;
        if (row < 0 || row >= _episodes.Count) return;
        var ep = _episodes[row];

        if (e.Control is TextBox tb)
        {
            tb.BackColor = AppColors.Card;
            tb.ForeColor = AppColors.TextPrimary;
            tb.BorderStyle = BorderStyle.None;

            string raw = col switch
            {
                ColEp      => ep.EpisodeNumber.ToString(),
                ColCost    => ep.Cost.ToString("F2"),
                ColParts   => ep.Parts.ToString("F2"),
                ColEstSell => ep.EstSellPrice.HasValue ? ep.EstSellPrice.Value.ToString("F2") : "",
                ColActSell => ep.ActualSellPrice.HasValue ? ep.ActualSellPrice.Value.ToString("F2") : "",
                ColPostage => ep.Postage.ToString("F2"),
                ColItem    => ep.ItemDescription,
                _          => tb.Text
            };
            tb.Text = raw;
            tb.SelectAll();
        }
    }

    private void Grid_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _episodes.Count) return;
        var ep = _episodes[e.RowIndex];
        string raw = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString()?.Trim() ?? "";

        switch (e.ColumnIndex)
        {
            case ColEp:
                if (int.TryParse(raw, out int epNum) && epNum > 0) ep.EpisodeNumber = epNum;
                break;
            case ColItem:
                if (!string.IsNullOrWhiteSpace(raw)) ep.ItemDescription = raw;
                break;
            case ColCost:
                if (TryNum(raw, out double cost)) ep.Cost = cost;
                break;
            case ColParts:
                if (TryNum(raw, out double parts)) ep.Parts = parts;
                break;
            case ColEstSell:
                ep.EstSellPrice = string.IsNullOrWhiteSpace(raw) ? null : TryNum(raw, out double est) ? est : ep.EstSellPrice;
                break;
            case ColActSell:
                ep.ActualSellPrice = string.IsNullOrWhiteSpace(raw) ? null : TryNum(raw, out double act) ? act : ep.ActualSellPrice;
                break;
            case ColPostage:
                if (TryNum(raw, out double post)) ep.Postage = post;
                break;
            default:
                return;
        }

        DbContext.UpdateEpisode(ep);
        RefreshRow(e.RowIndex);
        UpdateSummary();
    }

    private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _episodes.Count) return;
        if (e.ColumnIndex != ColDelete) return;

        var ep = _episodes[e.RowIndex];
        var result = MessageBox.Show(
            $"Delete \"{ep.ItemDescription}\"?\n\nThis cannot be undone.",
            "Delete Item",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes) return;

        DbContext.DeleteEpisode(ep.Id);
        _episodes.RemoveAt(e.RowIndex);
        dgv.Rows.RemoveAt(e.RowIndex);
        UpdateSummary();
    }

    private static bool TryNum(string s, out double v) =>
        double.TryParse(s.Replace("£", ""), out v) && v >= 0;

    private void BtnBack_Click(object? sender, EventArgs e)
    {
        _app.Navigate(new MainMenuControl(_app));
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        int suggested = DbContext.GetNextEpisodeNumber(_season.Id);
        using var dlg = new AddEpisodeForm(_season.Id, suggested);
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.CreatedEpisode == null) return;

        try
        {
            var ep = DbContext.CreateEpisode(dlg.CreatedEpisode);
            _episodes.Add(ep);
            dgv.Rows.Add();
            RefreshRow(dgv.Rows.Count - 1);
            UpdateSummary();
            dgv.FirstDisplayedScrollingRowIndex = dgv.Rows.Count - 1;

            using var hoursForm = new LogHoursForm(_season.Id, ep.EpisodeNumber);
            hoursForm.ShowDialog(FindForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not add item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void BtnHours_Click(object? sender, EventArgs e)
    {
        DbContext.SetAppState("last_season_id", _season.Id.ToString());
        _app.Navigate(new HoursViewControl(_app, _season));
    }
}

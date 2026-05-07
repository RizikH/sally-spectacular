using RepairTracker.Database;
using RepairTracker.Helpers;
using RepairTracker.Logic;
using RepairTracker.Models;

namespace RepairTracker.Forms;

public class SeasonViewForm : Form
{
    private const int ColEp = 0, ColItem = 1, ColCost = 2, ColParts = 3, ColEbayFee = 4,
                      ColEstSell = 5, ColEstProfit = 6, ColActSell = 7, ColPostage = 8, ColNetProfit = 9;

    private readonly Season _season;
    private List<Episode> _episodes = new();
    private DataGridView dgv = null!;
    private Label lblSummary = null!;

    public SeasonViewForm(Season season)
    {
        _season = season;
        InitializeComponent();
        LoadEpisodes();
    }

    private void InitializeComponent()
    {
        Text = $"Repair Tracker — {_season.Name}";
        Size = new Size(1220, 680);
        MinimumSize = new Size(900, 500);
        StartPosition = FormStartPosition.CenterScreen;
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

        var btnAdd = AppColors.MakeBtn("+ Add Episode", AppColors.Accent);
        btnAdd.Width = 130; btnAdd.Height = 34;
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAdd.Click += BtnAdd_Click;

        var btnHours = AppColors.MakeBtn("⏱ Hours Log", AppColors.Card);
        btnHours.Width = 110; btnHours.Height = 34;
        btnHours.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnHours.Click += BtnHours_Click;

        pnlHeader.SizeChanged += (_, _) =>
        {
            btnAdd.Location = new Point(pnlHeader.Width - 252, 12);
            btnHours.Location = new Point(pnlHeader.Width - 128, 12);
        };

        pnlHeader.Controls.AddRange(new Control[] { btnBack, lblSeason, btnAdd, btnHours });

        // Footer / summary
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

        AddCol("Ep",             45, false, DataGridViewContentAlignment.MiddleCenter);
        AddCol("Item",          185, true,  DataGridViewContentAlignment.MiddleLeft);
        AddCol("Cost",           75, true,  DataGridViewContentAlignment.MiddleRight);
        AddCol("Parts",          75, true,  DataGridViewContentAlignment.MiddleRight);
        AddCol("eBay Fee (9%)",  90, false, DataGridViewContentAlignment.MiddleRight);
        AddCol("Est. Sell",      90, true,  DataGridViewContentAlignment.MiddleRight);
        AddCol("Est. Profit",    95, false, DataGridViewContentAlignment.MiddleRight);
        AddCol("Actual Sell",    95, true,  DataGridViewContentAlignment.MiddleRight);
        AddCol("Postage",        75, true,  DataGridViewContentAlignment.MiddleRight);
        AddCol("Net Profit",     95, false, DataGridViewContentAlignment.MiddleRight);

        // Make Item column expand to fill remaining width
        dgv.Columns[ColItem].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        dgv.EditingControlShowing += Grid_EditingControlShowing;
        dgv.CellEndEdit           += Grid_CellEndEdit;

        Controls.Add(dgv);
        Controls.Add(pnlFooter);
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

        // eBay fee — show whichever sell price is available
        double? feeBase = ep.EstSellPrice ?? ep.ActualSellPrice;
        row.Cells[ColEbayFee].Value = feeBase.HasValue ? Calculations.Gbp(Calculations.EbayFee(feeBase.Value)) : "-";

        row.Cells[ColEstSell].Value = ep.EstSellPrice.HasValue ? Calculations.Gbp(ep.EstSellPrice.Value) : "-";

        if (ep.EstSellPrice.HasValue)
        {
            double ep_ = Calculations.EstimatedProfit(ep.Cost, ep.Parts, ep.EstSellPrice.Value);
            row.Cells[ColEstProfit].Value = Calculations.Gbp(ep_);
            CellFormatter.ApplyProfit(row.Cells[ColEstProfit], ep_, triggered: true);
        }
        else
        {
            row.Cells[ColEstProfit].Value = "-";
            CellFormatter.Reset(row.Cells[ColEstProfit]);
        }

        row.Cells[ColActSell].Value = ep.ActualSellPrice.HasValue ? Calculations.Gbp(ep.ActualSellPrice.Value) : "-";
        row.Cells[ColPostage].Value = Calculations.Gbp(ep.Postage);

        if (ep.ActualSellPrice.HasValue)
        {
            double np = Calculations.NetProfit(ep.Cost, ep.Parts, ep.ActualSellPrice.Value, ep.Postage);
            row.Cells[ColNetProfit].Value = Calculations.Gbp(np);
            CellFormatter.ApplyProfit(row.Cells[ColNetProfit], np, triggered: true);
        }
        else
        {
            row.Cells[ColNetProfit].Value = "-";
            CellFormatter.Reset(row.Cells[ColNetProfit]);
        }
    }

    private void UpdateSummary()
    {
        int count = _episodes.Count;
        double totalCost = _episodes.Sum(e => e.Cost + e.Parts);
        var withNet = _episodes.Where(e => e.ActualSellPrice.HasValue).ToList();
        double? totalNet = withNet.Count > 0
            ? withNet.Sum(e => Calculations.NetProfit(e.Cost, e.Parts, e.ActualSellPrice!.Value, e.Postage))
            : null;

        lblSummary.Text = $"  Episodes: {count}   |   Total Investment: {Calculations.Gbp(totalCost)}" +
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
                ColCost     => ep.Cost.ToString("F2"),
                ColParts    => ep.Parts.ToString("F2"),
                ColEstSell  => ep.EstSellPrice.HasValue ? ep.EstSellPrice.Value.ToString("F2") : "",
                ColActSell  => ep.ActualSellPrice.HasValue ? ep.ActualSellPrice.Value.ToString("F2") : "",
                ColPostage  => ep.Postage.ToString("F2"),
                ColItem     => ep.ItemDescription,
                _           => tb.Text
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
        }

        DbContext.UpdateEpisode(ep);
        RefreshRow(e.RowIndex);
        UpdateSummary();
    }

    private static bool TryNum(string s, out double v) =>
        double.TryParse(s.Replace("£", ""), out v) && v >= 0;

    private void BtnBack_Click(object? sender, EventArgs e)
    {
        DbContext.SetAppState("last_season_id", _season.Id.ToString());
        Close();
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        int next = DbContext.GetNextEpisodeNumber(_season.Id);
        using var dlg = new AddEpisodeForm(_season.Id, next);
        if (dlg.ShowDialog(this) != DialogResult.OK || dlg.CreatedEpisode == null) return;

        var ep = DbContext.CreateEpisode(dlg.CreatedEpisode);
        _episodes.Add(ep);
        dgv.Rows.Add();
        RefreshRow(dgv.Rows.Count - 1);
        UpdateSummary();

        // Scroll to new row
        dgv.FirstDisplayedScrollingRowIndex = dgv.Rows.Count - 1;

        using var hoursForm = new LogHoursForm(ep.Id, ep.EpisodeNumber);
        hoursForm.ShowDialog(this);
    }

    private void BtnHours_Click(object? sender, EventArgs e)
    {
        DbContext.SetAppState("last_season_id", _season.Id.ToString());
        var form = new HoursViewForm(_season);
        form.FormClosed += (_, _) => Show();
        Hide();
        form.Show();
    }
}

using RepairTracker.Database;
using RepairTracker.Helpers;
using RepairTracker.Logic;
using RepairTracker.Models;

namespace RepairTracker.Forms;

public class ProfitSummaryForm : Form
{
    private readonly int? _filterSeasonId;
    private readonly string _title;

    public ProfitSummaryForm(Season? filterSeason = null)
    {
        _filterSeasonId = filterSeason?.Id;
        _title = filterSeason == null ? "Total Profit — All Seasons" : $"Profit — {filterSeason.Name}";
        InitializeComponent();
        LoadData();
    }

    private DataGridView dgv = null!;

    private void InitializeComponent()
    {
        Text = _title;
        Size = new Size(780, 480);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = new Font("Segoe UI", 9f);

        var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = AppColors.Card };
        var lbl = AppColors.MakeLabel(_title, 13f, bold: true);
        lbl.Location = new Point(16, 15);
        pnlTitle.Controls.Add(lbl);

        dgv = new DataGridView { Dock = DockStyle.Fill };
        AppColors.StyleGrid(dgv);
        dgv.ReadOnly = true;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        AddCol(dgv, "Season",         180, DataGridViewContentAlignment.MiddleLeft);
        AddCol(dgv, "Episodes",        70, DataGridViewContentAlignment.MiddleCenter);
        AddCol(dgv, "Total Cost",      90, DataGridViewContentAlignment.MiddleRight);
        AddCol(dgv, "Total Parts",     90, DataGridViewContentAlignment.MiddleRight);
        AddCol(dgv, "Total Postage",   95, DataGridViewContentAlignment.MiddleRight);
        AddCol(dgv, "Est. Profit",     95, DataGridViewContentAlignment.MiddleRight);
        AddCol(dgv, "Net Profit",      95, DataGridViewContentAlignment.MiddleRight);

        dgv.CellFormatting += Grid_CellFormatting;

        var pnlClose = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = AppColors.Card };
        var btnClose = AppColors.MakeBtn("Close", AppColors.AccentDark);
        btnClose.Width = 90;
        btnClose.Location = new Point(pnlClose.Width - 106, 8);
        btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        btnClose.Click += (_, _) => Close();
        pnlClose.Controls.Add(btnClose);

        Controls.Add(dgv);
        Controls.Add(pnlClose);
        Controls.Add(pnlTitle);
    }

    private void LoadData()
    {
        var rows = DbContext.GetProfitSummary(_filterSeasonId);
        dgv.Rows.Clear();

        double sumCost = 0, sumParts = 0, sumPost = 0;
        double? sumEst = null, sumNet = null;
        int totalEps = 0;

        foreach (var (s, cost, parts, post, est, net) in rows)
        {
            int i = dgv.Rows.Add(
                s.Name,
                s.EpisodeCount,
                Calculations.Gbp(cost),
                Calculations.Gbp(parts),
                Calculations.Gbp(post),
                Calculations.Gbp(est),
                Calculations.Gbp(net));

            dgv.Rows[i].Tag = (est, net);

            sumCost += cost; sumParts += parts; sumPost += post; totalEps += s.EpisodeCount;
            if (est.HasValue) sumEst = (sumEst ?? 0) + est.Value;
            if (net.HasValue) sumNet = (sumNet ?? 0) + net.Value;
        }

        if (rows.Count > 1)
        {
            int ti = dgv.Rows.Add(
                "TOTAL",
                totalEps,
                Calculations.Gbp(sumCost),
                Calculations.Gbp(sumParts),
                Calculations.Gbp(sumPost),
                Calculations.Gbp(sumEst),
                Calculations.Gbp(sumNet));

            dgv.Rows[ti].Tag = (sumEst, sumNet);
            dgv.Rows[ti].DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.Rows[ti].DefaultCellStyle.BackColor = AppColors.GridHeader;
        }
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || dgv.Rows[e.RowIndex].Tag is not ValueTuple<double?, double?> tag) return;
        var (est, net) = tag;

        if (e.ColumnIndex == 5) // Est. Profit
        {
            if (est.HasValue)
            {
                e.CellStyle.ForeColor = est.Value < 0 ? AppColors.RedFg : AppColors.GreenFg;
                e.CellStyle.BackColor = est.Value < 0 ? AppColors.RedBg : AppColors.GreenBg;
            }
        }
        else if (e.ColumnIndex == 6) // Net Profit
        {
            if (net.HasValue)
            {
                e.CellStyle.ForeColor = net.Value < 0 ? AppColors.RedFg : AppColors.GreenFg;
                e.CellStyle.BackColor = net.Value < 0 ? AppColors.RedBg : AppColors.GreenBg;
            }
        }
    }

    private static void AddCol(DataGridView dgv, string header, int width, DataGridViewContentAlignment align)
    {
        var col = new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            Width = width,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            ReadOnly = true
        };
        col.DefaultCellStyle.Alignment = align;
        col.HeaderCell.Style.Alignment = align;
        dgv.Columns.Add(col);
    }
}

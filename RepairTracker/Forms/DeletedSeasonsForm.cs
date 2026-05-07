using RepairTracker.Database;
using RepairTracker.Helpers;
using RepairTracker.Models;

namespace RepairTracker.Forms;

public class DeletedSeasonsForm : Form
{
    private List<Season> _deleted = new();
    private DataGridView dgv = null!;
    private Label lblEmpty = null!;
    private Button btnRestore = null!;

    public DeletedSeasonsForm()
    {
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        Text = "Recently Deleted Seasons";
        Size = new Size(620, 440);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = new Font("Segoe UI", 9f);

        // Title panel
        var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = AppColors.Surface };
        var lblTitle = AppColors.MakeLabel("Recently Deleted", 13f, bold: true);
        lblTitle.Location = new Point(16, 12);
        var lblSub = AppColors.MakeLabel(
            "Deleted seasons are permanently removed after 30 days.",
            9f, color: AppColors.TextSecond);
        lblSub.Location = new Point(16, 38);
        pnlTitle.Controls.AddRange(new Control[] { lblTitle, lblSub });

        // Grid
        dgv = new DataGridView { Dock = DockStyle.Fill };
        AppColors.StyleGrid(dgv);
        dgv.ReadOnly = true;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;

        AddCol("Season Name",  220, DataGridViewContentAlignment.MiddleLeft);
        AddCol("Deleted On",   140, DataGridViewContentAlignment.MiddleCenter);
        AddCol("Days Left",    100, DataGridViewContentAlignment.MiddleCenter);

        dgv.CellFormatting += Grid_CellFormatting;

        // Empty state label (shown when no deleted seasons)
        lblEmpty = new Label
        {
            Text = "No recently deleted seasons.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = AppColors.TextMuted,
            Font = new Font("Segoe UI", 11f),
            BackColor = AppColors.Background,
            Visible = false
        };

        // Button panel
        var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = AppColors.Card };

        btnRestore = AppColors.MakeBtn("Restore Selected", AppColors.Accent);
        btnRestore.Width = 150; btnRestore.Height = 34; btnRestore.Location = new Point(14, 10);
        btnRestore.Click += BtnRestore_Click;

        var btnClose = AppColors.MakeBtn("Close", AppColors.Card);
        btnClose.Width = 90; btnClose.Height = 34;
        btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnClose.Click += (_, _) => Close();
        pnlBtn.SizeChanged += (_, _) => btnClose.Location = new Point(pnlBtn.Width - 104, 10);

        pnlBtn.Controls.AddRange(new Control[] { btnRestore, btnClose });

        Controls.Add(lblEmpty);
        Controls.Add(dgv);
        Controls.Add(pnlBtn);
        Controls.Add(pnlTitle);
    }

    private void AddCol(string header, int width, DataGridViewContentAlignment align)
    {
        var col = new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            Width = width,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        col.DefaultCellStyle.Alignment = align;
        col.HeaderCell.Style.Alignment = align;
        dgv.Columns.Add(col);
    }

    private void LoadData()
    {
        _deleted = DbContext.GetDeletedSeasons();
        dgv.Rows.Clear();

        if (_deleted.Count == 0)
        {
            dgv.Visible = false;
            lblEmpty.Visible = true;
            btnRestore.Enabled = false;
            return;
        }

        dgv.Visible = true;
        lblEmpty.Visible = false;
        btnRestore.Enabled = true;

        foreach (var s in _deleted)
        {
            var deletedAt = DateTime.Parse(s.DeletedAt!, null,
                System.Globalization.DateTimeStyles.RoundtripKind);

            dgv.Rows.Add(
                s.Name,
                deletedAt.ToLocalTime().ToString("dd MMM yyyy"),
                $"{s.DaysUntilPurge} day{(s.DaysUntilPurge == 1 ? "" : "s")}");

            dgv.Rows[dgv.Rows.Count - 1].Tag = s;
        }
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 2) return;
        if (dgv.Rows[e.RowIndex].Tag is not Season s) return;

        // Colour the "Days Left" cell: red if ≤ 7 days, amber if ≤ 14
        if (s.DaysUntilPurge <= 7)
        {
            e.CellStyle.ForeColor = AppColors.RedFg;
            e.CellStyle.BackColor = AppColors.RedBg;
        }
        else if (s.DaysUntilPurge <= 14)
        {
            e.CellStyle.ForeColor = Color.FromArgb(255, 200, 80);
            e.CellStyle.BackColor = Color.FromArgb(80, 60, 10);
        }
    }

    private void BtnRestore_Click(object? sender, EventArgs e)
    {
        if (dgv.SelectedRows.Count == 0) return;
        if (dgv.SelectedRows[0].Tag is not Season season) return;

        var active = DbContext.GetAllSeasons();
        if (active.Any(s => s.Name.Equals(season.Name, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(
                $"Cannot restore \"{season.Name}\" — an active season with that name already exists.\n\nRename or delete the existing season first.",
                "Cannot Restore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            DbContext.RestoreSeason(season.Id);
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not restore season: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

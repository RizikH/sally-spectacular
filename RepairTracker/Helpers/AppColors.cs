namespace RepairTracker.Helpers;

public static class AppColors
{
    public static readonly Color Background  = Color.FromArgb(30, 30, 30);
    public static readonly Color Surface     = Color.FromArgb(37, 37, 38);
    public static readonly Color Card        = Color.FromArgb(45, 45, 48);
    public static readonly Color CardHover   = Color.FromArgb(58, 58, 62);
    public static readonly Color Accent      = Color.FromArgb(0, 120, 212);
    public static readonly Color AccentDark  = Color.FromArgb(0, 90, 170);
    public static readonly Color TextPrimary = Color.FromArgb(240, 240, 240);
    public static readonly Color TextSecond  = Color.FromArgb(180, 180, 185);
    public static readonly Color TextMuted   = Color.FromArgb(110, 110, 115);
    public static readonly Color Border      = Color.FromArgb(63, 63, 70);
    public static readonly Color BorderHL    = Color.FromArgb(0, 120, 212);
    public static readonly Color GridHeader  = Color.FromArgb(28, 28, 28);
    public static readonly Color GridRow     = Color.FromArgb(40, 40, 42);
    public static readonly Color GridAlt     = Color.FromArgb(35, 35, 37);
    public static readonly Color StatusBar   = Color.FromArgb(24, 24, 24);
    public static readonly Color GreenBg     = Color.FromArgb(20, 70, 35);
    public static readonly Color GreenFg     = Color.FromArgb(130, 220, 160);
    public static readonly Color RedBg       = Color.FromArgb(90, 20, 20);
    public static readonly Color RedFg       = Color.FromArgb(240, 140, 140);

    public static Button MakeBtn(string text, Color back, int height = 34)
    {
        var btn = new Button
        {
            Text = text,
            BackColor = back,
            ForeColor = TextPrimary,
            FlatStyle = FlatStyle.Flat,
            Height = height,
            AutoSize = false,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            Padding = new Padding(0)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(back, 0.15f);
        return btn;
    }

    public static void StyleGrid(DataGridView dgv)
    {
        dgv.EditMode = DataGridViewEditMode.EditOnEnter;
        dgv.BackgroundColor = GridRow;
        dgv.GridColor = Border;
        dgv.BorderStyle = BorderStyle.None;
        dgv.EnableHeadersVisualStyles = false;
        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.AllowUserToResizeRows = false;
        dgv.RowTemplate.Height = 28;
        dgv.ColumnHeadersHeight = 30;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        dgv.DefaultCellStyle.BackColor = GridRow;
        dgv.DefaultCellStyle.ForeColor = TextPrimary;
        dgv.DefaultCellStyle.SelectionBackColor = Accent;
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
        dgv.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);

        dgv.AlternatingRowsDefaultCellStyle.BackColor = GridAlt;
        dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary;
        dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Accent;
        dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

        dgv.ColumnHeadersDefaultCellStyle.BackColor = GridHeader;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextSecond;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f);
        dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = GridHeader;
        dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
    }

    public static Label MakeLabel(string text, float size = 9f, bool bold = false, Color? color = null)
    {
        return new Label
        {
            Text = text,
            ForeColor = color ?? TextPrimary,
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
            AutoSize = true,
            BackColor = Color.Transparent
        };
    }
}

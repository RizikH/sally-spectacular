namespace RepairTracker.Helpers;

public static class AppColors
{
    public static Color Background  = Color.FromArgb(30, 30, 30);
    public static Color Surface     = Color.FromArgb(37, 37, 38);
    public static Color Card        = Color.FromArgb(45, 45, 48);
    public static Color CardHover   = Color.FromArgb(58, 58, 62);
    public static Color Accent      = Color.FromArgb(0, 120, 212);
    public static Color AccentDark  = Color.FromArgb(0, 90, 170);
    public static Color TextPrimary = Color.FromArgb(240, 240, 240);
    public static Color TextSecond  = Color.FromArgb(180, 180, 185);
    public static Color TextMuted   = Color.FromArgb(110, 110, 115);
    public static Color Border      = Color.FromArgb(63, 63, 70);
    public static Color BorderHL    = Color.FromArgb(0, 120, 212);
    public static Color GridHeader  = Color.FromArgb(28, 28, 28);
    public static Color GridRow     = Color.FromArgb(40, 40, 42);
    public static Color GridAlt     = Color.FromArgb(35, 35, 37);
    public static Color StatusBar   = Color.FromArgb(24, 24, 24);
    public static Color GreenBg     = Color.FromArgb(20, 70, 35);
    public static Color GreenFg     = Color.FromArgb(130, 220, 160);
    public static Color GreenSel    = Color.FromArgb(30, 100, 50);
    public static Color RedBg       = Color.FromArgb(90, 20, 20);
    public static Color RedFg       = Color.FromArgb(240, 140, 140);
    public static Color RedSel      = Color.FromArgb(130, 40, 40);

    public static void Apply(bool dark)
    {
        if (dark)
        {
            Background  = Color.FromArgb(30, 30, 30);
            Surface     = Color.FromArgb(37, 37, 38);
            Card        = Color.FromArgb(45, 45, 48);
            CardHover   = Color.FromArgb(58, 58, 62);
            Accent      = Color.FromArgb(0, 120, 212);
            AccentDark  = Color.FromArgb(0, 90, 170);
            TextPrimary = Color.FromArgb(240, 240, 240);
            TextSecond  = Color.FromArgb(180, 180, 185);
            TextMuted   = Color.FromArgb(110, 110, 115);
            Border      = Color.FromArgb(63, 63, 70);
            BorderHL    = Color.FromArgb(0, 120, 212);
            GridHeader  = Color.FromArgb(28, 28, 28);
            GridRow     = Color.FromArgb(40, 40, 42);
            GridAlt     = Color.FromArgb(35, 35, 37);
            StatusBar   = Color.FromArgb(24, 24, 24);
            GreenBg     = Color.FromArgb(20, 70, 35);
            GreenFg     = Color.FromArgb(130, 220, 160);
            GreenSel    = Color.FromArgb(30, 100, 50);
            RedBg       = Color.FromArgb(90, 20, 20);
            RedFg       = Color.FromArgb(240, 140, 140);
            RedSel      = Color.FromArgb(130, 40, 40);
        }
        else
        {
            Background  = Color.FromArgb(245, 245, 248);
            Surface     = Color.FromArgb(255, 255, 255);
            Card        = Color.FromArgb(232, 232, 240);
            CardHover   = Color.FromArgb(218, 218, 230);
            Accent      = Color.FromArgb(0, 100, 190);
            AccentDark  = Color.FromArgb(0, 75, 155);
            TextPrimary = Color.FromArgb(22, 22, 30);
            TextSecond  = Color.FromArgb(88, 88, 105);
            TextMuted   = Color.FromArgb(145, 145, 158);
            Border      = Color.FromArgb(195, 195, 210);
            BorderHL    = Color.FromArgb(0, 100, 190);
            GridHeader  = Color.FromArgb(218, 218, 230);
            GridRow     = Color.FromArgb(252, 252, 255);
            GridAlt     = Color.FromArgb(240, 240, 248);
            StatusBar   = Color.FromArgb(225, 225, 235);
            GreenBg     = Color.FromArgb(210, 245, 220);
            GreenFg     = Color.FromArgb(18, 105, 45);
            GreenSel    = Color.FromArgb(150, 215, 170);
            RedBg       = Color.FromArgb(250, 215, 215);
            RedFg       = Color.FromArgb(155, 22, 22);
            RedSel      = Color.FromArgb(220, 150, 150);
        }
    }

    public static Button MakeBtn(string text, Color back, int height = 34)
    {
        var fg = back.GetBrightness() < 0.45f ? Color.White : TextPrimary;
        var hover = back.GetBrightness() < 0.45f
            ? ControlPaint.Light(back, 0.15f)
            : ControlPaint.Dark(back, 0.08f);

        var btn = new Button
        {
            Text = text,
            BackColor = back,
            ForeColor = fg,
            FlatStyle = FlatStyle.Flat,
            Height = height,
            AutoSize = false,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            Padding = new Padding(0)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = hover;
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

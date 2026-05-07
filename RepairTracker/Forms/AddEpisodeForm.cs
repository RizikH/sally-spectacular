using RepairTracker.Helpers;
using RepairTracker.Models;

namespace RepairTracker.Forms;

public class AddEpisodeForm : Form
{
    public Episode? CreatedEpisode { get; private set; }

    private readonly int _seasonId;
    private readonly int _nextNum;

    private TextBox txtItem = null!, txtCost = null!, txtParts = null!,
                    txtPostage = null!, txtEstSell = null!, txtActSell = null!;
    private Label lblError = null!;

    public AddEpisodeForm(int seasonId, int nextNum)
    {
        _seasonId = seasonId;
        _nextNum = nextNum;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Add Episode";
        Size = new Size(420, 480);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppColors.Surface;
        ForeColor = AppColors.TextPrimary;
        Font = new Font("Segoe UI", 9f);

        var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = AppColors.Card };
        var lblTitle = AppColors.MakeLabel($"Add Episode {_nextNum}", 13f, bold: true);
        lblTitle.Location = new Point(16, 15);
        pnlTitle.Controls.Add(lblTitle);

        var pnlForm = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 14, 20, 0) };

        (txtItem,    var r1) = MakeRow(pnlForm,  0, "Item Description *");
        (txtCost,    var r2) = MakeRow(pnlForm,  1, "Cost (£)");
        (txtParts,   var r3) = MakeRow(pnlForm,  2, "Parts (£)");
        (txtPostage, var r4) = MakeRow(pnlForm,  3, "Postage (£)");
        (txtEstSell, var r5) = MakeRow(pnlForm,  4, "Est. Sell Price (£, optional)");
        (txtActSell, var r6) = MakeRow(pnlForm,  5, "Actual Sell Price (£, optional)");

        txtCost.Text = "0"; txtParts.Text = "0"; txtPostage.Text = "0";

        lblError = new Label
        {
            ForeColor = AppColors.RedFg,
            Font = new Font("Segoe UI", 8.5f),
            AutoSize = true,
            BackColor = Color.Transparent,
            Location = new Point(20, 6 * 58 + 20)
        };
        pnlForm.Controls.Add(lblError);

        var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = AppColors.Card };
        var btnCancel = AppColors.MakeBtn("Cancel", AppColors.Card);
        btnCancel.Width = 90;
        btnCancel.Location = new Point(200, 10);
        var btnNext = AppColors.MakeBtn("  Next →", AppColors.Accent);
        btnNext.Width = 100;
        btnNext.Location = new Point(298, 10);

        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        btnNext.Click += BtnNext_Click;

        pnlBtn.Controls.AddRange(new Control[] { btnCancel, btnNext });

        Controls.Add(pnlForm);
        Controls.Add(pnlBtn);
        Controls.Add(pnlTitle);

        AcceptButton = btnNext;
        CancelButton = btnCancel;
    }

    private static (TextBox tb, Panel row) MakeRow(Panel parent, int index, string label)
    {
        int y = index * 58 + 20;
        var lbl = new Label
        {
            Text = label,
            Location = new Point(0, y),
            AutoSize = true,
            ForeColor = AppColors.TextSecond,
            Font = new Font("Segoe UI", 8.5f),
            BackColor = Color.Transparent
        };
        var tb = new TextBox
        {
            Location = new Point(0, y + 20),
            Width = 360,
            BackColor = AppColors.Card,
            ForeColor = AppColors.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9.5f)
        };
        parent.Controls.Add(lbl);
        parent.Controls.Add(tb);
        return (tb, parent);
    }

    private void BtnNext_Click(object? sender, EventArgs e)
    {
        lblError.Text = "";

        if (string.IsNullOrWhiteSpace(txtItem.Text))
        {
            lblError.Text = "Item description is required.";
            txtItem.Focus();
            return;
        }

        if (!TryParseNum(txtCost.Text, out double cost) ||
            !TryParseNum(txtParts.Text, out double parts) ||
            !TryParseNum(txtPostage.Text, out double postage))
        {
            lblError.Text = "Cost, Parts and Postage must be valid numbers.";
            return;
        }

        double? estSell = null, actSell = null;
        if (!string.IsNullOrWhiteSpace(txtEstSell.Text))
        {
            if (!TryParseNum(txtEstSell.Text, out double v)) { lblError.Text = "Invalid Est. Sell Price."; return; }
            estSell = v;
        }
        if (!string.IsNullOrWhiteSpace(txtActSell.Text))
        {
            if (!TryParseNum(txtActSell.Text, out double v)) { lblError.Text = "Invalid Actual Sell Price."; return; }
            actSell = v;
        }

        CreatedEpisode = new Episode
        {
            SeasonId = _seasonId,
            EpisodeNumber = _nextNum,
            ItemDescription = txtItem.Text.Trim(),
            Cost = cost,
            Parts = parts,
            Postage = postage,
            EstSellPrice = estSell,
            ActualSellPrice = actSell
        };

        DialogResult = DialogResult.OK;
        Close();
    }

    private static bool TryParseNum(string s, out double v) =>
        double.TryParse(s.Replace("£", "").Trim(), out v) && v >= 0;
}

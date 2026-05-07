using RepairTracker.Helpers;
using RepairTracker.Models;

namespace RepairTracker.Forms;

public class AddEpisodeForm : Form
{
    public Episode? CreatedEpisode { get; private set; }

    private readonly int _seasonId;
    private readonly int _suggestedNum;

    private TextBox txtNum = null!, txtItem = null!, txtCost = null!, txtParts = null!,
                    txtPostage = null!, txtEstSell = null!, txtActSell = null!;
    private Label lblError = null!;

    public AddEpisodeForm(int seasonId, int suggestedNum)
    {
        _seasonId = seasonId;
        _suggestedNum = suggestedNum;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Add Item";
        Size = new Size(420, 530);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppColors.Surface;
        ForeColor = AppColors.TextPrimary;
        Font = new Font("Segoe UI", 9f);

        var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = AppColors.Card };
        var lblTitle = AppColors.MakeLabel("Add Item to Season", 13f, bold: true);
        lblTitle.Location = new Point(16, 15);
        pnlTitle.Controls.Add(lblTitle);

        var pnlForm = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 14, 20, 0), AutoScroll = true };

        AddRow(pnlForm, 0, "Episode # (suggested — editable)", out txtNum);
        AddRow(pnlForm, 1, "Item Description *", out txtItem);
        AddRow(pnlForm, 2, "Cost (£)", out txtCost);
        AddRow(pnlForm, 3, "Parts (£)", out txtParts);
        AddRow(pnlForm, 4, "Postage (£)", out txtPostage);
        AddRow(pnlForm, 5, "Est. Sell Price (£, optional)", out txtEstSell);
        AddRow(pnlForm, 6, "Actual Sell Price (£, optional)", out txtActSell);

        txtNum.Text = _suggestedNum.ToString();
        txtCost.Text = "0";
        txtParts.Text = "0";
        txtPostage.Text = "0";

        lblError = new Label
        {
            ForeColor = AppColors.RedFg,
            Font = new Font("Segoe UI", 8.5f),
            AutoSize = true,
            BackColor = Color.Transparent,
            Location = new Point(20, 7 * 58 + 16)
        };
        pnlForm.Controls.Add(lblError);

        var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = AppColors.Card };
        var btnCancel = AppColors.MakeBtn("Cancel", AppColors.Card);
        btnCancel.Width = 90; btnCancel.Location = new Point(200, 10);
        var btnNext = AppColors.MakeBtn("  Next →", AppColors.Accent);
        btnNext.Width = 100; btnNext.Location = new Point(298, 10);

        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        btnNext.Click += BtnNext_Click;

        pnlBtn.Controls.AddRange(new Control[] { btnCancel, btnNext });

        Controls.Add(pnlForm);
        Controls.Add(pnlBtn);
        Controls.Add(pnlTitle);

        AcceptButton = btnNext;
        CancelButton = btnCancel;
    }

    private static void AddRow(Panel parent, int index, string label, out TextBox tb)
    {
        int y = index * 58 + 20;
        parent.Controls.Add(new Label
        {
            Text = label,
            Location = new Point(0, y),
            AutoSize = true,
            ForeColor = AppColors.TextSecond,
            Font = new Font("Segoe UI", 8.5f),
            BackColor = Color.Transparent
        });
        tb = new TextBox
        {
            Location = new Point(0, y + 20),
            Width = 360,
            BackColor = AppColors.Card,
            ForeColor = AppColors.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9.5f)
        };
        parent.Controls.Add(tb);
    }

    private void BtnNext_Click(object? sender, EventArgs e)
    {
        lblError.Text = "";

        if (!int.TryParse(txtNum.Text.Trim(), out int epNum) || epNum < 1)
        {
            lblError.Text = "Episode number must be a positive integer.";
            txtNum.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(txtItem.Text))
        {
            lblError.Text = "Item description is required.";
            txtItem.Focus();
            return;
        }

        if (!TryNum(txtCost.Text, out double cost) ||
            !TryNum(txtParts.Text, out double parts) ||
            !TryNum(txtPostage.Text, out double postage))
        {
            lblError.Text = "Cost, Parts and Postage must be valid non-negative numbers.";
            return;
        }

        double? estSell = null, actSell = null;
        if (!string.IsNullOrWhiteSpace(txtEstSell.Text))
        {
            if (!TryNum(txtEstSell.Text, out double v)) { lblError.Text = "Invalid Est. Sell Price."; return; }
            estSell = v;
        }
        if (!string.IsNullOrWhiteSpace(txtActSell.Text))
        {
            if (!TryNum(txtActSell.Text, out double v)) { lblError.Text = "Invalid Actual Sell Price."; return; }
            actSell = v;
        }

        CreatedEpisode = new Episode
        {
            SeasonId = _seasonId,
            EpisodeNumber = epNum,
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

    private static bool TryNum(string s, out double v) =>
        double.TryParse(s.Replace("£", "").Trim(), out v) && v >= 0;
}

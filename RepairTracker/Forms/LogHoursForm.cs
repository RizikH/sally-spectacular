using RepairTracker.Database;
using RepairTracker.Helpers;
using RepairTracker.Models;

namespace RepairTracker.Forms;

public class LogHoursForm : Form
{
    private readonly int _seasonId;
    private readonly int _episodeNumber;
    private TextBox txtHours = null!, txtNotes = null!;
    private Label lblError = null!;

    public LogHoursForm(int seasonId, int episodeNumber)
    {
        _seasonId = seasonId;
        _episodeNumber = episodeNumber;
        InitializeComponent();
        PreFill();
    }

    private void InitializeComponent()
    {
        Text = "Log Hours";
        Size = new Size(400, 330);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppColors.Surface;
        ForeColor = AppColors.TextPrimary;
        Font = new Font("Segoe UI", 9f);

        var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = AppColors.Card };
        var lblTitle = AppColors.MakeLabel($"Log Hours — Episode {_episodeNumber}", 13f, bold: true);
        lblTitle.Location = new Point(16, 15);
        pnlTitle.Controls.Add(lblTitle);

        var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 14, 20, 0), AutoScroll = true };

        var lblPrompt = new Label
        {
            Text = $"Item added to Episode {_episodeNumber}. Log or update hours for this episode:",
            Location = new Point(0, 14),
            AutoSize = false,
            Width = 340,
            Height = 36,
            ForeColor = AppColors.TextSecond,
            Font = new Font("Segoe UI", 9.5f),
            BackColor = Color.Transparent
        };

        var lblH = new Label { Text = "Hours Worked", Location = new Point(0, 58), AutoSize = true, ForeColor = AppColors.TextSecond, BackColor = Color.Transparent };
        txtHours = new TextBox
        {
            Location = new Point(0, 78),
            Width = 150,
            BackColor = AppColors.Card,
            ForeColor = AppColors.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9.5f)
        };

        var lblN = new Label { Text = "Notes (optional)", Location = new Point(0, 112), AutoSize = true, ForeColor = AppColors.TextSecond, BackColor = Color.Transparent };
        txtNotes = new TextBox
        {
            Location = new Point(0, 132),
            Width = 340,
            Height = 60,
            Multiline = true,
            BackColor = AppColors.Card,
            ForeColor = AppColors.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9.5f)
        };

        lblError = new Label
        {
            Location = new Point(0, 200),
            AutoSize = true,
            ForeColor = AppColors.RedFg,
            Font = new Font("Segoe UI", 8.5f),
            BackColor = Color.Transparent
        };

        pnlBody.Controls.AddRange(new Control[] { lblPrompt, lblH, txtHours, lblN, txtNotes, lblError });

        var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = AppColors.Card };
        var btnSkip = AppColors.MakeBtn("Skip", AppColors.Card);
        btnSkip.Width = 90; btnSkip.Location = new Point(188, 10);
        var btnSave = AppColors.MakeBtn("Save Hours", AppColors.Accent);
        btnSave.Width = 100; btnSave.Location = new Point(286, 10);

        btnSkip.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        btnSave.Click += BtnSave_Click;

        pnlBtn.Controls.AddRange(new Control[] { btnSkip, btnSave });

        Controls.Add(pnlBody);
        Controls.Add(pnlBtn);
        Controls.Add(pnlTitle);

        AcceptButton = btnSave;
    }

    private void PreFill()
    {
        var existing = DbContext.GetHoursLog(_seasonId, _episodeNumber);
        if (existing == null) return;
        txtHours.Text = existing.HoursWorked.ToString("F1");
        txtNotes.Text = existing.Notes ?? "";
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        lblError.Text = "";

        if (!double.TryParse(txtHours.Text.Trim(), out double hours) || hours < 0)
        {
            lblError.Text = "Please enter a valid number of hours.";
            txtHours.Focus();
            return;
        }

        DbContext.UpsertHoursLog(new HoursLog
        {
            SeasonId = _seasonId,
            EpisodeNumber = _episodeNumber,
            HoursWorked = hours,
            Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim()
        });

        DialogResult = DialogResult.OK;
        Close();
    }
}

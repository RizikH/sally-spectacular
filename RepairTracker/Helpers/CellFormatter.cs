namespace RepairTracker.Helpers;

public static class CellFormatter
{
    public static void ApplyProfit(DataGridViewCell cell, double value, bool triggered)
    {
        if (!triggered)
        {
            Reset(cell);
            return;
        }
        if (value < 0)
        {
            cell.Style.BackColor = AppColors.RedBg;
            cell.Style.ForeColor = AppColors.RedFg;
        }
        else
        {
            cell.Style.BackColor = AppColors.GreenBg;
            cell.Style.ForeColor = AppColors.GreenFg;
        }
    }

    public static void ApplyValue(DataGridViewCell cell, double? value)
    {
        if (!value.HasValue) { Reset(cell); return; }
        if (value.Value < 0)
        {
            cell.Style.BackColor = AppColors.RedBg;
            cell.Style.ForeColor = AppColors.RedFg;
        }
        else
        {
            Reset(cell);
        }
    }

    public static void Reset(DataGridViewCell cell)
    {
        cell.Style.BackColor = Color.Empty;
        cell.Style.ForeColor = Color.Empty;
    }
}

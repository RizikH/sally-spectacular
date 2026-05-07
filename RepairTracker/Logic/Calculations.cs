namespace RepairTracker.Logic;

public static class Calculations
{
    public const double EbayFeeRate = 0.09;

    public static double EbayFee(double sellPrice) => sellPrice * EbayFeeRate;

    public static double EstimatedProfit(double cost, double parts, double estSellPrice)
        => estSellPrice - cost - parts - EbayFee(estSellPrice);

    public static double NetProfit(double cost, double parts, double actualSellPrice, double postage)
        => actualSellPrice - cost - parts - EbayFee(actualSellPrice) - postage;

    public static double HourlyProfit(double netProfit, double hoursWorked)
        => hoursWorked > 0 ? netProfit / hoursWorked : 0;

    public static string Gbp(double value) => $"£{value:F2}";

    public static string Gbp(double? value) => value.HasValue ? Gbp(value.Value) : "-";
}

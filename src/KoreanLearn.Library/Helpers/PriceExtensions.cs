namespace KoreanLearn.Library.Helpers;

/// <summary>價格格式化擴充方法（預設 zh-TW 環境，千分位為逗號）</summary>
public static class PriceExtensions
{
    /// <summary>格式化為台幣格式（加千分位，例如：1,250）</summary>
    public static string ToTaiwanPrice(this decimal price) => price.ToString("N0");

    /// <summary>格式化為完整台幣顯示（例如：NT$ 1,250）</summary>
    public static string ToFullTaiwanPrice(this decimal price) => $"NT$ {price:N0}";

    /// <summary>格式化為台幣格式含小數（例如：1,250.00）</summary>
    public static string ToTaiwanPriceWithDecimal(this decimal price) => price.ToString("N2");
}

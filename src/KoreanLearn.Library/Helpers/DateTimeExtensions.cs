namespace KoreanLearn.Library.Helpers;

/// <summary>日期時間格式化擴充方法</summary>
public static class DateTimeExtensions
{
    /// <summary>格式化為台灣日期（yyyy-MM-dd）</summary>
    public static string ToTaiwanDate(this DateTime date) => date.ToString("yyyy-MM-dd");

    /// <summary>格式化為台灣日期時間（yyyy-MM-dd HH:mm）</summary>
    public static string ToTaiwanDateTime(this DateTime date) => date.ToString("yyyy-MM-dd HH:mm");

    /// <summary>格式化為韓國日期格式（yyyy.MM.dd）</summary>
    public static string ToKoreanDate(this DateTime date) => date.ToString("yyyy.MM.dd");
}

namespace KoreanLearn.Library.Enums;

/// <summary>LessonType 列舉擴充方法，提供中文顯示名稱與圖示 CSS 類別</summary>
public static class LessonTypeExtensions
{
    /// <summary>取得單元類型中文顯示名稱</summary>
    public static string ToDisplay(this LessonType type) => type switch
    {
        LessonType.Video => "影片",
        LessonType.Article => "文章",
        LessonType.Pdf => "PDF",
        _ => "未知"
    };

    /// <summary>取得單元類型對應的 Bootstrap Icon CSS 類別</summary>
    public static string ToIcon(this LessonType type) => type switch
    {
        LessonType.Video => "bi-play-circle",
        LessonType.Article => "bi-file-text",
        LessonType.Pdf => "bi-file-pdf",
        _ => "bi-file"
    };
}

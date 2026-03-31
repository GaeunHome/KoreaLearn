namespace KoreanLearn.Service.ViewModels.Learn;

/// <summary>影片進度儲存請求模型</summary>
public class SaveProgressRequest
{
    /// <summary>單元 ID</summary>
    public int LessonId { get; set; }
    /// <summary>觀看進度（秒）</summary>
    public int ProgressSeconds { get; set; }
}

/// <summary>字卡複習請求模型</summary>
public class ReviewRequest
{
    /// <summary>字卡 ID</summary>
    public int CardId { get; set; }
    /// <summary>複習品質評分（0-5，用於 SM-2 演算法）</summary>
    public int Quality { get; set; }
}

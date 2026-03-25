namespace KoreanLearn.Data.Entities;

/// <summary>所有實體的基底類別，提供主鍵與時間戳記欄位</summary>
public abstract class BaseEntity
{
    /// <summary>主鍵識別碼</summary>
    public int Id { get; set; }

    /// <summary>建立時間（UTC）</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>最後更新時間（UTC）</summary>
    public DateTime UpdatedAt { get; set; }
}

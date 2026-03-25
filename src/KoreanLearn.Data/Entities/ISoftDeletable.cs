namespace KoreanLearn.Data.Entities;

/// <summary>軟刪除介面，實作此介面的實體在刪除時不會從資料庫移除，而是標記 IsDeleted</summary>
public interface ISoftDeletable
{
    /// <summary>是否已被軟刪除</summary>
    bool IsDeleted { get; set; }

    /// <summary>軟刪除時間（UTC）</summary>
    DateTime? DeletedAt { get; set; }
}

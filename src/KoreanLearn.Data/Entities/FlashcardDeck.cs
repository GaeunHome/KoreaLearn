using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>字卡牌組實體，將多張字卡組織成一個主題牌組</summary>
public class FlashcardDeck : BaseEntity, ISoftDeletable
{
    /// <summary>牌組標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>牌組描述</summary>
    public string? Description { get; set; }

    /// <summary>關聯課程 ID（可選，null 表示獨立牌組）</summary>
    public int? CourseId { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public Course? Course { get; set; }
    public ICollection<Flashcard> Flashcards { get; set; } = [];
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>FlashcardDeck 的資料庫欄位與關聯設定</summary>
public class FlashcardDeckConfiguration : IEntityTypeConfiguration<FlashcardDeck>
{
    public void Configure(EntityTypeBuilder<FlashcardDeck> builder)
    {
        builder.ToTable("FlashcardDecks");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired().HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(2000);

        builder.HasOne(d => d.Course)
            .WithMany()
            .HasForeignKey(d => d.CourseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

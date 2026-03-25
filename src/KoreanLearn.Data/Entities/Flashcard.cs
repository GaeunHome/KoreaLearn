using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

/// <summary>字卡實體，記錄韓文詞彙的韓文、中文翻譯與羅馬拼音</summary>
public class Flashcard : BaseEntity
{
    /// <summary>所屬牌組 ID</summary>
    public int DeckId { get; set; }

    /// <summary>韓文文字</summary>
    public string Korean { get; set; } = string.Empty;

    /// <summary>中文翻譯</summary>
    public string Chinese { get; set; } = string.Empty;

    /// <summary>羅馬拼音</summary>
    public string? Romanization { get; set; }

    /// <summary>例句</summary>
    public string? ExampleSentence { get; set; }

    /// <summary>發音音檔路徑</summary>
    public string? AudioUrl { get; set; }

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }

    // ── 導覽屬性 ─────────────────────────────────────────
    public FlashcardDeck Deck { get; set; } = null!;
    public ICollection<FlashcardLog> Logs { get; set; } = [];
}

// ── EF Core Fluent API 設定 ─────────────────────────────
/// <summary>Flashcard 的資料庫欄位與關聯設定</summary>
public class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
{
    public void Configure(EntityTypeBuilder<Flashcard> builder)
    {
        builder.ToTable("Flashcards");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Korean)
            .IsRequired().HasMaxLength(200);

        builder.Property(f => f.Chinese)
            .IsRequired().HasMaxLength(200);

        builder.Property(f => f.Romanization)
            .HasMaxLength(200);

        builder.Property(f => f.ExampleSentence)
            .HasMaxLength(1000);

        builder.Property(f => f.AudioUrl)
            .HasMaxLength(500);

        builder.HasOne(f => f.Deck)
            .WithMany(d => d.Flashcards)
            .HasForeignKey(f => f.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.DeckId);

        builder.HasQueryFilter(f => !f.Deck.IsDeleted);
    }
}

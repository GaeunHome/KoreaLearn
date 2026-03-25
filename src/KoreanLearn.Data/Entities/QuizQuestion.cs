using KoreanLearn.Library.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Entities;

public class QuizQuestion : BaseEntity
{
    public int QuizId { get; set; }
    public string Content { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int Points { get; set; } = 1;
    public int SortOrder { get; set; }
    public string? CorrectAnswer { get; set; } // FillInBlank 的正確答案

    // Navigation
    public Quiz Quiz { get; set; } = null!;
    public ICollection<QuizOption> Options { get; set; } = [];
}

public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("QuizQuestions");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Content)
            .IsRequired().HasMaxLength(2000);

        builder.Property(q => q.CorrectAnswer)
            .HasMaxLength(500);

        builder.HasOne(q => q.Quiz)
            .WithMany(qz => qz.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => q.QuizId);

        builder.HasQueryFilter(q => !q.Quiz.IsDeleted);
    }
}

using KoreanLearn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KoreanLearn.Data.Configurations;

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

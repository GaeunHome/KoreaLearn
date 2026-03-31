using System.Linq.Expressions;
using KoreanLearn.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data;

/// <summary>應用程式資料庫上下文，繼承 IdentityDbContext 並整合軟刪除與時間戳記自動化</summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    // ── DbSet 定義 ───────────────────────────────────────
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonAttachment> LessonAttachments => Set<LessonAttachment>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Progress> Progresses => Set<Progress>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();
    public DbSet<FlashcardDeck> FlashcardDecks => Set<FlashcardDeck>();
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<FlashcardLog> FlashcardLogs => Set<FlashcardLog>();
    public DbSet<PronunciationExercise> PronunciationExercises => Set<PronunciationExercise>();
    public DbSet<PronunciationAttempt> PronunciationAttempts => Set<PronunciationAttempt>();
    public DbSet<Discussion> Discussions => Set<Discussion>();
    public DbSet<DiscussionReply> DiscussionReplies => Set<DiscussionReply>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===== 軟刪除 Global Query Filter =====
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType)) continue;

            var param = Expression.Parameter(entityType.ClrType, "e");
            var filter = Expression.Lambda(
                Expression.Equal(
                    Expression.Property(param, nameof(ISoftDeletable.IsDeleted)),
                    Expression.Constant(false)),
                param);
            builder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        // ===== 跨實體 Query Filter（過濾關聯已軟刪除的資料）=====
        builder.Entity<Enrollment>().HasQueryFilter(e => !e.Course.IsDeleted);
        builder.Entity<OrderItem>().HasQueryFilter(oi => !oi.Course.IsDeleted);
        builder.Entity<Progress>().HasQueryFilter(p => !p.Lesson.IsDeleted);
        builder.Entity<QuizQuestion>().HasQueryFilter(q => !q.Quiz.IsDeleted);
        builder.Entity<QuizOption>().HasQueryFilter(o => !o.Question.Quiz.IsDeleted);
        builder.Entity<QuizAttempt>().HasQueryFilter(a => !a.Quiz.IsDeleted);
        builder.Entity<QuizAnswer>().HasQueryFilter(a => !a.Attempt.Quiz.IsDeleted);
        builder.Entity<Flashcard>().HasQueryFilter(f => !f.Deck.IsDeleted);
        builder.Entity<FlashcardLog>().HasQueryFilter(l => !l.Flashcard.Deck.IsDeleted);
        builder.Entity<PronunciationAttempt>().HasQueryFilter(a => !a.Exercise.IsDeleted);
        builder.Entity<Certificate>().HasQueryFilter(c => !c.Course.IsDeleted);
        builder.Entity<UserSubscription>().HasQueryFilter(s => !s.Plan.IsDeleted);

        // ===== 欄位約束（原 DataAnnotations 轉 Fluent API）=====

        // ── BaseEntity：RowVersion 並行衝突欄位 ──
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;
            builder.Entity(entityType.ClrType).Property("RowVersion").IsRowVersion();
        }

        // ── Course ──
        builder.Entity<Course>(e =>
        {
            e.Property(c => c.Title).IsRequired().HasMaxLength(200);
            e.Property(c => c.Description).HasMaxLength(4000);
            e.Property(c => c.CoverImageUrl).HasMaxLength(500);
            e.Property(c => c.Price).HasPrecision(18, 2);
            e.Property(c => c.TeacherId).HasMaxLength(450);
        });

        // ── Section ──
        builder.Entity<Section>(e =>
        {
            e.Property(s => s.Title).IsRequired().HasMaxLength(200);
            e.Property(s => s.Description).HasMaxLength(2000);
        });

        // ── Lesson ──
        builder.Entity<Lesson>(e =>
        {
            e.Property(l => l.Title).IsRequired().HasMaxLength(200);
            e.Property(l => l.Description).HasMaxLength(2000);
            e.Property(l => l.VideoUrl).HasMaxLength(500);
            e.Property(l => l.PdfUrl).HasMaxLength(500);
            e.Property(l => l.PdfFileName).HasMaxLength(200);
        });

        // ── LessonAttachment ──
        builder.Entity<LessonAttachment>(e =>
        {
            e.Property(a => a.FileName).IsRequired().HasMaxLength(500);
            e.Property(a => a.FileUrl).IsRequired().HasMaxLength(1000);
        });

        // ── Order ──
        builder.Entity<Order>(e =>
        {
            e.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);
            e.Property(o => o.UserId).IsRequired();
        });

        // ── OrderItem ──
        builder.Entity<OrderItem>(e =>
        {
            e.Property(oi => oi.Price).HasPrecision(18, 2);
        });

        // ── Quiz ──
        builder.Entity<Quiz>(e =>
        {
            e.Property(q => q.Title).IsRequired().HasMaxLength(200);
            e.Property(q => q.Description).HasMaxLength(2000);
        });

        // ── QuizQuestion ──
        builder.Entity<QuizQuestion>(e =>
        {
            e.Property(q => q.Content).IsRequired().HasMaxLength(2000);
            e.Property(q => q.CorrectAnswer).HasMaxLength(500);
        });

        // ── QuizOption ──
        builder.Entity<QuizOption>(e =>
        {
            e.Property(o => o.Content).IsRequired().HasMaxLength(1000);
        });

        // ── QuizAnswer ──
        builder.Entity<QuizAnswer>(e =>
        {
            e.Property(a => a.TextAnswer).HasMaxLength(1000);
        });

        // ── FlashcardDeck ──
        builder.Entity<FlashcardDeck>(e =>
        {
            e.Property(d => d.Title).IsRequired().HasMaxLength(200);
            e.Property(d => d.Description).HasMaxLength(2000);
        });

        // ── Flashcard ──
        builder.Entity<Flashcard>(e =>
        {
            e.Property(f => f.Korean).IsRequired().HasMaxLength(200);
            e.Property(f => f.Chinese).IsRequired().HasMaxLength(200);
            e.Property(f => f.Romanization).HasMaxLength(200);
            e.Property(f => f.ExampleSentence).HasMaxLength(1000);
            e.Property(f => f.AudioUrl).HasMaxLength(500);
        });

        // ── PronunciationExercise ──
        builder.Entity<PronunciationExercise>(e =>
        {
            e.Property(p => p.Korean).IsRequired().HasMaxLength(200);
            e.Property(p => p.Romanization).HasMaxLength(200);
            e.Property(p => p.Chinese).HasMaxLength(200);
            e.Property(p => p.StandardAudioUrl).IsRequired().HasMaxLength(500);
        });

        // ── PronunciationAttempt ──
        builder.Entity<PronunciationAttempt>(e =>
        {
            e.Property(a => a.RecordingUrl).IsRequired().HasMaxLength(500);
        });

        // ── Discussion ──
        builder.Entity<Discussion>(e =>
        {
            e.Property(d => d.Title).IsRequired().HasMaxLength(200);
            e.Property(d => d.Content).IsRequired().HasMaxLength(4000);
        });

        // ── DiscussionReply ──
        builder.Entity<DiscussionReply>(e =>
        {
            e.Property(r => r.Content).IsRequired().HasMaxLength(4000);
        });

        // ── SubscriptionPlan ──
        builder.Entity<SubscriptionPlan>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.Description).HasMaxLength(2000);
            e.Property(p => p.MonthlyPrice).HasPrecision(18, 2);
        });

        // ── Certificate ──
        builder.Entity<Certificate>(e =>
        {
            e.Property(c => c.CertificateNumber).IsRequired().HasMaxLength(50);
            e.Property(c => c.PdfUrl).HasMaxLength(500);
        });

        // ── Announcement ──
        builder.Entity<Announcement>(e =>
        {
            e.Property(a => a.Title).IsRequired().HasMaxLength(200);
            e.Property(a => a.Content).IsRequired().HasMaxLength(4000);
        });

        // ── SystemSetting ──
        builder.Entity<SystemSetting>(e =>
        {
            e.Property(s => s.Key).IsRequired().HasMaxLength(100);
            e.Property(s => s.Value).IsRequired().HasMaxLength(2000);
            e.Property(s => s.Description).HasMaxLength(500);
            e.Property(s => s.Group).HasMaxLength(50);
        });

        // ── Banner ──
        builder.Entity<Banner>(e =>
        {
            e.Property(b => b.Title).HasMaxLength(100);
            e.Property(b => b.ImageUrl).IsRequired().HasMaxLength(500);
        });

        // ── PasswordHistory ──
        builder.Entity<PasswordHistory>(e =>
        {
            e.Property(p => p.PasswordHash).IsRequired();
            e.Property(p => p.UserId).IsRequired();
        });

        // ===== Course 關聯 =====
        builder.Entity<Course>()
            .HasOne(c => c.Teacher)
            .WithMany(u => u.TeacherCourses)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Course>().HasIndex(c => c.Title);
        builder.Entity<Course>().HasIndex(c => c.IsPublished);
        builder.Entity<Course>().HasIndex(c => c.TeacherId);

        // ===== Section 關聯 =====
        builder.Entity<Section>()
            .HasOne(s => s.Course)
            .WithMany(c => c.Sections)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Section>().HasIndex(s => s.CourseId);

        // ===== Lesson 關聯 =====
        builder.Entity<Lesson>()
            .HasOne(l => l.Section)
            .WithMany(s => s.Lessons)
            .HasForeignKey(l => l.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lesson>().HasIndex(l => l.SectionId);

        // ===== LessonAttachment 關聯 =====
        builder.Entity<LessonAttachment>()
            .HasOne(a => a.Lesson)
            .WithMany(l => l.Attachments)
            .HasForeignKey(a => a.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<LessonAttachment>().HasIndex(a => a.LessonId);

        // ===== Enrollment 關聯 =====
        builder.Entity<Enrollment>()
            .HasOne(e => e.User)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Enrollment>()
            .HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
        builder.Entity<Enrollment>().HasIndex(e => e.UserId);

        // ===== Order 關聯 =====
        builder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasIndex(o => o.OrderNumber).IsUnique();
        builder.Entity<Order>().HasIndex(o => o.UserId);

        // ===== OrderItem 關聯 =====
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Course)
            .WithMany()
            .HasForeignKey(oi => oi.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderItem>().HasIndex(oi => oi.OrderId);

        // ===== Progress 關聯 =====
        builder.Entity<Progress>()
            .HasOne(p => p.User)
            .WithMany(u => u.Progresses)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Progress>()
            .HasOne(p => p.Lesson)
            .WithMany(l => l.Progresses)
            .HasForeignKey(p => p.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Progress>()
            .HasIndex(p => new { p.UserId, p.LessonId }).IsUnique();
        builder.Entity<Progress>().HasIndex(p => p.UserId);

        // ===== Quiz 關聯（一對一：Lesson ↔ Quiz）=====
        builder.Entity<Quiz>()
            .HasOne(q => q.Lesson)
            .WithOne(l => l.Quiz)
            .HasForeignKey<Quiz>(q => q.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quiz>()
            .HasIndex(q => q.LessonId).IsUnique();

        // ===== QuizQuestion 關聯 =====
        builder.Entity<QuizQuestion>()
            .HasOne(q => q.Quiz)
            .WithMany(qz => qz.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuizQuestion>().HasIndex(q => q.QuizId);

        // ===== QuizOption 關聯 =====
        builder.Entity<QuizOption>()
            .HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuizOption>().HasIndex(o => o.QuestionId);

        // ===== QuizAttempt 關聯 =====
        builder.Entity<QuizAttempt>()
            .HasOne(a => a.User)
            .WithMany(u => u.QuizAttempts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuizAttempt>()
            .HasOne(a => a.Quiz)
            .WithMany(q => q.Attempts)
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuizAttempt>().HasIndex(a => a.UserId);
        builder.Entity<QuizAttempt>().HasIndex(a => a.QuizId);

        // ===== QuizAnswer 關聯 =====
        builder.Entity<QuizAnswer>()
            .HasOne(a => a.Attempt)
            .WithMany(at => at.Answers)
            .HasForeignKey(a => a.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuizAnswer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuizAnswer>()
            .HasOne(a => a.SelectedOption)
            .WithMany()
            .HasForeignKey(a => a.SelectedOptionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<QuizAnswer>().HasIndex(a => a.AttemptId);

        // ===== FlashcardDeck 關聯 =====
        builder.Entity<FlashcardDeck>()
            .HasOne(d => d.Course)
            .WithMany()
            .HasForeignKey(d => d.CourseId)
            .OnDelete(DeleteBehavior.SetNull);

        // ===== Flashcard 關聯 =====
        builder.Entity<Flashcard>()
            .HasOne(f => f.Deck)
            .WithMany(d => d.Flashcards)
            .HasForeignKey(f => f.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Flashcard>().HasIndex(f => f.DeckId);

        // ===== FlashcardLog 關聯 =====
        builder.Entity<FlashcardLog>()
            .HasOne(l => l.User)
            .WithMany(u => u.FlashcardLogs)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FlashcardLog>()
            .HasOne(l => l.Flashcard)
            .WithMany(f => f.Logs)
            .HasForeignKey(l => l.FlashcardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FlashcardLog>()
            .HasIndex(l => new { l.UserId, l.FlashcardId });
        builder.Entity<FlashcardLog>().HasIndex(l => l.NextReviewDate);

        // ===== Discussion 關聯 =====
        builder.Entity<Discussion>()
            .HasOne(d => d.User)
            .WithMany(u => u.Discussions)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Discussion>()
            .HasOne(d => d.Course)
            .WithMany(c => c.Discussions)
            .HasForeignKey(d => d.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Discussion>().HasIndex(d => d.UserId);
        builder.Entity<Discussion>().HasIndex(d => d.CourseId);

        // ===== DiscussionReply 關聯 =====
        builder.Entity<DiscussionReply>()
            .HasOne(r => r.User)
            .WithMany(u => u.DiscussionReplies)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DiscussionReply>()
            .HasOne(r => r.Discussion)
            .WithMany(d => d.Replies)
            .HasForeignKey(r => r.DiscussionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DiscussionReply>().HasIndex(r => r.DiscussionId);

        // ===== PronunciationExercise 關聯 =====
        builder.Entity<PronunciationExercise>()
            .HasOne(p => p.Lesson)
            .WithMany()
            .HasForeignKey(p => p.LessonId)
            .OnDelete(DeleteBehavior.SetNull);

        // ===== PronunciationAttempt 關聯 =====
        builder.Entity<PronunciationAttempt>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PronunciationAttempt>()
            .HasOne(a => a.Exercise)
            .WithMany(e => e.Attempts)
            .HasForeignKey(a => a.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PronunciationAttempt>().HasIndex(a => a.UserId);
        builder.Entity<PronunciationAttempt>().HasIndex(a => a.ExerciseId);

        // ===== Certificate 關聯 =====
        builder.Entity<Certificate>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Certificate>()
            .HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Certificate>()
            .HasIndex(c => c.CertificateNumber).IsUnique();
        builder.Entity<Certificate>()
            .HasIndex(c => new { c.UserId, c.CourseId }).IsUnique();

        // ===== UserSubscription 關聯（一對一：AppUser ↔ UserSubscription）=====
        builder.Entity<UserSubscription>()
            .HasOne(s => s.User)
            .WithOne(u => u.ActiveSubscription)
            .HasForeignKey<UserSubscription>(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserSubscription>()
            .HasOne(s => s.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserSubscription>().HasIndex(s => s.UserId);

        // ===== Banner 關聯 =====
        builder.Entity<Banner>()
            .HasOne(b => b.Course)
            .WithMany()
            .HasForeignKey(b => b.CourseId)
            .OnDelete(DeleteBehavior.SetNull);

        // ===== PasswordHistory 關聯 =====
        builder.Entity<PasswordHistory>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PasswordHistory>().HasIndex(p => p.UserId);

        // ===== SystemSetting 唯一索引 =====
        builder.Entity<SystemSetting>()
            .HasIndex(s => s.Key).IsUnique();

        // ===== Announcement 索引 =====
        builder.Entity<Announcement>().HasIndex(a => a.IsActive);
    }

    // ── 覆寫 SaveChangesAsync：軟刪除攔截與時間戳記自動化 ──
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>()
                     .Where(e => e.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTime.UtcNow;
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(ct);
    }
}

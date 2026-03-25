namespace KoreanLearn.Data.Entities;

public class FlashcardDeck : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CourseId { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Course? Course { get; set; }
    public ICollection<Flashcard> Flashcards { get; set; } = [];
}

using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class FlashcardDeckRepository(ApplicationDbContext db) : Repository<FlashcardDeck>(db), IFlashcardDeckRepository
{
    public async Task<FlashcardDeck?> GetWithCardsAsync(int id, CancellationToken ct = default)
        => await DbSet
            .Include(d => d.Flashcards.OrderBy(f => f.SortOrder))
            .FirstOrDefaultAsync(d => d.Id == id, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<FlashcardDeck>> GetByCourseIdAsync(int courseId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(d => d.CourseId == courseId)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<Flashcard?> GetCardByIdAsync(int cardId, CancellationToken ct = default)
        => await db.Set<Flashcard>()
            .Include(c => c.Deck)
            .FirstOrDefaultAsync(c => c.Id == cardId, ct).ConfigureAwait(false);
}

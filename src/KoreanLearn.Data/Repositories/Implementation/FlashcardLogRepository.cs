using KoreanLearn.Data.Entities;
using KoreanLearn.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KoreanLearn.Data.Repositories.Implementation;

public class FlashcardLogRepository(ApplicationDbContext db) : Repository<FlashcardLog>(db), IFlashcardLogRepository
{
    public async Task<FlashcardLog?> GetByUserAndCardAsync(
        string userId, int flashcardId, CancellationToken ct = default)
        => await DbSet
            .FirstOrDefaultAsync(l => l.UserId == userId && l.FlashcardId == flashcardId, ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<FlashcardLog>> GetDueCardsAsync(
        string userId, int deckId, CancellationToken ct = default)
        => await DbSet
            .Include(l => l.Flashcard)
            .Where(l => l.UserId == userId
                && l.Flashcard.DeckId == deckId
                && l.NextReviewDate <= DateTime.UtcNow)
            .OrderBy(l => l.NextReviewDate)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<FlashcardLog>> GetByUserAndDeckAsync(
        string userId, int deckId, CancellationToken ct = default)
        => await DbSet
            .Include(l => l.Flashcard)
            .Where(l => l.UserId == userId && l.Flashcard.DeckId == deckId)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<int> CountDueForUserAsync(string userId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(l => l.UserId == userId && l.NextReviewDate <= DateTime.UtcNow)
            .CountAsync(ct).ConfigureAwait(false);
}

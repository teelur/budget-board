using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing transaction tags.
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Applies tag additions and removals to a transaction.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="transaction">The transaction to update.</param>
    /// <param name="addTags">The tag values to add.</param>
    /// <param name="removeTags">The tag values to remove.</param>
    /// <returns>The unique identifiers of the tags removed from the transaction.</returns>
    Task<IReadOnlyCollection<Guid>> ApplyTagChangesAsync(
        Guid userGuid,
        Transaction transaction,
        IEnumerable<string>? addTags,
        IEnumerable<string>? removeTags
    );

    /// <summary>
    /// Removes all tags from a transaction.
    /// </summary>
    /// <param name="transaction">The transaction whose tags should be removed.</param>
    /// <returns>The unique identifiers of the tags removed from the transaction.</returns>
    Task<IReadOnlyCollection<Guid>> RemoveAllTagsAsync(Transaction transaction);

    /// <summary>
    /// Marks the specified tags for deletion when they are no longer associated with a transaction.
    /// This only changes the current EF unit of work; the caller owns SaveChangesAsync so deferred
    /// transaction operations can commit all related changes together.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="tagIds">The unique identifiers of the tags to check.</param>
    Task DeleteOrphanedTagsAsync(Guid userGuid, IEnumerable<Guid> tagIds);

    /// <summary>
    /// Retrieves tag suggestions for a user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="prefix">The optional prefix used to filter suggestions.</param>
    /// <param name="limit">The maximum number of suggestions to return.</param>
    /// <returns>A list of matching tag suggestions.</returns>
    Task<IReadOnlyList<string>> ReadSuggestionsAsync(Guid userGuid, string? prefix, int limit);
}

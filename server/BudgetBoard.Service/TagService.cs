using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.Service;

public class TagService(
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer
) : ITagService
{
    private const int DefaultSuggestionLimit = 20;
    private const int MaximumSuggestionLimit = 50;

    public async Task<IReadOnlyCollection<Guid>> ApplyTagChangesAsync(
        Guid userGuid,
        Transaction transaction,
        IEnumerable<string>? addTags,
        IEnumerable<string>? removeTags
    )
    {
        var additions = NormalizeValues(addTags);
        var removals = NormalizeValues(removeTags);
        var overlappingValues = additions.Keys.Intersect(removals.Keys).ToList();
        if (overlappingValues.Count > 0)
        {
            throw new BudgetBoardServiceException(responseLocalizer["TagAddRemoveOverlapError"]);
        }

        var currentLinks = await userDataContext
            .TransactionTags.Where(link => link.TransactionID == transaction.ID)
            .Include(link => link.Tag)
            .ToListAsync();

        var unsavedLinks = transaction.TransactionTags.Where(link =>
            userDataContext.Entry(link).State != EntityState.Deleted
            && currentLinks.All(current =>
                current.TransactionID != link.TransactionID || current.TagID != link.TagID
            )
        );
        currentLinks.AddRange(unsavedLinks);

        var linksToRemove = currentLinks
            .Where(link => link.Tag != null && removals.ContainsKey(link.Tag.NormalizedValue))
            .ToList();
        userDataContext.TransactionTags.RemoveRange(linksToRemove);
        foreach (var link in linksToRemove)
        {
            transaction.TransactionTags.Remove(link);
        }

        var currentValues = currentLinks
            .Where(link => !linksToRemove.Contains(link) && link.Tag != null)
            .Select(link => link.Tag!.NormalizedValue)
            .ToHashSet(StringComparer.Ordinal);

        var tagsByNormalizedValue = await userDataContext
            .Tags.Where(tag =>
                tag.UserID == userGuid && additions.Keys.Contains(tag.NormalizedValue)
            )
            .ToDictionaryAsync(tag => tag.NormalizedValue, StringComparer.Ordinal);

        foreach (var (normalizedValue, displayValue) in additions)
        {
            if (currentValues.Contains(normalizedValue))
            {
                continue;
            }

            if (!tagsByNormalizedValue.TryGetValue(normalizedValue, out var tag))
            {
                tag = new Tag
                {
                    UserID = userGuid,
                    Value = displayValue,
                    NormalizedValue = normalizedValue,
                };
                userDataContext.Tags.Add(tag);
                tagsByNormalizedValue.Add(normalizedValue, tag);
            }

            var link = new TransactionTag
            {
                TransactionID = transaction.ID,
                TagID = tag.ID,
                Transaction = transaction,
                Tag = tag,
            };
            userDataContext.TransactionTags.Add(link);
            transaction.TransactionTags.Add(link);
        }

        return linksToRemove.Select(link => link.TagID).ToHashSet();
    }

    public async Task<IReadOnlyCollection<Guid>> RemoveAllTagsAsync(Transaction transaction)
    {
        var links = transaction.TransactionTags.ToList();
        if (links.Count == 0)
        {
            links = await userDataContext
                .TransactionTags.Where(link => link.TransactionID == transaction.ID)
                .ToListAsync();
        }

        userDataContext.TransactionTags.RemoveRange(links);
        foreach (var link in links)
        {
            transaction.TransactionTags.Remove(link);
        }

        return links.Select(link => link.TagID).ToHashSet();
    }

    public async Task DeleteOrphanedTagsAsync(Guid userGuid, IEnumerable<Guid> tagIds)
    {
        var uniqueTagIds = tagIds.Distinct().ToList();
        if (uniqueTagIds.Count == 0)
        {
            return;
        }

        var deletedLinkKeys = userDataContext
            .ChangeTracker.Entries<TransactionTag>()
            .Where(entry => entry.State == EntityState.Deleted)
            .Select(entry => (entry.Entity.TransactionID, entry.Entity.TagID))
            .ToHashSet();

        var persistedLinks = await userDataContext
            .TransactionTags.AsNoTracking()
            .Where(link => uniqueTagIds.Contains(link.TagID))
            .Select(link => new { link.TransactionID, link.TagID })
            .ToListAsync();

        var persistedTagIds = persistedLinks
            .Where(link => !deletedLinkKeys.Contains((link.TransactionID, link.TagID)))
            .Select(link => link.TagID);
        var trackedTagIds = userDataContext
            .ChangeTracker.Entries<TransactionTag>()
            .Where(entry => entry.State != EntityState.Deleted)
            .Select(entry => entry.Entity.TagID);
        var activeTagIds = persistedTagIds.Concat(trackedTagIds).ToHashSet();

        var orphanedTags = await userDataContext
            .Tags.Where(tag =>
                tag.UserID == userGuid
                && uniqueTagIds.Contains(tag.ID)
                && !activeTagIds.Contains(tag.ID)
            )
            .ToListAsync();

        if (orphanedTags.Count == 0)
        {
            return;
        }

        userDataContext.Tags.RemoveRange(orphanedTags);
    }

    public async Task<IReadOnlyList<string>> ReadSuggestionsAsync(
        Guid userGuid,
        string? prefix,
        int limit
    )
    {
        var boundedLimit =
            limit <= 0 ? DefaultSuggestionLimit : Math.Min(limit, MaximumSuggestionLimit);
        var normalizedPrefix = string.IsNullOrWhiteSpace(prefix)
            ? null
            : prefix.Trim().ToUpperInvariant();

        var query = userDataContext
            .TransactionTags.AsNoTracking()
            .Where(link =>
                link.Tag != null
                && link.Tag.UserID == userGuid
                && link.Transaction != null
                && link.Transaction.Deleted == null
            )
            .GroupBy(link => new
            {
                link.TagID,
                Value = link.Tag!.Value,
                NormalizedValue = link.Tag!.NormalizedValue,
            })
            .Select(group => new
            {
                group.Key.Value,
                group.Key.NormalizedValue,
                UsageCount = group.Count(),
            });

        if (normalizedPrefix != null)
        {
            query = query.Where(tag => tag.NormalizedValue.StartsWith(normalizedPrefix));
        }

        return await query
            .OrderByDescending(tag => tag.UsageCount)
            .ThenBy(tag => tag.NormalizedValue)
            .Take(boundedLimit)
            .Select(tag => tag.Value)
            .ToListAsync();
    }

    private Dictionary<string, string> NormalizeValues(IEnumerable<string>? values)
    {
        var normalizedValues = new Dictionary<string, string>(StringComparer.Ordinal);
        if (values == null)
        {
            return normalizedValues;
        }

        foreach (var rawValue in values)
        {
            var displayValue = rawValue?.Trim();
            if (string.IsNullOrEmpty(displayValue))
            {
                throw new BudgetBoardServiceException(responseLocalizer["TagValueEmptyError"]);
            }

            if (displayValue.Length > Tag.MaxValueLength)
            {
                throw new BudgetBoardServiceException(responseLocalizer["TagValueTooLongError"]);
            }

            normalizedValues.TryAdd(displayValue.ToUpperInvariant(), displayValue);
        }

        return normalizedValues;
    }
}

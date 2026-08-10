export interface ITagChanges {
  addTags?: string[];
  removeTags?: string[];
}

export const normalizeTag = (value: string): string =>
  value.trim().toUpperCase();

export const getUniqueTags = (tags: readonly string[]): string[] => {
  const seen = new Set<string>();
  const uniqueTags: string[] = [];

  tags.forEach((tag) => {
    const normalizedTag = normalizeTag(tag);
    if (normalizedTag.length === 0 || seen.has(normalizedTag)) {
      return;
    }

    seen.add(normalizedTag);
    uniqueTags.push(tag);
  });

  return uniqueTags;
};

export const getTrimmedUniqueTags = (tags: readonly string[]): string[] =>
  getUniqueTags(tags.map((tag) => tag.trim()));

export const getTagChanges = (
  currentTags: readonly string[],
  desiredTags: readonly string[],
): ITagChanges => {
  const uniqueCurrentTags = getUniqueTags(currentTags);
  const uniqueDesiredTags = getTrimmedUniqueTags(desiredTags);
  const currentTagValues = new Set(uniqueCurrentTags.map(normalizeTag));
  const desiredTagValues = new Set(uniqueDesiredTags.map(normalizeTag));

  const addTags = uniqueDesiredTags.filter(
    (tag) => !currentTagValues.has(normalizeTag(tag)),
  );
  const removeTags = uniqueCurrentTags.filter(
    (tag) => !desiredTagValues.has(normalizeTag(tag)),
  );

  return {
    ...(addTags.length > 0 ? { addTags } : {}),
    ...(removeTags.length > 0 ? { removeTags } : {}),
  };
};

import { MultiSelect, MultiSelectProps, Pill } from "@mantine/core";
import { useDebouncedValue } from "@mantine/hooks";
import { AxiosError } from "axios";
import React from "react";
import { useTranslation } from "react-i18next";
import {
  getTrimmedUniqueTags,
  getUniqueTags,
  normalizeTag,
} from "~/helpers/tags";
import { translateAxiosError } from "~/helpers/requests";
import { useTagSuggestionsQuery } from "~/hooks/queries/useTagSuggestionsQuery";
import classes from "./TransactionTagsInput.module.css";

export interface TransactionTagsInputProps extends Omit<
  MultiSelectProps,
  | "data"
  | "value"
  | "defaultValue"
  | "onChange"
  | "searchValue"
  | "defaultSearchValue"
  | "onSearchChange"
  | "renderPill"
> {
  value: string[];
  onChange: (value: string[]) => void;
  existingTags?: string[];
  elevation?: number;
}

const tagSuggestionsLimit = 20;

const TransactionTagsInput = ({
  value,
  onChange,
  existingTags = [],
  disabled,
  elevation = 0,
  error,
  ...props
}: TransactionTagsInputProps): React.ReactNode => {
  const { t } = useTranslation();
  const [searchValue, setSearchValue] = React.useState("");
  const [debouncedSearchValue] = useDebouncedValue(searchValue, 250);
  const suggestionsQuery = useTagSuggestionsQuery({
    prefix: debouncedSearchValue,
    limit: tagSuggestionsLimit,
    enabled: !disabled,
  });

  const selectedTagValues = new Set(value.map(normalizeTag));
  const suggestionOptions = getUniqueTags([
    ...existingTags,
    ...(suggestionsQuery.data ?? []),
  ]).filter((tag) => !selectedTagValues.has(normalizeTag(tag)));
  const customTag = searchValue.trim();
  const options =
    customTag.length > 0 &&
    !selectedTagValues.has(normalizeTag(customTag)) &&
    !suggestionOptions.some(
      (tag) => normalizeTag(tag) === normalizeTag(customTag),
    )
      ? [...suggestionOptions, customTag]
      : suggestionOptions;
  const suggestionError = suggestionsQuery.isError
    ? t("tag_suggestions_error", {
        error: translateAxiosError(suggestionsQuery.error as AxiosError),
      })
    : undefined;

  return (
    <MultiSelect
      {...props}
      data-elevation={elevation}
      value={value}
      onChange={(nextValue) => onChange(getTrimmedUniqueTags(nextValue))}
      searchValue={searchValue}
      onSearchChange={setSearchValue}
      data={options}
      disabled={disabled}
      searchable
      clearable
      hidePickedOptions
      nothingFoundMessage={
        suggestionError ??
        (suggestionsQuery.isFetching
          ? t("loading_tag_suggestions")
          : t("no_tag_suggestions"))
      }
      classNames={{
        input: classes.input,
        pillsList: classes.pillsList,
      }}
      error={error ?? suggestionError}
      aria-busy={suggestionsQuery.isFetching}
      renderPill={({
        option,
        value: pillValue,
        onRemove,
        disabled: pillDisabled,
      }) => {
        const tagValue = String(
          option?.label ?? option?.value ?? pillValue ?? "",
        );

        return (
          <Pill
            size="sm"
            withRemoveButton
            onRemove={onRemove}
            disabled={pillDisabled}
            removeButtonProps={{
              "aria-label": t("remove_tag", { tag: tagValue }),
            }}
          >
            {tagValue}
          </Pill>
        );
      }}
    />
  );
};

export default TransactionTagsInput;

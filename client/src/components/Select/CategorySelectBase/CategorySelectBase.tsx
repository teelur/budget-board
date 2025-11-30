import dropdownClasses from "~/styles/Dropdown.module.css";

import {
  buildCategoriesTree,
  getFormattedCategoryValue,
  getIsParentCategory,
} from "~/helpers/category";
import { areStringsEqual } from "~/helpers/utils";
import {
  CheckIcon,
  Combobox,
  Group,
  Input,
  InputBase,
  InputBaseProps,
  Text,
  useCombobox,
} from "@mantine/core";
import { ICategory, ICategoryNode } from "~/models/category";
import React from "react";
import { uncategorizedTransactionCategory } from "~/models/transaction";

export interface CategorySelectBaseProps extends InputBaseProps {
  categories: ICategory[];
  value: string;
  onChange: (value: string) => void;
  withinPortal?: boolean;
  includeUncategorized?: boolean;
}

const CategorySelectBase = ({
  categories,
  value,
  onChange,
  withinPortal,
  includeUncategorized,
  ...props
}: CategorySelectBaseProps): React.ReactNode => {
  const [search, setSearch] = React.useState("");

  const combobox = useCombobox({
    onDropdownClose: () => {
      combobox.focusTarget();
      setSearch("");
    },
    onDropdownOpen: () => {
      combobox.focusSearchInput();
    },
  });

  const categoriesTree = React.useMemo(
    () => buildCategoriesTree(categories),
    [categories]
  );

  const buildCategoriesOptions = (categoriesTree: ICategoryNode[]) => {
    const options: React.ReactNode[] = [];
    categoriesTree.forEach((category) => {
      if (category.value.toLowerCase().includes(search.toLowerCase().trim())) {
        options.push(
          <Combobox.Option
            key={category.value}
            value={category.value}
            active={areStringsEqual(category.value, value)}
          >
            <Group gap="0.5rem">
              {areStringsEqual(category.value, value) ? (
                <CheckIcon size={12} />
              ) : (
                <div style={{ width: 12 }} />
              )}
              <Text
                fz="sm"
                style={{
                  fontWeight: getIsParentCategory(category.value, categories)
                    ? 700
                    : 400,
                  textWrap: "nowrap",
                }}
                pl={getIsParentCategory(category.value, categories) ? 0 : 10}
              >
                {category.value}
              </Text>
            </Group>
          </Combobox.Option>
        );
      }
      if (category?.subCategories.length > 0) {
        options.push(
          ...buildCategoriesOptions(
            category.subCategories.sort((a, b) =>
              a.value
                .toLocaleLowerCase()
                .localeCompare(b.value.toLocaleLowerCase())
            )
          )
        );
      }
    });

    return options;
  };

  const categoryOptions = (): React.ReactNode => {
    const options = buildCategoriesOptions(categoriesTree);
    if (
      includeUncategorized &&
      uncategorizedTransactionCategory
        .toLowerCase()
        .includes(search.toLowerCase().trim())
    ) {
      options.push(
        <Combobox.Option
          key={uncategorizedTransactionCategory}
          value={uncategorizedTransactionCategory}
          active={areStringsEqual(uncategorizedTransactionCategory, value)}
        >
          <Group gap="0.5rem">
            {areStringsEqual(uncategorizedTransactionCategory, value) ? (
              <CheckIcon size={12} />
            ) : (
              <div style={{ width: 12 }} />
            )}
            <Text
              fz="sm"
              style={{
                fontWeight: 700,
                textWrap: "nowrap",
              }}
            >
              {uncategorizedTransactionCategory}
            </Text>
          </Group>
        </Combobox.Option>
      );
    }
    return options;
  };

  return (
    <Combobox
      classNames={{
        dropdown: dropdownClasses.dropdown,
        search: dropdownClasses.search,
      }}
      store={combobox}
      onOptionSubmit={(val) => {
        if (areStringsEqual(val, value)) {
          onChange("");
        } else {
          onChange(val);
        }
        combobox.closeDropdown();
      }}
      withinPortal={withinPortal ?? false}
    >
      <Combobox.Target>
        <InputBase
          miw={props.miw ?? 200}
          component="button"
          type="button"
          rightSection={<Combobox.Chevron />}
          onClick={() => combobox.toggleDropdown()}
          rightSectionPointerEvents="none"
          multiline
          pointer
          {...props}
        >
          {value ? (
            <Text fz="sm">{getFormattedCategoryValue(value, categories)}</Text>
          ) : (
            <Input.Placeholder>Pick value</Input.Placeholder>
          )}
        </InputBase>
      </Combobox.Target>
      <Combobox.Dropdown miw="max-content">
        <Combobox.Search
          value={search}
          onChange={(event) => setSearch(event.currentTarget.value)}
          placeholder="Search Categories"
          size="sm"
        />
        <Combobox.Options mah={300} style={{ overflowY: "auto" }}>
          {categoryOptions()}
        </Combobox.Options>
      </Combobox.Dropdown>
    </Combobox>
  );
};

export default CategorySelectBase;

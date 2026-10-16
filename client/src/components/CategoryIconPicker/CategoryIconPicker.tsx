import classes from "./CategoryIconPicker.module.css";

import { ScrollArea, SimpleGrid, Stack, UnstyledButton } from "@mantine/core";
import {
  ActionIcon,
  BodyText,
  Button,
  Popover,
  TextInput,
} from "@teelur/budget-board-ui";
import { SmilePlusIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import {
  filterCategoryIconGroups,
  ICategoryIconGroup,
} from "~/helpers/categoryIcons";
import { useSetTransactionCategoryIconMutation } from "~/hooks/mutations/transactionCategories/useSetTransactionCategoryIconMutation";

interface CategoryIconPickerProps {
  category: string;
  icon: string;
  size?: string;
}

const CategoryIconPicker = (
  props: CategoryIconPickerProps,
): React.ReactNode => {
  const [isPickerOpen, setIsPickerOpen] = React.useState(false);
  const [search, setSearch] = React.useState("");

  const { t } = useTranslation();
  const setTransactionCategoryIconMutation =
    useSetTransactionCategoryIconMutation();

  const getGroupLabel = React.useCallback(
    (group: ICategoryIconGroup) => t(group.labelKey),
    [t],
  );

  const groups = React.useMemo(
    () => filterCategoryIconGroups(search, getGroupLabel),
    [search, getGroupLabel],
  );

  // The mutation stays pending until its query invalidation settles, so this
  // also covers the window where props.icon is still the previous value.
  const isSaving = setTransactionCategoryIconMutation.isPending;

  const openPicker = () => {
    if (isSaving) {
      return;
    }
    setIsPickerOpen(true);
  };

  const closePicker = () => {
    setIsPickerOpen(false);
    setSearch("");
  };

  const handlePick = (icon: string) => {
    if (isSaving) {
      return;
    }
    closePicker();
    if (icon === props.icon) {
      return;
    }
    setTransactionCategoryIconMutation.mutate({
      category: props.category,
      icon,
    });
  };

  return (
    <Popover
      opened={isPickerOpen}
      onChange={(opened) => (opened ? openPicker() : closePicker())}
      position="bottom-start"
      trapFocus
      withArrow
    >
      <Popover.Target>
        <ActionIcon
          variant="ghost"
          color="primary"
          size={props.size ?? "compact-sm"}
          loading={isSaving}
          aria-label={t("set_category_icon", { category: props.category })}
          onClick={(e) => {
            e.stopPropagation();
            if (isPickerOpen) {
              closePicker();
            } else {
              openPicker();
            }
          }}
        >
          {props.icon.length > 0 ? (
            <span className={classes.icon}>{props.icon}</span>
          ) : (
            <SmilePlusIcon size={16} />
          )}
        </ActionIcon>
      </Popover.Target>
      <Popover.Dropdown w={264} onClick={(e) => e.stopPropagation()}>
        <Stack gap="0.5rem">
          <TextInput
            value={search}
            onChange={(e) => setSearch(e.currentTarget.value)}
            placeholder={t("search_icons")}
            size="xs"
          />
          {groups.length === 0 ? (
            <BodyText tone="muted" size="xs">
              {t("no_icons_found")}
            </BodyText>
          ) : (
            <ScrollArea.Autosize
              mah={260}
              type="auto"
              offsetScrollbars="present"
            >
              <Stack gap="0.5rem">
                {groups.map((group) => (
                  <Stack key={group.labelKey} gap="0.25rem">
                    <BodyText tone="secondary" size="xs">
                      {getGroupLabel(group)}
                    </BodyText>
                    <SimpleGrid
                      cols={7}
                      spacing="0.25rem"
                      verticalSpacing="0.25rem"
                    >
                      {group.icons.map((option) => (
                        <UnstyledButton
                          key={option.icon}
                          className={classes.option}
                          data-selected={
                            option.icon === props.icon || undefined
                          }
                          disabled={isSaving}
                          onClick={() => handlePick(option.icon)}
                        >
                          {option.icon}
                        </UnstyledButton>
                      ))}
                    </SimpleGrid>
                  </Stack>
                ))}
              </Stack>
            </ScrollArea.Autosize>
          )}
          {props.icon.length > 0 && (
            <Button
              variant="filled"
              color="error"
              size="compact-xs"
              disabled={isSaving}
              onClick={() => handlePick("")}
            >
              {t("remove_category_icon")}
            </Button>
          )}
        </Stack>
      </Popover.Dropdown>
    </Popover>
  );
};

export default CategoryIconPicker;

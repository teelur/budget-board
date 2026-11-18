import { Group, ScrollArea, Stack } from "@mantine/core";
import {
  Alert,
  BodyText,
  Button,
  SegmentedControl,
  Select,
  Skeleton,
  Table,
} from "@teelur/budget-board-ui";
import {
  AlertCircleIcon,
  CheckCircle2Icon,
  CheckIcon,
  MoveLeftIcon,
  TagIcon,
} from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import {
  CategoryResolutionAction,
  IBudgetTemplateCategoryItem,
  IBudgetTemplateExport,
  IMissingCategoryResolution,
} from "~/models/budgetTemplate";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

interface CategoryMappingProps {
  template: IBudgetTemplateExport;
  resolutions: Map<string, IMissingCategoryResolution>;
  setResolutions: React.Dispatch<
    React.SetStateAction<Map<string, IMissingCategoryResolution>>
  >;
  goBack: () => void;
  onSubmit: () => void;
  isSubmitting: boolean;
}

const CategoryMapping = ({
  template,
  resolutions,
  setResolutions,
  goBack,
  onSubmit,
  isSubmitting,
}: CategoryMappingProps): React.ReactNode => {
  const { t } = useTranslation();
  const { allTransactionCategories, isPending: areCategoriesPending } =
    useTransactionCategories();

  // Find all categories in template that do not exist in allTransactionCategories
  React.useEffect(() => {
    if (areCategoriesPending) {
      return;
    }

    const existingCategoryNames = new Set(
      allTransactionCategories.map((c) => c.value.toLowerCase()),
    );

    const initialResolutions = new Map<string, IMissingCategoryResolution>();

    const checkCategory = (
      cat: IBudgetTemplateCategoryItem,
      parentName: string | null,
    ) => {
      const lower = cat.name.toLowerCase();
      if (!existingCategoryNames.has(lower) && !initialResolutions.has(lower)) {
        initialResolutions.set(lower, {
          categoryName: cat.name,
          parentName,
          categoryType: cat.categoryType,
          icon: cat.icon ?? null,
          action: "create",
          mappedCategory: "",
        });
      }

      cat.subcategories?.forEach((sub) => checkCategory(sub, cat.name));
    };

    template.categories.forEach((cat) => checkCategory(cat, null));

    // Only update if resolutions wasn't initialized yet
    if (resolutions.size === 0 && initialResolutions.size > 0) {
      setResolutions(initialResolutions);
    }
  }, [
    allTransactionCategories,
    areCategoriesPending,
    template,
    resolutions.size,
    setResolutions,
  ]);

  const existingCategoryOptions = React.useMemo(() => {
    return allTransactionCategories
      .map((c) => ({
        value: c.value,
        label: c.parent ? `${c.parent} > ${c.value}` : c.value,
      }))
      .sort((a, b) => a.label.localeCompare(b.label));
  }, [allTransactionCategories]);

  const parentCategoryOptions = React.useMemo(
    () =>
      allTransactionCategories
        .filter((category) => !category.parent)
        .map((category) => ({ value: category.value, label: category.value }))
        .sort((a, b) => a.label.localeCompare(b.label)),
    [allTransactionCategories],
  );

  const missingList = Array.from(resolutions.values());

  const handleActionChange = (
    categoryName: string,
    action: CategoryResolutionAction,
  ) => {
    setResolutions((prev) => {
      const next = new Map(prev);
      const lower = categoryName.toLowerCase();
      const current = next.get(lower);
      if (current) {
        next.set(lower, { ...current, action });
      }
      return next;
    });
  };

  const handleMappedCategoryChange = (
    categoryName: string,
    mappedCategory: string,
  ) => {
    setResolutions((prev) => {
      const next = new Map(prev);
      const lower = categoryName.toLowerCase();
      const current = next.get(lower);
      if (current) {
        next.set(lower, { ...current, mappedCategory });
      }
      return next;
    });
  };

  const setAllActions = (action: CategoryResolutionAction) => {
    setResolutions((prev) => {
      const next = new Map(prev);
      next.forEach((item, key) => {
        next.set(key, { ...item, action });
      });
      return next;
    });
  };

  // Check if mapping is valid (any item with action="map" must have mappedCategory selected)
  const isMappingValid = missingList.every(
    (item) => item.action !== "map" || !!item.mappedCategory,
  );

  return (
    <Stack gap="1rem">
      {areCategoriesPending ? (
        <Skeleton h={280} />
      ) : missingList.length === 0 ? (
        <Alert
          icon={<CheckCircle2Icon size={16} />}
          title={t("budget_import_all_categories_exist_title")}
          color="success"
          variant="light"
        >
          {t("budget_import_all_categories_exist_message")}
        </Alert>
      ) : (
        <>
          <Alert
            icon={<AlertCircleIcon size={16} />}
            title={t("budget_import_missing_categories_title")}
            color="warning"
            variant="light"
          >
            {t("budget_import_missing_categories_description", {
              count: missingList.length,
            })}
          </Alert>
          <Group justify="space-between" align="center">
            <BodyText tone="secondary" size="xs">
              {t("budget_import_quick_actions")}:
            </BodyText>
            <Group gap="0.5rem">
              <Button
                variant="outline"
                color="primary"
                size="compact-xs"
                onClick={() => setAllActions("create")}
              >
                {t("budget_import_create_all")}
              </Button>
              <Button
                variant="outline"
                color="muted"
                size="compact-xs"
                onClick={() => setAllActions("skip")}
              >
                {t("budget_import_skip_all")}
              </Button>
            </Group>
          </Group>

          <ScrollArea h={280} type="auto">
            <Table striped highlightOnHover withTableBorder>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>{t("missing_category")}</Table.Th>
                  <Table.Th>{t("action")}</Table.Th>
                  <Table.Th>{t("destination")}</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {missingList.map((item) => (
                  <Table.Tr key={item.categoryName}>
                    <Table.Td>
                      <Group gap="0.35rem">
                        {item.icon ? (
                          <span>{item.icon}</span>
                        ) : (
                          <TagIcon size={16} aria-hidden="true" />
                        )}
                        <Stack gap={0}>
                          <BodyText size="xs" fw={500}>
                            {item.categoryName}
                          </BodyText>
                          {item.parentName && (
                            <BodyText tone="secondary" size="xs">
                              {t("parent")}: {item.parentName}
                            </BodyText>
                          )}
                        </Stack>
                      </Group>
                    </Table.Td>
                    <Table.Td>
                      <SegmentedControl
                        size="xs"
                        value={item.action}
                        onChange={(val) =>
                          handleActionChange(
                            item.categoryName,
                            val as CategoryResolutionAction,
                          )
                        }
                        data={[
                          {
                            label: t("create_new"),
                            value: "create",
                          },
                          {
                            label: t("map_existing"),
                            value: "map",
                          },
                          {
                            label: t("skip"),
                            value: "skip",
                          },
                        ]}
                        aria-label={t("select_action")}
                      />
                    </Table.Td>
                    <Table.Td>
                      {item.action === "map" ? (
                        <Select
                          size="xs"
                          placeholder={t("select_a_category")}
                          data={
                            item.parentName
                              ? existingCategoryOptions
                              : parentCategoryOptions
                          }
                          value={item.mappedCategory}
                          onChange={(val) =>
                            handleMappedCategoryChange(
                              item.categoryName,
                              val || "",
                            )
                          }
                          searchable
                          error={!item.mappedCategory}
                        />
                      ) : item.action === "create" ? (
                        <BodyText tone="primary" size="xs">
                          {t("will_be_created")}
                        </BodyText>
                      ) : (
                        <BodyText tone="secondary" size="xs">
                          {t("will_be_skipped")}
                        </BodyText>
                      )}
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </ScrollArea>
        </>
      )}
      <Group w="100%">
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          flex="1 1 auto"
          leftSection={<MoveLeftIcon size={16} />}
          onClick={goBack}
          disabled={isSubmitting}
        >
          {t("back")}
        </Button>
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          flex="1 1 auto"
          leftSection={<CheckIcon size={16} />}
          onClick={onSubmit}
          loading={isSubmitting}
          disabled={areCategoriesPending || !isMappingValid}
        >
          {t("import_budget")}
        </Button>
      </Group>
    </Stack>
  );
};

export default CategoryMapping;

import { Group, Stack } from "@mantine/core";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { ICategoryResponse } from "~/models/category";
import CustomCategoryCard from "./CustomCategoryCard/CustomCategoryCard";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { defaultGuid } from "~/models/applicationUser";

const CustomCategoryCards = () => {
  const { t } = useTranslation();
  const { allTransactionCategories, customTransactionCategories } =
    useTransactionCategories();

  if (customTransactionCategories.length === 0) {
    return (
      <Group justify="center" p="1rem">
        <DimmedText size="sm">{t("no_custom_categories")}</DimmedText>
      </Group>
    );
  }

  const customParents = customTransactionCategories.filter(
    (c) => c.parent === "",
  );
  const customChildren = customTransactionCategories.filter(
    (c) => c.parent !== "",
  );

  const childrenByParent = new Map<string, ICategoryResponse[]>();
  for (const child of customChildren) {
    const key = child.parent.toLowerCase();
    if (!childrenByParent.has(key)) childrenByParent.set(key, []);
    childrenByParent.get(key)!.push(child);
  }

  const customParentKeys = new Set(
    customParents.map((c) => c.value.toLowerCase()),
  );
  const builtInParentsNeeded = new Map<string, ICategoryResponse>();
  for (const child of customChildren) {
    const key = child.parent.toLowerCase();
    if (!customParentKeys.has(key)) {
      const builtIn = allTransactionCategories.find(
        (c) => c.id === defaultGuid && c.value.toLowerCase() === key,
      );
      if (builtIn && !builtInParentsNeeded.has(key)) {
        builtInParentsNeeded.set(key, builtIn);
      }
    }
  }

  type CategoryGroup = {
    parent: ICategoryResponse;
    isBuiltIn: boolean;
    children: ICategoryResponse[];
  };

  const groups: CategoryGroup[] = [
    ...customParents.map((p) => ({
      parent: p,
      isBuiltIn: false,
      children: childrenByParent.get(p.value.toLowerCase()) ?? [],
    })),
    ...[...builtInParentsNeeded.values()].map((p) => ({
      parent: p,
      isBuiltIn: true,
      children: childrenByParent.get(p.value.toLowerCase()) ?? [],
    })),
  ].sort((a, b) => a.parent.value.localeCompare(b.parent.value));

  return (
    <Stack gap="0.5rem">
      {groups.map((group) => (
        <Stack key={group.parent.value} align="center" gap="0.5rem">
          <CustomCategoryCard
            category={group.parent}
            isBuiltIn={group.isBuiltIn}
          />
          {group.children.map((child) => (
            <CustomCategoryCard key={child.id} category={child} isChildCard />
          ))}
        </Stack>
      ))}
    </Stack>
  );
};

export default CustomCategoryCards;

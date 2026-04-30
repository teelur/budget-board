import { Group, Skeleton, Stack } from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { ICategory, ICategoryResponse } from "~/models/category";
import { defaultTransactionCategories } from "~/models/transaction";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import CustomCategoryCard from "./CustomCategoryCard/CustomCategoryCard";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import { AxiosError } from "axios";

const CustomCategoryCards = () => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const transactionCategoriesQuery = useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async () => {
      const res = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategoryResponse[];
      }

      return undefined;
    },
  });

  const queryClient = useQueryClient();
  const doDeleteCategory = useMutation({
    mutationFn: async (guid: string) =>
      await request({
        url: "/api/transactionCategory",
        method: "DELETE",
        params: { guid },
      }),
    onSuccess: async () => {
      queryClient.invalidateQueries({ queryKey: ["transactionCategories"] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });

  if (transactionCategoriesQuery.isPending) {
    return <Skeleton height={46} radius="md" />;
  }

  if ((transactionCategoriesQuery.data ?? []).length === 0) {
    return (
      <Group justify="center" p="1rem">
        <DimmedText size="sm">{t("no_custom_categories")}</DimmedText>
      </Group>
    );
  }

  const buildCategoryCards = () => {
    const customCategories = transactionCategoriesQuery.data ?? [];

    const customParents = customCategories.filter((c) => c.parent === "");
    const customChildren = customCategories.filter((c) => c.parent !== "");

    // Map parent value (lowercase) → children
    const childrenByParent = new Map<string, ICategoryResponse[]>();
    for (const child of customChildren) {
      const key = child.parent.toLowerCase();
      if (!childrenByParent.has(key)) childrenByParent.set(key, []);
      childrenByParent.get(key)!.push(child);
    }

    // Find built-in parents that are parents of custom children
    const customParentKeys = new Set(
      customParents.map((c) => c.value.toLowerCase()),
    );
    const builtInParentsNeeded = new Map<string, ICategory>();
    for (const child of customChildren) {
      const key = child.parent.toLowerCase();
      if (!customParentKeys.has(key)) {
        const builtIn = defaultTransactionCategories.find(
          (c) => c.value.toLowerCase() === key,
        );
        if (builtIn && !builtInParentsNeeded.has(key)) {
          builtInParentsNeeded.set(key, builtIn);
        }
      }
    }

    type Group = {
      parent: ICategory;
      id: string | null;
      isBuiltIn: boolean;
      children: ICategoryResponse[];
    };

    const groups: Group[] = [
      ...customParents.map((p) => ({
        parent: p as ICategory,
        id: p.id,
        isBuiltIn: false,
        children: childrenByParent.get(p.value.toLowerCase()) ?? [],
      })),
      ...[...builtInParentsNeeded.values()].map((p) => ({
        parent: p,
        id: null,
        isBuiltIn: true,
        children: childrenByParent.get(p.value.toLowerCase()) ?? [],
      })),
    ].sort((a, b) => a.parent.value.localeCompare(b.parent.value));

    return groups.map((group) => (
      <Stack key={group.parent.value} align="center" gap="0.5rem">
        <CustomCategoryCard
          name={group.parent.value}
          isBuiltIn={group.isBuiltIn}
          deleteCategory={
            group.id != null
              ? async () => {
                  await doDeleteCategory.mutateAsync(group.id!);
                }
              : async () => {}
          }
        />
        {group.children.map((child) => (
          <CustomCategoryCard
            key={child.id}
            name={child.value}
            isChildCard
            deleteCategory={async () => {
              await doDeleteCategory.mutateAsync(child.id);
            }}
          />
        ))}
      </Stack>
    ));
  };

  return <Stack gap="0.5rem">{buildCategoryCards()}</Stack>;
};

export default CustomCategoryCards;

import { Group, Stack } from "@mantine/core";
import { useTranslation } from "react-i18next";
import React from "react";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { IAccountTypeResponse } from "~/models/accountType";
import { defaultGuid } from "~/models/applicationUser";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import CustomAccountTypeCard from "./CustomAccountTypeCard/CustomAccountTypeCard";
import { useUpdateAccountTypeMutation } from "~/hooks/mutations/accountTypes/useUpdateAccountTypeMutation";
import { useDeleteAccountTypeMutation } from "~/hooks/mutations/accountTypes/useDeleteAccountTypeMutation";

const CustomAccountTypeCards = (): React.ReactNode => {
  const { t } = useTranslation();
  const { allAccountTypes, customAccountTypes } = useAccountTypes();

  const updateAccountTypeMutation = useUpdateAccountTypeMutation();
  const deleteAccountTypeMutation = useDeleteAccountTypeMutation();

  if (customAccountTypes.length === 0) {
    return (
      <Group justify="center" p="1rem">
        <DimmedText size="sm">{t("no_custom_account_types")}</DimmedText>
      </Group>
    );
  }

  const customParents = customAccountTypes.filter((type) => type.parent === "");
  const customChildren = customAccountTypes.filter(
    (type) => type.parent !== "",
  );

  const childrenByParent = new Map<string, IAccountTypeResponse[]>();
  for (const child of customChildren) {
    const key = child.parent.toLowerCase();
    if (!childrenByParent.has(key)) childrenByParent.set(key, []);
    childrenByParent.get(key)!.push(child);
  }

  const customParentKeys = new Set(
    customParents.map((t) => t.value.toLowerCase()),
  );
  const builtInParentsNeeded = new Map<string, IAccountTypeResponse>();
  for (const child of customChildren) {
    const key = child.parent.toLowerCase();
    if (!customParentKeys.has(key)) {
      const builtIn = allAccountTypes.find(
        (t) => t.id === defaultGuid && t.value.toLowerCase() === key,
      );
      if (builtIn && !builtInParentsNeeded.has(key)) {
        builtInParentsNeeded.set(key, builtIn);
      }
    }
  }

  type TypeGroup = {
    parent: IAccountTypeResponse;
    isBuiltIn: boolean;
    children: IAccountTypeResponse[];
  };

  const groups: TypeGroup[] = [
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
        <Stack key={group.parent.value} align="flex-start" gap="0.5rem">
          <CustomAccountTypeCard
            accountType={group.parent}
            isBuiltIn={group.isBuiltIn}
            deleteAccountType={async () => {
              await deleteAccountTypeMutation.mutateAsync(group.parent.id);
            }}
            updateAccountType={async (req) => {
              await updateAccountTypeMutation.mutateAsync(req);
            }}
          />
          {group.children.map((child) => (
            <CustomAccountTypeCard
              key={child.id}
              accountType={child}
              isChildCard
              deleteAccountType={async () => {
                await deleteAccountTypeMutation.mutateAsync(child.id);
              }}
              updateAccountType={async (req) => {
                await updateAccountTypeMutation.mutateAsync(req);
              }}
            />
          ))}
        </Stack>
      ))}
    </Stack>
  );
};

export default CustomAccountTypeCards;

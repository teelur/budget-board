import { ActionIcon, Badge, Flex, Group } from "@mantine/core";
import { CornerDownRight, TrashIcon } from "lucide-react";
import React from "react";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";

interface CustomCategoryCardProps {
  name: string;
  isBuiltIn?: boolean;
  isChildCard?: boolean;
  deleteCategory: () => Promise<void>;
}

const CustomCategoryCard = (
  props: CustomCategoryCardProps,
): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Group w="100%" maw={600} wrap="nowrap">
      {props.isChildCard && <CornerDownRight />}
      <Card flex={1} p="0.25rem" elevation={1}>
        <Group justify="space-between">
          <Group gap="0.5rem">
            {props.isBuiltIn ? (
              <DimmedText size="sm">{props.name}</DimmedText>
            ) : (
              <PrimaryText size="sm">{props.name}</PrimaryText>
            )}
            {props.isBuiltIn && <Badge size="xs">{t("built_in")}</Badge>}
          </Group>
          <Flex justify="flex-end" flex="1 1 auto">
            {!props.isBuiltIn && (
              <ActionIcon
                size="sm"
                onClick={props.deleteCategory}
                bg="var(--button-color-destructive)"
              >
                <TrashIcon size="1rem" />
              </ActionIcon>
            )}
          </Flex>
        </Group>
      </Card>
    </Group>
  );
};

export default CustomCategoryCard;

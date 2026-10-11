import { Flex, Group } from "@mantine/core";
import { ActionIcon, BodyText } from "@teelur/budget-board-ui";
import { ChevronRightIcon, PencilIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import { getSubtypeOptions, NET_WORTH_CATEGORY_TYPES } from "~/helpers/widgets";
import { INetWorthWidgetCategory } from "~/models/widgetSettings";

interface NetWorthLineCategoryContentProps {
  category: INetWorthWidgetCategory;
  enableEdit: () => void;
}

const NetWorthLineCategoryContent = (
  props: NetWorthLineCategoryContentProps,
) => {
  const { t } = useTranslation();

  const getTypeDisplayString = (): string => {
    // This needs to be coerced to lowercase because pre-localization these were stored with a capitalized first letter.
    const type = NET_WORTH_CATEGORY_TYPES.find(
      (type) => type.value === props.category.type.toLowerCase(),
    );
    return type ? t(type.label) : props.category.type;
  };

  const getSubtypeDisplayString = (): string => {
    // This needs to be coerced to lowercase because pre-localization these were stored with a capitalized first letter.
    const subtypeOptions = getSubtypeOptions(props.category.type);
    const subtype = subtypeOptions.find(
      (subtype) => subtype.value === props.category.subtype.toLowerCase(),
    );
    return subtype ? t(subtype.label) : props.category.subtype;
  };

  return (
    <Flex
      direction={{ base: "column", xs: "row" }}
      align="center"
      justify="space-between"
    >
      <Group gap="0.25rem" align="center">
        {props.category.type.length > 0 ? (
          <BodyText tone="primary" size="sm">
            {getTypeDisplayString()}
          </BodyText>
        ) : (
          <BodyText tone="muted" size="sm">
            {t("no_type")}
          </BodyText>
        )}
        <ChevronRightIcon size={14} />
        {props.category.subtype.length > 0 ? (
          <BodyText tone="muted" size="sm">
            {getSubtypeDisplayString()}
          </BodyText>
        ) : (
          <BodyText tone="muted" size="sm">
            {t("no_subtype")}
          </BodyText>
        )}
        <ActionIcon
          variant="ghost"
          color="primary"
          size="compact-xs"
          onClick={props.enableEdit}
        >
          <PencilIcon size={14} />
        </ActionIcon>
      </Group>
      <BodyText tone="primary" size="sm">
        {props.category.value}
      </BodyText>
    </Flex>
  );
};

export default NetWorthLineCategoryContent;
